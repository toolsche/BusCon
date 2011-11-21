using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace BusCon.Views
{
    public partial class DetailView : PhoneApplicationPage
    {
        public DetailView()
        {
            InitializeComponent();
        }

        private void ConnectionsLoaded(object sender, RoutedEventArgs e)
        {
            this.Progress1.IsIndeterminate = false;
            this.Progress1.Visibility = Visibility.Collapsed;
        }

        private void btn_ShowStops_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}