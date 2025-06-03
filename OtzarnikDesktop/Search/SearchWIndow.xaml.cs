using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace Otzarnik.Search
{
    public partial class SearchWindow : Window
    {
        public SearchWindow()
        {
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedIndex > -1
                && listView.SelectedItem is ResultModel result)
            {
                //string content = File.ReadAllText(result.FilePath);

                ////הוספת סימון לפי אינדקס
                //if (result.MatchIndex >= 0 && result.MatchIndex + result.MatchValue.Length <= content.Length)
                //{
                //    content = content.Insert(result.MatchIndex + result.MatchValue.Length, "</mark>");
                //    content = content.Insert(result.MatchIndex, "<mark id=\"match\">");
                //}

                // עטיפת HTML עם סקריפט שמריץ scrollIntoView
                string htmlWrapper = $@"
<html>
<head>
    <meta charset='utf-8'>
 <style>
        /* Fullscreen overlay with spinner */
        #spinner-overlay {{
            position: fixed;
            top: 0; left: 0; right: 0; bottom: 0;
            background: rgba(255, 255, 255, 0.9);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 9999;
            font-size: 24px;
            color: #333;
            user-select: none;
        }}

 .loader{{
        display: block;
        position: relative;
        height: 12px;
        width: 80%;
        border: 1px solid #fff;
        border-radius: 10px;
        overflow: hidden;
      }}
      .loader:after{{
        content: '';
        position: absolute;
        left: 0;
        top: 0;
        height: 100%;
        width: 0;
        background: #FF3D00;
        animation: 6s prog ease-in infinite;
      }}
      
      @keyframes prog {{
        to  {{   width: 100%;}}
      }}
      

    </style>

    <script>
      (function () {{
    function scrollToMatch() {{
        var el = document.getElementById('match');
        if (el) {{
            el.scrollIntoView({{ block: 'center' }});

            const spinner = document.getElementById('spinner-overlay');
            if (spinner) spinner.style.display = 'none';

            return true;
              
        }}
        return false;
    }}

    if (!scrollToMatch()) {{
        var observer = new MutationObserver(function () {{
            if (scrollToMatch()) observer.disconnect();
        }});
        observer.observe(document.documentElement, {{ childList: true, subtree: true }});
    }}
}})();
    </script>
</head>
<body dir='auto'>
    <div id=""spinner-overlay"">
      <span class=""loader""></span>
    </div>
</body>
</html>
";

                string tempFilePath = Path.Combine(Path.GetTempPath(), "OtarnikTemp.html");
                File.WriteAllText(tempFilePath, htmlWrapper);

                webView.Source = new Uri("about:blank");
                webView.Source = new Uri(tempFilePath);

                listView.SelectedIndex = -1;
            }
        }
    }
}
