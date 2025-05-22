using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace Oztarnik.FileViewer
{
    public static class HtmlBuilder
    {
        public static string HtmlDoc(string content)
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
                        {content}
                        {Js()}
                  </body>
                </html>";
        }

        public static string css()
        {
            return @"
                body {line-height: 120%; text-align: justify;}
                line { display: block; }
                header { margin-top: 10px; margin-bottom: 10px;  color:#000066;}
                h1,h2,h3,h4,h5,h6,h7,h8,h9 {  color:#000066;}
                ot { color:#000066; }
            ";
        }
        public static string Js()
        {
            return $@"<script>
            let zoomLevel = 1;
            let originalText = document.body.innerHTML;
            let isVowelsReversed = false;
            let isCantillationReversed = false;
            let isInline = false;
        
            function navigateToLine(lineNumberString) {{
                const lineNumber = parseInt(lineNumberString);
                const lines = document.querySelectorAll('line');

                if (isNaN(lineNumber) || lineNumber < 1 || lineNumber > lines.length) {{
                    console.log('Invalid line number');
                    return;
                }}

                const targetLine = lines[lineNumber];
                targetLine.scrollIntoView({{ block: 'center'}});
                
                const originalBackgroundColor = targetLine.style.backgroundColor;
                targetLine.style.backgroundColor = 'rgb(243, 240, 235)'; // Set highlight color

                setTimeout(() => {{
                    targetLine.style.backgroundColor = originalBackgroundColor || ''; // Restore original color
                }}, 2000); // Highlight duration: 2000ms (2 seconds)
            }};

            function toggleInline()
            {{
                const lines = document.querySelectorAll('line');
                isInline = !isInline;
                lines.forEach(line => {{
                    line.style.display = isInline ? 'inline' : 'block';
                }});
            }};
            {TitleBarJs()}

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

        static string TitleBarJs()
        {
            return @"

const headings = Array.from(document.querySelectorAll('h1, h2, h3, h4, h5, h6'));
let lastScrollTop = window.scrollY;
let currentHeaderIndex = -1;

function scrollToHeader(index) {
  if (index >= 0 && index < headings.length) {
    headings[index].scrollIntoView({ behavior: 'smooth', block: 'start' });
    currentHeaderIndex = index;
}
}

function scrollToNextHeader() {
  if (currentHeaderIndex < headings.length - 1) {
    scrollToHeader(currentHeaderIndex + 1);
  }
}

function scrollToPreviousHeader() {
  if (currentHeaderIndex > 0) {
    scrollToHeader(currentHeaderIndex - 1);
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
    window.chrome.webview.postMessage({
      action: ""set"",
      target: ""CurrentTitle"",
      value: headings[newIndex].textContent
    });
  }
}

window.addEventListener('scroll', updateTitle);
window.addEventListener('load', updateTitle);

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
