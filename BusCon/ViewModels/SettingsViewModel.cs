using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Caliburn.Micro;

namespace BusCon.ViewModels
{
    public class SettingsViewModel : Screen
    {
        private bool _areLocationServicesAllowed;
        public bool AreLocationServicesAllowed
        {
            get { return _areLocationServicesAllowed; }
            set 
            { 
                _areLocationServicesAllowed = value;
                NotifyOfPropertyChange<bool>(() => AreLocationServicesAllowed);
            }
        }
        

        private bool _isSendFeedbackEnabled;
        public bool IsSendFeedbackEnabled
        {
            get { return _isSendFeedbackEnabled; }
            set 
            { 
                _isSendFeedbackEnabled = value;
                NotifyOfPropertyChange<bool>(() => IsSendFeedbackEnabled);
            }
        }
    }
}
