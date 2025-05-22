using Microsoft.Web.WebView2.Core;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using WebViewLib;
using WpfLib;
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
                    JsonElement valueElement = json.GetProperty("value");
                    object value = JsonSerializer.Deserialize(valueElement.GetRawText(), property.PropertyType);
                    property.SetValue(targetObject, value);
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




        public void LoadDocument(string content)
        {
            var htmlDoc = HtmlBuilder.HtmlDoc(content);
            DocumentWrite(htmlDoc);
        }

        async void ToggleInline()
        {
            await ExcuteScriptAsync($"toggleInline();");
        }
        async void ToggleNikud()
        {
            await ExcuteScriptAsync($@"
                var newText = originalText;
                if (!isVowelsReversed)
                {{
                    newText = newText.replace(/[\u05B0-\u05BD\u05C1\u05C2\u05C4\u05C5,;?!.:]/g, """");
                }}           
                if (isCantillationReversed)
                {{
                    newText = newText.replace(/[\u0591-\u05AF]/g, """");
                }}   

                document.body.innerHTML = newText
                isVowelsReversed = !isVowelsReversed;");
        }

        async void ToggleCantillations()
        {
            await ExcuteScriptAsync($@"
                var newText = originalText;
                if (!isCantillationReversed)
                {{
                    newText = newText.replace(/[\u0591-\u05AF]/g, """");
                }}
                if (isVowelsReversed)
                {{
                    newText = newText.replace(/[\u05B0-\u05BD\u05C1\u05C2\u05C4\u05C5]/g, """");
                }}          
                document.body.innerHTML = newText
                isCantillationReversed = !isCantillationReversed;");
        }

        public async void NavigateToLine(int lineNumber) =>
            await ExcuteScriptAsync($"navigateToLine('{lineNumber.ToString()}')");

        async void ScrollToNextHeader() =>
            await ExcuteScriptAsync($"scrollToNextHeader()");

        async void ScrollToPreviousHeader() =>
            await ExcuteScriptAsync($"scrollToPreviousHeader()");

        async void ZoomIn() =>
          await ExcuteScriptAsync($"zoomIn()");

        async void ZoomOut() =>
            await ExcuteScriptAsync($"zoomOut()");
    }
}

