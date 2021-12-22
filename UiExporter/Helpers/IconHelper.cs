using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace UiExporter.Helpers
{
    internal class IconHelper
    {

        public static BitmapSource GetForegroundWindowIcon()
        {
            var hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            Process proc = Process.GetProcessById((int)pid);
            // modern apps run under ApplicationFrameHost host process in windows 10
            // don't forget to check if that is true for windows 8 - maybe they use another host there
            if (proc.MainModule.ModuleName == "ApplicationFrameHost.exe")
            {
                // this should be modern app
                return GetModernAppLogo(hwnd);
            }
            return GetWindowIcon(hwnd);
        }

        public static BitmapSource GetModernAppLogo(IntPtr hwnd)
        {
            // get folder where actual app resides
            var exePath = GetModernAppProcessPath(hwnd);
            var dir = System.IO.Path.GetDirectoryName(exePath);
            var manifestPath = System.IO.Path.Combine(dir, "AppxManifest.xml");
            if (File.Exists(manifestPath))
            {
                // this is manifest file
                string pathToLogo;
                using (var fs = File.OpenRead(manifestPath))
                {
                    var manifest = XDocument.Load(fs);
                    const string ns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
                    // rude parsing - take more care here
                    pathToLogo = manifest.Root.Element(XName.Get("Properties", ns)).Element(XName.Get("Logo", ns)).Value;
                }
                // now here it is tricky again - there are several files that match logo, for example
                // black, white, contrast white. Here we choose first, but you might do differently
                string finalLogo = null;
                // serach for all files that match file name in Logo element but with any suffix (like "Logo.black.png, Logo.white.png etc)
                foreach (var logoFile in Directory.GetFiles(System.IO.Path.Combine(dir, System.IO.Path.GetDirectoryName(pathToLogo)),
                    System.IO.Path.GetFileNameWithoutExtension(pathToLogo) + "*" + System.IO.Path.GetExtension(pathToLogo)))
                {
                    finalLogo = logoFile;
                    break;
                }

                if (System.IO.File.Exists(finalLogo))
                {
                    using (var fs = File.OpenRead(finalLogo))
                    {
                        var img = new BitmapImage()
                        {
                        };
                        img.BeginInit();
                        img.StreamSource = fs;
                        img.CacheOption = BitmapCacheOption.OnLoad;
                        img.EndInit();
                        return img;
                    }
                }
            }
            return null;
        }

        private static string GetModernAppProcessPath(IntPtr hwnd)
        {
            uint pid = 0;
            GetWindowThreadProcessId(hwnd, out pid);
            // now this is a bit tricky. Modern apps are hosted inside ApplicationFrameHost process, so we need to find
            // child window which does NOT belong to this process. This should be the process we need
            var children = GetChildWindows(hwnd);
            foreach (var childHwnd in children)
            {
                uint childPid = 0;
                GetWindowThreadProcessId(childHwnd, out childPid);
                if (childPid != pid)
                {
                    // here we are
                    Process childProc = Process.GetProcessById((int)childPid);
                    return childProc.MainModule.FileName;
                }
            }

            throw new Exception("Cannot find a path to Modern App executable file");
        }

        public static BitmapSource GetWindowIcon(IntPtr windowHandle)
        {
            var hIcon = default(IntPtr);
            hIcon = SendMessage(windowHandle, WM_GETICON, (IntPtr)ICON_BIG, IntPtr.Zero);

            if (hIcon == IntPtr.Zero)
                hIcon = GetClassLongPtr(windowHandle, GCL_HICON);

            if (hIcon == IntPtr.Zero)
            {
                hIcon = LoadIcon(IntPtr.Zero, (IntPtr)0x7F00 /*IDI_APPLICATION*/);
            }

            if (hIcon != IntPtr.Zero)
            {
                return Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            else
            {
                throw new InvalidOperationException("Could not load window icon.");
            }
        }

        #region Helper methods
        const UInt32 WM_GETICON = 0x007F;
        const int ICON_BIG = 1;
        const int GCL_HICON = -14;

        private static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }

        public delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);
        [DllImport("user32.Dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr parentHandle, EnumWindowProc callback, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out uint processId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        private static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size > 4)
                return GetClassLongPtr64(hWnd, nIndex);
            else
                return new IntPtr(GetClassLongPtr32(hWnd, nIndex));
        }

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        public static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        public static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);
        #endregion
    }
}
