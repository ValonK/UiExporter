using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiExporter.Models;

namespace UiExporter.Services
{
    internal interface IApplicationService
    {
        void SelectApplicationAsync();

        event EventHandler<Models.Application> ApplicationSelected;
    }
}
