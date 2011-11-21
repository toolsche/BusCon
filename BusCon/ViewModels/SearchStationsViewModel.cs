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
using System.Collections.ObjectModel;

namespace BusCon.ViewModels
{
    public class SearchStationsViewModel : Screen
    {
        readonly INavigationService navigationService;
        public SearchStationsViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;

            Items = new ObservableCollection<ItemViewModel>();
            Items.Add(new ItemViewModel { StationName = "St. Johann", City = "Erlangen-Büchenbach" });
            Items.Add(new ItemViewModel { StationName = "Gostenhof", City = "Nürnberg" });
            Items.Add(new ItemViewModel { StationName = "Plärrer", City = "Nürnberg" });
        }

        private Visibility _nearbyProgressBarVisibility;
        public Visibility NearbyProgressBarVisibility
        {
            get { return _nearbyProgressBarVisibility; }
            set
            {
                _nearbyProgressBarVisibility = value;
                NotifyOfPropertyChange(() => NearbyProgressBarVisibility);
            }
        }

        private ObservableCollection<ItemViewModel> items;
        public ObservableCollection<ItemViewModel> Items
        {
            get { return items; }
            set
            {
                items = value;
                NotifyOfPropertyChange(() => Items);
            }
        }

        public void SelectStation(ItemViewModel station)
        {
            MessageBox.Show("Haltestelle " + station.StationName + " ausgewählt.");
        }
    }
}
