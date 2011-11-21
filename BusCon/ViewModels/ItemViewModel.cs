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
using System.Collections.ObjectModel;
using System.ComponentModel;
using BusCon.PTE.DTO;

namespace BusCon.ViewModels
{
    public class ItemViewModel
    {
        public ObservableCollection<Departure> Departures { get; set; }

        public ItemViewModel()
        {
            Departures = new ObservableCollection<Departure>();
        }

        private Thickness _selectionBorderThickness;

        public Thickness SelectionBorderThickness
        {
            get
            {
                return _selectionBorderThickness;
            }
            set
            {
                if (value != _selectionBorderThickness)
                {
                    _selectionBorderThickness = value;
                    NotifyPropertyChanged("SelectionBorderThickness");
                }
            }
        }

        public int ConnectionNr { get; set; }

        public string Duration { get; set; }

        public int Changes { get; set; }

        public string DepTime { get; set; }

        public string ArrTime { get; set; }

        private string _stationName;
        /// <summary>
        /// 
        /// </summary>
        public string StationName
        {
            get
            {
                return _stationName;
            }
            set
            {
                if (value != _stationName)
                {
                    _stationName = value;
                    NotifyPropertyChanged("StationName");
                }
            }
        }

        private string _stationNameDest;
        /// <summary>
        /// 
        /// </summary>
        public string StationNameDest
        {
            get
            {
                return _stationNameDest;
            }
            set
            {
                if (value != _stationNameDest)
                {
                    _stationNameDest = value;
                    NotifyPropertyChanged("StationNameDest");
                }
            }
        }

        private string _city;
        /// <summary>
        /// 
        /// </summary>
        public string City
        {
            get
            {
                return _city;
            }
            set
            {
                if (value != _city)
                {
                    _city = value;
                    NotifyPropertyChanged("City");
                }
            }
        }

        private string _distance;

        public string Distance
        {
            get
            {
                return _distance;
            }
            set
            {
                if (value != _distance)
                {
                    _distance = value;
                    NotifyPropertyChanged("Distance");
                }
            }
        }

        private Location _location;

        public Location Location
        {
            get
            {
                return _location;
            }
            set
            {
                if (value != _location)
                {
                    _location = value;
                    NotifyPropertyChanged("Location");
                }
            }
        }

        private string _itemGuid;
        /// <summary>
        /// 
        /// </summary>
        public string ItemGuid
        {
            get
            {
                return _itemGuid;
            }
            set
            {
                if (value != _itemGuid)
                {
                    _itemGuid = value;
                    NotifyPropertyChanged("ItemGuid");
                }
            }
        }


        private string _pageTitle;
        /// <summary>
        /// 
        /// </summary>
        public string PageTitle
        {
            get
            {
                return _pageTitle;
            }
            set
            {
                if (value != _pageTitle)
                {
                    _pageTitle = value;
                    NotifyPropertyChanged("PageTitle");
                }
            }
        }

        private string _pageURL;
        /// <summary>
        /// 
        /// </summary>
        public string PageURL
        {
            get
            {
                return _pageURL;
            }
            set
            {
                if (value != _pageURL)
                {
                    _pageURL = value;
                    NotifyPropertyChanged("PageURL");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
