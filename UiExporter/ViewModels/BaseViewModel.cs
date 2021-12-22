using System;
using System.Collections.Generic;
using System.ComponentModel;
using UiExporter.Models;

namespace UiExporter.ViewModels
{
    internal class BaseViewModel : ObservableObject
    {
        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
    }
}
