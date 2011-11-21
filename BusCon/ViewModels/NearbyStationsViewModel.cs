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
using BusCon.PTE;
using BusCon.Utility;

namespace BusCon.ViewModels
{
    public class NearbyStationsViewModel : Screen
    {
        readonly INavigationService navigationService;
        public NearbyStationsViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;

            Items = new ObservableCollection<ItemViewModel>();
            Items.Add(new ItemViewModel { StationName = "Schenkstr.", City = "Erlangen" });
            Items.Add(new ItemViewModel { StationName = "Stintzingstr.", City = "Erlangen" });
            Items.Add(new ItemViewModel { StationName = "Plärrer", City = "Nürnberg" });

            EfaRequest.Instance.SearchNearbyLocationsCompleted += Request_SearchNearbyLocationsCompleted;
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

        public void LocateNearbyStations()
        {
            Items.Clear();
            NearbyProgressBarVisibility = Visibility.Visible;

            bool stationsOnly = true;

            EfaRequest.Instance.LocateNearbyStations(new Object[] { this, stationsOnly });
        }

        private void Request_SearchNearbyLocationsCompleted(object sender, SearchLocationsCompletedEventArgs e)
        {
            if (e.UserState.Equals("nearbyItems"))
            {
                NearbyProgressBarVisibility = Visibility.Collapsed;
                Utility.UIThread.Invoke(() =>
                {
                    foreach (var station in e.Stations)
                    {
                        Items.Add(station);
                    }
                });
            }
        }

        public void Locate()
        {
            VvmProvider efa = new VvmProvider();
        }

        public void SelectStation(ItemViewModel station)
        {
            MessageBox.Show("Haltestelle " + station.StationName + " ausgewählt.");
        }
    }
}
