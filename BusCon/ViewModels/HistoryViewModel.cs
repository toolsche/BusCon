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
    public class HistoryViewModel : Screen
    {
        readonly INavigationService navigationService;

        public int SelectedIndex { get; set; }

        public HistoryViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;

            SelectedIndex = -1;
            NotifyOfPropertyChange("SelectedIndex");
        }

        //public void OnHistoryItemMouseUp(MouseEventArgs e)
        //{
        //    FrameworkElement fwe = e.OriginalSource as FrameworkElement;
        //    navigationService.Navigate(new Uri(String.Format("/Views/SearchView.xaml?HistoryIndex={0}", fwe.Tag), UriKind.RelativeOrAbsolute));
        //}

        public void OnSelectionChangedAction(SelectionChangedEventArgs e)
        {
            FrameworkElement fwe = e.AddedItems[0] as FrameworkElement;
            navigationService.Navigate(new Uri(String.Format("/Views/SearchView.xaml?HistoryIndex={0}", fwe.Tag), UriKind.RelativeOrAbsolute));
        }
    }
}
