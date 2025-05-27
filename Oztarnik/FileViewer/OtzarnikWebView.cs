using Microsoft.Web.WebView2.Core;
using Oztarnik.Main;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WebViewLib;
using WpfLib;
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
            WebView.WebMessageReceived += WebView_WebMessageReceived;
            ThemeHelper.StaticPropertyChanged += ThemeHelper_StaticPropertyChanged;
            Settings.StaticPropertyChanged += Settings_StaticPropertyChanged;
        }

        private async void Settings_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.DoNotChangeDocumentColors))
            {
                await ExecuteScriptAsync($@"document.body.style.color = ""{ThemeHelper.ForeGround.ToRgbString()}"";");
                await ExecuteScriptAsync($@" document.body.style.backgroundColor = ""{ThemeHelper.BackGround.ToRgbString()}"";");
            }
        }

        private async void ThemeHelper_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Settings.DoNotChangeDocumentColors)
                return;

            if (e.PropertyName == nameof(ThemeHelper.ForeGround))
                await ExecuteScriptAsync($@" document.body.style.color = ""{ThemeHelper.ForeGround.ToRgbString()}"";");
            else if (e.PropertyName == nameof(ThemeHelper.BackGround))
                await ExecuteScriptAsync($@"document.body.style.backgroundColor = ""{ThemeHelper.BackGround.ToRgbString()}"";");
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

        public void LoadDocument(string content, string scrollIndex)
        {
            var htmlDoc = HtmlBuilder.HtmlDoc(content, scrollIndex);
            DocumentWrite(htmlDoc);
        }

        public void ShowFavorites() =>
          WpfLib.Helpers.DependencyHelper.FindParent<OtzarnikView>(this)?.ShowFavorites();

        public void OpenFile() =>
            WpfLib.Helpers.DependencyHelper.FindParent<OtzarnikView>(this)?.OpenFile();

        public void CloseCurrentTab() =>
             WpfLib.Helpers.DependencyHelper.FindParent<OtzarnikView>(this)?.CloseCurrentTab();

        public void CloseAllTabs() =>
            WpfLib.Helpers.DependencyHelper.FindParent<OtzarnikView>(this)?.CloseAllTabs();

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

