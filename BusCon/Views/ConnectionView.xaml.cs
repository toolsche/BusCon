using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BusCon.ViewModels;
using BusCon.Animation;
using BusCon.Utility;
using Microsoft.Phone.Controls;

namespace BusCon.Views
{
    public partial class ConnectionView : PhoneApplicationPage, INotifyPropertyChanged
    {
        public UCConnection CurrentConnection { get; set; }
        private int _testWidth;

        public int TestWidth
        {
            get { return _testWidth; }
            set
            {
                _testWidth = value;
                NotifyPropertyChanged("TestWidth");
            }
        }

        public ConnectionView()
        {
            InitializeComponent();

            //DataContext = this;
            //CurrentConnection = UCConnection.GetDummyConnection();
            //Loaded += new RoutedEventHandler(OnLoaded);
            TestWidth = 120;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var fwe = sender as FrameworkElement;
            int msdelay = 0;

            if (fwe is Rectangle)
            {
                var rect = fwe as Rectangle;
                double width = rect.Width.Equals(double.NaN) ? 0.0 : rect.Width;
                rect.Width = 0;
                var effect = new GrowToWidthEffect(0, width, 3, msdelay);
                effect.Start(rect);
                msdelay += 100;
            }

            //foreach (Rectangle rect in fwe.FindVisualChildren<Rectangle>(x => x.Style != null))
            //{
            //    double width = rect.Width.Equals(double.NaN) ? 0.0 : rect.Width;
            //    rect.Width = 0;
            //    var effect = new GrowToWidthEffect(0, width, 3, msdelay);
            //    effect.Start(rect);
            //    msdelay += 100;
            //}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(
                this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
