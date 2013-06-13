using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Media;
using Umbraco.UmbracoStudio.ToolWindows;

namespace Umbraco.UmbracoStudio.Helpers
{
    public class VsTheming
    {
        public static SolidColorBrush GetCommandBackground()
        {
            int color = (int)__VSSYSCOLOREX.VSCOLOR_COMMANDBAR_GRADIENT_BEGIN;
            return SolidColorBrushFromWin32Color(GetWin32Color(color));
        }

        public static SolidColorBrush GetWindowBackground()
        {
            return new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
        }

        public static SolidColorBrush GetWindowText()
        {
            int color = (int)__VSSYSCOLOREX3.VSCOLOR_WINDOWTEXT;
            return SolidColorBrushFromWin32Color(GetWin32Color(color));
        }

        public static SolidColorBrush GetToolbarSeparatorBackground()
        {
            int color = (int)__VSSYSCOLOREX3.VSCOLOR_COMMANDBAR_TOOLBAR_SEPARATOR;
            return SolidColorBrushFromWin32Color(GetWin32Color(color));
        }

        public static SolidColorBrush GetToolWindowBackground()
        {
            int color = (int)__VSSYSCOLOREX3.VSCOLOR_WINDOW;
            return SolidColorBrushFromWin32Color(GetWin32Color(color));
        }

        private static uint GetWin32Color(int color)
        {
            uint win32Color;
            var shell = ExplorerControl.Package.GetServiceHelper(typeof(SVsUIShell)) as IVsUIShell2;
            shell.GetVSSysColorEx(color, out win32Color);
            return win32Color;
        }

        private static SolidColorBrush SolidColorBrushFromWin32Color(uint win32Color)
        {
            byte[] bytes = BitConverter.GetBytes(win32Color);
            return new SolidColorBrush(Color.FromArgb(0xFF, bytes[0], bytes[1], bytes[2]));
        }
    }
}