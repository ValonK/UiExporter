using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace UiExporter.Models
{
    internal class Element
    {
        public ControlType ControlType { get; set; }

        public string Name { get; set; }

        public IList<string> Data { get; set; }

        public bool IsChecked { get; set; }
    }

    
}
