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
    public class DetailViewModel : Screen
    {
        readonly INavigationService navigationService;

        public DetailViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }
    }
}
