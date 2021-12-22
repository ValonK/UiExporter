using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPFUI.Common;
using Application = UiExporter.Models.Application;

namespace UiExporter.Views
{
    public partial class SelectAppView : Window
    {
        public event EventHandler<Application?> ApplicationSelected; 
        public SelectAppView(IList<Models.Application> applications)
        {
            InitializeComponent();
            DataContext = this;
            Applications = new ObservableCollection<Models.Application>(applications.Where(x => x.Name != "UiExporter"));
        }

        public ObservableCollection<Models.Application> Applications { get; set; }


        private void EventSetter_OnHandler(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem { DataContext: Models.Application application })
            {
                ApplicationSelected?.Invoke(this, application);
                Hide();
            }
        }
    }
}
