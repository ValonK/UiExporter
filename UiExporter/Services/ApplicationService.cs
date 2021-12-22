using System;
using System.Collections.Generic;
using System.Diagnostics;
using UiExporter.Exceptions;
using UiExporter.Helpers;
using UiExporter.Models;
using UiExporter.Views;

namespace UiExporter.Services
{
    internal class ApplicationService : IApplicationService
    {
        public event EventHandler<Application> ApplicationSelected;

        public void SelectApplicationAsync()
        {
            var guiProcesses = GetGuiProcesses();
            if(guiProcesses == null)
            {
                throw new NoGuiApplicationsException();
            }

            var applications = MapToApplications(guiProcesses);

            var dialogView = new SelectAppView(applications);
            dialogView.ApplicationSelected += DialogView_ApplicationSelected;
            dialogView.ShowDialog();
        }

        private void DialogView_ApplicationSelected(object? sender, Application e)
        {
            ApplicationSelected?.Invoke(this, e);
        }

        private IList<Process> GetGuiProcesses()
        {
            var list = new List<Process>(); 
            foreach (var proc in Process.GetProcesses())
            {
                if (proc.MainWindowHandle != IntPtr.Zero)
                {
                     list.Add(proc);
                }
            }
            return list;
        }

        private IList<Application> MapToApplications(IList<Process> processes)
        {
            var applications = new List<Application>();
            foreach(var process in processes)
            {
                try
                {
                    var app = new Application
                    {
                        Name = process.ProcessName,
                        Pid = process.Id.ToString(),
                        Hwnd = process.MainWindowHandle,
                        Location = process?.MainModule?.FileName,
                    };


                    app.Icon = IconHelper.GetWindowIcon(app.Hwnd);
                    applications.Add(app);

                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    System.IO.File.WriteAllText("log.txt",e.ToString());
                }
            }

            return applications;
        }

    }
}
