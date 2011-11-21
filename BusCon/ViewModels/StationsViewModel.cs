using System.Windows;
using System.Device.Location;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using BusCon.Utility;
using BusCon.PTE;
using BusCon.PTE.DTO;
using System;

namespace BusCon.ViewModels
{
    public class StationsViewModel : Screen
    {
        readonly INavigationService navigationService;
        public string SearchText { get; set; }
        public string SearchSource { get; set; }

        private Visibility _searchProgressBarVisibility;
        public Visibility SearchProgressBarVisibility
        {
            get { return _searchProgressBarVisibility; }
            set
            {
                _searchProgressBarVisibility = value;
                NotifyOfPropertyChange(() => SearchProgressBarVisibility);
            }
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

        private string _street;
        public string Street
        {
            get { return _street; }
            set
            {
                _street = value;
                NotifyOfPropertyChange(() => Street);
            }
        }

        private string _city;
        public string City
        {
            get { return _city; }
            set
            {
                _city = value;
                NotifyOfPropertyChange(() => City);
            }
        }

        private ObservableCollection<ItemViewModel> favoriteStations;
        public ObservableCollection<ItemViewModel> FavoriteStations
        {
            get { return favoriteStations; }
            set
            {
                favoriteStations = value;
                NotifyOfPropertyChange(() => FavoriteStations);
            }
        }

        private ObservableCollection<ItemViewModel> nearbyStations;
        public ObservableCollection<ItemViewModel> NearbyStations
        {
            get { return nearbyStations; }
            set
            {
                nearbyStations = value;
                NotifyOfPropertyChange(() => NearbyStations);
            }
        }

        private ObservableCollection<ItemViewModel> foundStations;
        public ObservableCollection<ItemViewModel> FoundStations
        {
            get { return foundStations; }
            set
            {
                foundStations = value;
                NotifyOfPropertyChange(() => FoundStations);
            }
        }

        private EfaRequest efaRequest { get; set; }
        private ConnectionQueryViewModel queryVM { get; set; }

        public StationsViewModel(INavigationService navigationService, ConnectionQueryViewModel queryVM)
        {
            NearbyProgressBarVisibility = Visibility.Collapsed;
            SearchProgressBarVisibility = Visibility.Collapsed;

            NearbyStations = new ObservableCollection<ItemViewModel>();
            FoundStations = new ObservableCollection<ItemViewModel>();

            FavoriteStations = new ObservableCollection<ItemViewModel>();
            FavoriteStations.Add(new ItemViewModel { City = "Erlangen", StationName = "Anton-Bruckner-Str.", Location = new Location(LocationType.STATION, 3003357) });
            FavoriteStations.Add(new ItemViewModel { City = "Erlangen", StationName = "Schenkstr.", Location = new Location(LocationType.STATION, 3003373) });
            FavoriteStations.Add(new ItemViewModel { City = "Erlangen", StationName = "St. Johann", Location = new Location(LocationType.STATION, 3003859) });
            FavoriteStations.Add(new ItemViewModel { City = "Fürth", StationName = "Hauptbahnhof", Location = new Location(LocationType.STATION, 3002110) });
            FavoriteStations.Add(new ItemViewModel { City = "Nürnberg", StationName = "Gostenhof", Location = new Location(LocationType.STATION, 3000703) });
            FavoriteStations.Add(new ItemViewModel { City = "Nürnberg", StationName = "Plärrer", Location = new Location(LocationType.STATION, 3003859) });

            this.navigationService = navigationService;
            this.queryVM = queryVM;

            efaRequest = EfaRequest.Instance;
            efaRequest.ResolveAdressCompleted += new EventHandler<ResolveAddressCompletedEventArgs>(Request_ResolveAdressCompleted);
            efaRequest.SearchNearbyLocationsCompleted += new EventHandler<SearchLocationsCompletedEventArgs>(Request_SearchNearbyLocationsCompleted);
            //efaRequest.LocationRetrievalFailed += new EventHandler<LocationRetrievalFailedEventArgs>(Request_LocationRetrievalFailed);
        }

        private int selectedPivotIndex;
        public int SelectedPivotIndex
        {
            get { return selectedPivotIndex; }
            set
            {
                if (value != selectedPivotIndex)
                {
                    selectedPivotIndex = value;
                    switch (selectedPivotIndex)
                    {
                        case 0:
                            break;

                        case 1:
                            break;

                        // NearbyItems
                        case 2:
                            LocateNearbyStations();
                            break;
                    }
                }
            }
        }

        public void SearchPivotLoaded()
        {
            SearchText = string.Empty;
        }


        /// <summary>
        /// ACTIONS
        /// </summary>
        /// <param name="station"></param>
        
        public void SelectStation(ItemViewModel station)
        {
            if (SearchSource.Equals("DepartureView"))
            {
                queryVM.StationId = station.Location.Id;
                navigationService.UriFor<DepartureViewModel>().WithParam(x => x.DepartureField, station.StationName + ", " + station.City + "$" + station.Location.Id).Navigate();
            }
            else if (SearchSource.Equals("Departure"))
            {
                queryVM.From = station.Location;
                navigationService.UriFor<SearchViewModel>().WithParam(x => x.DepartureField, station.StationName + ", " + station.City + "$" + station.Location.Id).Navigate();
            }
            else
            {
                queryVM.From = station.Location;
                navigationService.UriFor<SearchViewModel>().WithParam(x => x.ArrivalField, station.StationName + ", " + station.City + "$" + station.Location.Id).Navigate();
            }
        }

        public void FindStations()
        {
            if (SearchText != string.Empty)
            {
                FoundStations.Clear();

                SearchProgressBarVisibility = Visibility.Visible;
                efaRequest.LoadStations(SearchText);
            }
        }

        public void LocateNearbyStations()
        {
            NearbyStations.Clear();
            NearbyProgressBarVisibility = Visibility.Visible;

            bool stationsOnly = true;
            
            efaRequest.LocateNearbyStations(new Object[] { this, stationsOnly });
        }


        /// <summary>
        /// ASYNC CALLBACKS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private bool _isNoResultMessageVisible;
        public bool IsNoResultMessageVisible
        {
            get { return _isNoResultMessageVisible; }
            set
            {
                if (_isNoResultMessageVisible != value)
                {
                    _isNoResultMessageVisible = value;
                    NotifyOfPropertyChange<bool>(() => IsNoResultMessageVisible);
                }
            }
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
                        NearbyStations.Add(station);
                    }
                });
            }
            else
            {
                SearchProgressBarVisibility = Visibility.Collapsed;
                Utility.UIThread.Invoke(() =>
                {
                    IsNoResultMessageVisible = e.Stations.Count == 0;

                    foreach (var station in e.Stations)
                    {
                        FoundStations.Add(station);
                    }
                });
            }
        }

        //private void Request_LocationRetrievalFailed(object sender, LocationRetrievalFailedEventArgs e)
        //{
        //    NearbyProgressBarVisibility = Visibility.Collapsed;
        //    NoResultMessageText = e.ErrorText;
        //    IsNoResultMessageVisible = true;
        //}

        private void Request_ResolveAdressCompleted(object sender, ResolveAddressCompletedEventArgs e)
        {
            Street = e.Address.AddressLine2.Split(',')[0];
            City = e.Address.City;
        }
    }
}
