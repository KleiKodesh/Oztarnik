using Microsoft.VisualBasic;
using Oztarnik.Main;
using System;
using System.Windows.Input;
using WpfLib.Helpers;

namespace Oztarnik.FileViewer
{
    public static class HtmlBuilder
    {
        public static string HtmlDoc(string content, string scrollIndex)
        {
            return $@"<!DOCTYPE html>
                <html lang=""he"" dir=""auto"">
                    <head>
                        <meta charset=UTF-8>
                        <style>
                            {css()}
                        </style>
                    </head>            
                      <body dir=""auto"">
                        <div id=""title-bar""></div>
                            <div id=""content"">
                        {content}
                            </div>
                        {Js(scrollIndex)}
                  </body>
                </html>";
        }

        public static string css()
        {
            string color = Settings.DoNotChangeDocumentColors ? "" : ThemeHelper.ForeGround.ToRgbString();
            string backGround = Settings.DoNotChangeDocumentColors ? "" : ThemeHelper.BackGround.ToRgbString();

            string fontFamily = Interaction.GetSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "DeafultFont", "Times New Roman");
            string fontSize = Interaction.GetSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "DefaultFontSize", "16");

            return $@"
                body {{line-height: 1.3; text-align: justify; font-family: '{fontFamily}'; font-size: {fontSize}px; 
                       color:{color}; background-color:{backGround};}}
                line {{ display: block; }}
                header {{ margin-top: 10px; margin-bottom: 10px;  color:#000066; }}
                h1,h2,h3,h4,h5,h6 {{ opacity: 0.75; }}
                ot {{ color:#000066; }}
                h1 {{ font-size: 200%; /* 32px */ }}  
                h2 {{ font-size: 175%; /* 28px */ }}
                h3 {{ font-size: 150%; /* 24px */ }}
                h4 {{ font-size: 125%; /* 20px */ }}
                h5 {{ font-size: 112.5%; /* 18px */ }}
                h6 {{font-size: 100%; /* 16px */  }}
                #title-bar {{ position: fixed; left: 0; top: 0; font-size:70%; opacity:0.8; background: #333; color: white; writing-mode: vertical-rl; text-orientation: mixed; padding: 3px 0; text-align: center; transform: rotate(180deg); }}
            ";
        }

       
        public static string Js(string scrollIndex, string targetHeaderIndex = "")
        {
            return $@"<script>
            window.addEventListener('DOMContentLoaded', () => {{
                const scrollindex = parseFloat('{scrollIndex}');
                if (!isNaN(scrollindex) && scrollindex >= 0) {{
                    window.scrollTo(0, scrollindex);
                }}

                const headerindex = parseInt('{targetHeaderIndex}');
                if (!isNaN(headerindex) && headerindex >= 0) {{
                    scrollToHeader(headerindex);
                }}
            }});


            let zoomLevel = 1;
            let isInline = false;

            {KeyDownJs()}
            {LinesJs()}
            {TitleBarJs()}
            {DiactrictsJs()}

             function zoomIn() {{
                zoomLevel += 0.1;
                document.body.style.zoom = zoomLevel;
              }}

              function zoomOut() {{
                zoomLevel -= 0.1;
                document.body.style.zoom = zoomLevel;
              }}
            </script>";
        }

        static string LinesJs()
        {
            return @"
                function toggleInline()
                {
                    const lines = document.querySelectorAll('line');
                    isInline = !isInline;
                    lines.forEach(line => {
                        line.style.display = isInline ? 'inline' : 'block';
                    });
                };

                function navigateToLine(lineNumberString) {
                    const lineNumber = parseInt(lineNumberString);
                    const lines = document.querySelectorAll('line');

                    if (isNaN(lineNumber) || lineNumber < 1 || lineNumber > lines.length) {
                        console.log('Invalid line number');
                        return;
                    }

                    const targetLine = lines[lineNumber];
                    targetLine.scrollIntoView({ block: 'center'});
                     updateTitle();
                    
                    const originalBackgroundColor = targetLine.style.backgroundColor;
                    targetLine.style.backgroundColor = 'rgb(243, 240, 235)'; // Set highlight color

                    setTimeout(() => {
                        targetLine.style.backgroundColor = originalBackgroundColor || ''; // Restore original color
                    }, 2000); // Highlight duration: 2000ms (2 seconds)
                };
";
        }

        static string TitleBarJs()
        {
            return @"
const titleBar = document.getElementById('title-bar');
const headings = Array.from(document.querySelectorAll('h1, h2, h3, h4, h5, h6'));
let lastScrollTop = window.scrollY;
let currentHeaderIndex = -1;

function scrollToHeader(index) {
  if (index >= 0 && index < headings.length) {
    headings[index].scrollIntoView({ behavior: 'smooth', block: 'start' });
    currentHeaderIndex = index;
    setTitle();
  }
}

function scrollToNextHeader() {
  if (currentHeaderIndex < headings.length - 1) {
    currentHeaderIndex += 1;
    scrollToHeader(currentHeaderIndex);
  }
}

function scrollToPreviousHeader() {
  if (currentHeaderIndex > 0) {
    currentHeaderIndex -= 1;
    scrollToHeader(currentHeaderIndex);
  }
}

function updateTitle() {
  const scrollTop = window.scrollY;
  const scrollDown = scrollTop > lastScrollTop;
  lastScrollTop = scrollTop;

  let newIndex = currentHeaderIndex;

  const visible = headings.map((h, i) => {
    const rect = h.getBoundingClientRect();
    return { index: i, top: rect.top, bottom: rect.bottom };
  }).filter(pos => pos.top < window.innerHeight && pos.bottom > 0);

  if (scrollDown) {
    for (let i = 0; i < headings.length; i++) {
      const rect = headings[i].getBoundingClientRect();
      if (rect.top >= 0 && rect.top < 150) {
        newIndex = i;
        break;
      }
    }
  } else {
    if (visible.length > 0) {
      newIndex = visible[0].index;
    } else {
      for (let i = headings.length - 1; i >= 0; i--) {
        const rect = headings[i].getBoundingClientRect();
        if (rect.bottom < 0) {
          newIndex = i;
          break;
        }
      }
    }
  }

  if (newIndex !== currentHeaderIndex && headings[newIndex]) {
    currentHeaderIndex = newIndex;
    setTitle();
  }
}

function setTitle() {
  if (currentHeaderIndex >= 0 && headings[currentHeaderIndex]) {
    const title = headings[currentHeaderIndex].textContent;
    titleBar.textContent = title;

    // תקשורת עם WebView
    window.chrome.webview.postMessage({
      action: ""set"",
      target: ""CurrentTitle"",
      value: title
    });
  }
}

window.addEventListener('scroll', updateTitle);
window.addEventListener('load', updateTitle);


                ";
        }

        static string DiactrictsJs()
        {
            return $@"
        let isVowelsReversed = false;
        let isCantillationReversed = false;
        const contentElement = document.getElementById(""content"");

        // Map to store original text content of each node
        const originalTexts = new Map();

        // Initialize map with original text content of text nodes
        function initializeOriginalTexts() {{
            const walker = document.createTreeWalker(
                contentElement,
                NodeFilter.SHOW_TEXT,
                null,
                false
            );

            while (walker.nextNode()) {{
                originalTexts.set(walker.currentNode, walker.currentNode.nodeValue);
            }}
        }}

        // Function to toggle diacritics
        function toggleDiactricts() {{
            const walker = document.createTreeWalker(
                contentElement,
                NodeFilter.SHOW_TEXT,
                null,
                false
            );

            while (walker.nextNode()) {{
                let node = walker.currentNode;
                let text = originalTexts.get(node); // Get original text

                if (isVowelsReversed) {{
                    text = text.replace(/[\u05B0-\u05BD\u05C1\u05C2\u05C4\u05C5,;?!.:]/g, """");
                }}

                if (isCantillationReversed) {{
                    text = text.replace(/[\u0591-\u05AF]/g, """");
                }}

                node.nodeValue = text;
            }}
        }}

        function toggleNikud() {{
            isVowelsReversed = !isVowelsReversed;
            toggleDiactricts();
        }}

        function toggleCantillations() {{
            isCantillationReversed = !isCantillationReversed;
            toggleDiactricts();
        }}

        // Initialize original texts on load
        initializeOriginalTexts();
    ";
        }

        static string KeyDownJs()
        {
            return $@"
            window.addEventListener('keydown', (event) => {{
      // Check if the Control key is pressed along with another key
      if (event.ctrlKey) {{
        let message = null;

        switch (event.key.toLowerCase()) {{
          case 'o': // Ctrl + O
            message = {{
              action: ""call"",
              target: ""OpenFile""
            }};
            break;

          case 'w': // Ctrl + W
            message = {{
              action: ""call"",
              target: ""CloseCurrentTab""
            }};
            break;

          case 'x': // Ctrl + X
            message = {{
              action: ""call"",
              target: ""CloseAllTabs""
            }};
            break;

          case 'h': // Ctrl + H
            message = {{
              action: ""call"",
              target: ""ShowFavorites""
            }};
            break;
        }}

        if (message) {{
          // Send the message to WebView
          window.chrome.webview.postMessage(message);
          event.preventDefault(); // Prevent default behavior (e.g., browser shortcuts)
        }}
      }}
    }});
";
        }

    }
}


//if (closest)
//{
//    window.chrome.webview.postMessage({
//    action: ""set"",
//      target: ""CurrentTitle"",
//      value: closest.textContent
//    });
//}
//  }


//document.addEventListener('keydown', function(event) {
//    if (event.ctrlKey && event.key.toLowerCase() === 'o') {
//        event.preventDefault();  // Stop the browser's open file dialog
//        alert('Ctrl + O detected!');
//    }
//});


