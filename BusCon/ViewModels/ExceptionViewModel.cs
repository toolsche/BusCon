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

namespace BusCon
{
    public class ExceptionViewModel : Screen
    {
        public string exceptionText;

        public string ExceptionText
        {
            get { return exceptionText; }
            set { exceptionText = value; NotifyOfPropertyChange(() => ExceptionText); }
        }

        public string Result { get; set; }
        public bool SendException;

        public void Confirm()
        {
            SendException = true;
            TryClose();
        }
        public void Cancel()
        {
            SendException = false;
            Result = null;
            this.TryClose();
        }
    }
}
