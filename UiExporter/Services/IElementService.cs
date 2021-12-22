using System.Collections.Generic;

namespace UiExporter.Services
{
    internal interface IElementService
    {
        IList<Models.Element> Analyze(Models.Application application);
        string GetApplicationInfo(Models.Application application);
    }
}
