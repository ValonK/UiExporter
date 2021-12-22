using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using UiExporter.Exceptions;
using UiExporter.Helpers;
using UiExporter.Models;
using UiExporter.Services;
using UiExporter.Views;
using WPFUI.Common;
using Application = UiExporter.Models.Application;

namespace UiExporter.ViewModels
{
    internal class MainViewModel : BaseViewModel
    {
        #region Fields

        private IApplicationService _applicationService;
        private IElementService _elementService;
        private Application selectedApplication;
        private ObservableCollection<Element> _elements;
        private bool _elementsFound; 
        #endregion

        public MainViewModel()
        {
            _applicationService = new ApplicationService();
            _elementService = new ElementService();
            SelectCommand = new RelayCommand<object>(OnSelect);
            AnalyzeCommand = new RelayCommand(OnAnalyze);
            ExportCommad = new RelayCommand(OnExport);
            InfoCommand = new RelayCommand(OnInfo);
        }


        #region Properties

        public Application SelectedApplication
        {
            get => selectedApplication;
            set => SetProperty(ref selectedApplication, value);
        }

        public ObservableCollection<Element> Elements
        {
            get => _elements;
            set => SetProperty(ref _elements, value);
        }

        public bool ElementsFound
        {
            get => _elementsFound;
            set => SetProperty(ref _elementsFound, value);
        } 
        #endregion

        #region Commands

        public RelayCommand AnalyzeCommand { get; set; }
        public RelayCommand ExportCommad { get; set; }
        public RelayCommand InfoCommand { get; set; }
        public RelayCommand<object> SelectCommand { get; set; } 
        #endregion

        private void OnSelect(object obj)
        {
            _applicationService.ApplicationSelected += ApplicationService_ApplicationSelected;
            _applicationService.SelectApplicationAsync();
        }

        private async void OnAnalyze()
        {
            ElementsFound = false;
            await AnalyzeAsync();
        }


        private async Task AnalyzeAsync()
        {

            IsBusy = true;
            try
            {
                IList<Element> elements = null;

                await Task.Run(() =>
                {
                    elements = _elementService.Analyze(SelectedApplication);
                });

                if (elements == null || elements?.Count == 0)
                {
                    MessageBox.Show($"No Ui Elements found for {SelectedApplication.Name}", "Information", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                ElementsFound = true;
                Elements = new ObservableCollection<Element>(elements);
            }
            catch (HwndZeroException)
            {
                MessageBox.Show("Could not analyze Application, Hwnd zero", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show("Could not analyze Application", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        
        private async void OnExport()
        {
            var saveDialog = new SaveFileDialog
            {
                Title = "Export",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (saveDialog.ShowDialog() == true)
            {
                var strBuilder = new StringBuilder();

                var applicationInfo = _elementService.GetApplicationInfo(SelectedApplication);
                strBuilder.Append(applicationInfo);

                var group = Elements.GroupBy(x => x.ControlType);
                foreach (var groupElement in group)
                {   
                    strBuilder.AppendLine(string.Empty);

                    strBuilder.AppendLine(
                        $"{groupElement.Key.LocalizedControlType.ToUpper()}: ====================================================");
                    
                    strBuilder.AppendLine(string.Empty);

                    foreach (var element in groupElement)
                    {
                        foreach (var data in element.Data)
                        {
                            strBuilder.AppendLine(data);
                        }
                    }
                }

                await File.WriteAllTextAsync(saveDialog.FileName, strBuilder.ToString());

                MessageBox.Show($"Exported to {saveDialog.FileName}", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ApplicationService_ApplicationSelected(object? sender, Application e)
        {
            SelectedApplication = e;
        }

        private void OnInfo()
        {
            try
            {
                var info = _elementService.GetApplicationInfo(SelectedApplication);
                MessageBox.Show(info, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch {}
        }
    }
}
