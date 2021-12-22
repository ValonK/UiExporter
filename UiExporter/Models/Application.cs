using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace UiExporter.Models
{
    public class Application
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Pid { get; set; }
        public string Location { get; set; }
        public BitmapSource Icon { get; set; }
        public IntPtr Hwnd { get; set; }
    }
}
