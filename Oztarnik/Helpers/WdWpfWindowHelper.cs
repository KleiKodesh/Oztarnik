using Microsoft.VisualBasic;
using Oztarnik.Main;
using System.Diagnostics;
using System.Windows.Interop;
using System;
using Microsoft.Office.Interop.Word;

namespace Otzarnik.Helpers
{
    public static class WdWpfWindowHelper
    {
        public static Application Application;
        public static void SetWordWindowOwner(System.Windows.Window window, bool removeContent = false)
        {
            if (Application == null) return;

            try
            {
                object content = null;  // optional remove window content if nessecary for perfomance isssues
                if (removeContent && content != null)
                {
                    content = window.Content;
                    window.Content = null;
                }

                IntPtr wordWindowHandle = IntPtr.Zero;

                var activeWindow = Application.ActiveWindow;
                wordWindowHandle = new IntPtr(activeWindow.Hwnd);

                WindowInteropHelper helper = new WindowInteropHelper(window);
                helper.Owner = wordWindowHandle;

                if (removeContent && content != null) { window.Content = content; }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SetOwner: {ex.Message}");
            }
        }
    }
}
