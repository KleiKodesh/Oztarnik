using Microsoft.Web.WebView2.Core;
using Oztarnik.AppData;
using Oztarnik.Main;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebViewLib;
using WpfLib.Helpers;
using WpfLib.ViewModels;

namespace Oztarnik.FileViewer
{
    public class OtzarnikWebView : WebViewHost
    {
        private string _currentTitle;
        public string CurrentTitle { get => _currentTitle;  set {   if (_currentTitle != value) { _currentTitle = value; OnPropertyChanged(nameof(CurrentTitle)); } } }
        public RelayCommand ToggleInlineCommand => new RelayCommand(ToggleInline);
        public RelayCommand ToggleCantillationsCommand => new RelayCommand(ToggleCantillations);
        public RelayCommand ToggleNikudCommand => new RelayCommand(ToggleNikud);
        public RelayCommand ScrollToNextHeaderCommand => new RelayCommand(ScrollToNextHeader);
        public RelayCommand ScrollToPreviousHeaderCommand => new RelayCommand(ScrollToPreviousHeader);
        public RelayCommand ZoomInCommand => new RelayCommand(ZoomIn);
        public RelayCommand ZoomOutCommand => new RelayCommand(ZoomOut);

        public OtzarnikWebView()
        {
            WebView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            WebView.WebMessageReceived += WebView_WebMessageReceived;
            ThemeHelper.StaticPropertyChanged += ThemeHelper_StaticPropertyChanged;
            Settings.StaticPropertyChanged += Settings_StaticPropertyChanged;
        }

        private void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            WebView.CoreWebView2.ContextMenuRequested += CoreWebView2_ContextMenuRequested;
        }

        private void CoreWebView2_ContextMenuRequested(object sender, CoreWebView2ContextMenuRequestedEventArgs e)
        {
            var items = e.MenuItems.ToList();

            for (int i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (Regex.IsMatch(item.Label, @"(refresh)|(reload)|(רי?ענו?ן)", RegexOptions.IgnoreCase)) // Adjust label based on your locale
                {
                    e.MenuItems.RemoveAt(i);
                }
            }
        }

        private async void Settings_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.DoNotChangeDocumentColors))
            {
                if (Settings.DoNotChangeDocumentColors)
                {
                    await ExecuteScriptAsync($@"document.body.style.color = ""black"";");
                    await ExecuteScriptAsync($@" document.body.style.backgroundColor = ""white"";");
                }
                else
                {
                    await ExecuteScriptAsync($@"document.body.style.color = ""{ThemeHelper.Foreground.ToRgbString()}"";");
                    await ExecuteScriptAsync($@" document.body.style.backgroundColor = ""{ThemeHelper.Background.ToRgbString()}"";");
                }
            }
        }

        private async void ThemeHelper_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Settings.DoNotChangeDocumentColors)
                return;

            if (e.PropertyName == nameof(ThemeHelper.Foreground))
                await ExecuteScriptAsync($@" document.body.style.color = ""{ThemeHelper.Foreground.ToRgbString()}"";");
            else if (e.PropertyName == nameof(ThemeHelper.Background))
                await ExecuteScriptAsync($@"document.body.style.backgroundColor = ""{ThemeHelper.Background.ToRgbString()}"";");
        }




        //{ "action": "set", "target": "SomeProperty", "value": "SomeValue"}
        //{  "action": "call", "target": "SomeMethod"}
        private void WebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var json = JsonSerializer.Deserialize<JsonElement>(e.WebMessageAsJson);

            string action = json.GetProperty("action").GetString();
            string target = json.GetProperty("target").GetString();

            var targetObject = this; // or replace with the object you want to reflect on

            if (action == "set")
            {
                var property = targetObject.GetType().GetProperty(target, BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanWrite)
                {
                    var valueText = json.GetProperty("value").GetRawText();
                    //object value = JsonSerializer.Deserialize(valueText, property.PropertyType);
                    property.SetValue(targetObject, valueText);
                    return;
                }
                else
                {
                    Console.WriteLine($"Property '{target}' not found or not writable.");
                }
            }
            else if (action == "call")
            {
                var method = targetObject.GetType().GetMethod(target, BindingFlags.Public | BindingFlags.Instance);
                if (method != null && method.GetParameters().Length == 0)
                {
                    method.Invoke(targetObject, null);
                }
                else
                {
                    Console.WriteLine($"Method '{target}' not found or requires parameters.");
                }
            }
            else
            {
                Console.WriteLine("Unsupported action.");
            }
        }

        public void LoadDocument(string content, string scrollIndex, bool scrollToMatch)
        {
            var htmlDoc = HtmlBuilder.HtmlDoc(content, scrollIndex, scrollToMatch);
            DocumentWrite(htmlDoc);
        }

        public void ShowFavorites() =>
          DependencyHelper.FindParent<OtzarnikView>(this)?.ShowFavorites();

        public void OpenFile() =>
            DependencyHelper.FindParent<OtzarnikView>(this)?.OpenFile();

        public void CloseCurrentTab() =>
             DependencyHelper.FindParent<OtzarnikView>(this)?.CloseCurrentTab();

        public void CloseAllTabs() =>
            DependencyHelper.FindParent<OtzarnikView>(this)?.CloseAllTabs();

        public async Task<string> GetScrollIndex() => 
            await ExecuteScriptAsync("window.scrollY");

        public async Task<string> GetCurrentHeaderIndex() =>
           await ExecuteScriptAsync("currentHeaderIndex");

        public async Task<string> GetCurrentHeaderValue() =>
          await ExecuteScriptAsync("currentHeaderIndex");

        async void ToggleInline() =>
            await ExecuteScriptAsync($"toggleInline()");

        async void ToggleNikud() =>
            await ExecuteScriptAsync("toggleNikud()");

        async void ToggleCantillations() =>
            await ExecuteScriptAsync("toggleCantillations()");

        public async void NavigateToLine(int lineNumber) =>
            await ExecuteScriptAsync($"navigateToLine('{lineNumber.ToString()}')");

        async void ScrollToNextHeader() =>
            await ExecuteScriptAsync($"scrollToNextHeader()");

        async void ScrollToPreviousHeader() =>
            await ExecuteScriptAsync($"scrollToPreviousHeader()");

        async void ZoomIn() =>
          await ExecuteScriptAsync($"zoomIn()");

        async void ZoomOut() =>
            await ExecuteScriptAsync($"zoomOut()");
    }
}

