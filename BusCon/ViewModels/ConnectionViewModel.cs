using System;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Caliburn.Micro;
using BusCon.Animation;
using BusCon.PTE.DTO;
using BusCon.Utility;

namespace BusCon.ViewModels
{
    public class ConnectionViewModel : Screen
    {
        readonly INavigationService navigationService;
        readonly IWindowManager windowManager;

        private EfaRequest efaRequest { get; set; }

        private UCConnection _currentConnection = new UCConnection();
        public UCConnection CurrentConnection
        {
            get { return _currentConnection; }
        }

        private Thickness _redLineMargin;
        public Thickness RedLineMargin
        {
            get { return _redLineMargin; }
            set
            {
                _redLineMargin = value;
                NotifyOfPropertyChange("RedLineMargin");
            }
        }

        public string QueryTime { get; set; }

        private List<string> _timeSlots;
        public List<string> TimeSlots 
        {
            get { return _timeSlots; }
            set
            {
                _timeSlots = value;
                NotifyOfPropertyChange("TimeSlots");
            }
        }

        public ConnectionViewModel()
        {
            efaRequest = EfaRequest.Instance;
            efaRequest.SearchConnectionsCompleted += new EventHandler<SearchConnectionsCompletedEventArgs>(Request_SearchConnectionsCompleted);
        }

        public ConnectionViewModel(INavigationService navigationService, IWindowManager windowManager)
        {
            this.navigationService = navigationService;
            this.windowManager = windowManager;

            efaRequest = EfaRequest.Instance;
            efaRequest.SearchConnectionsCompleted += new EventHandler<SearchConnectionsCompletedEventArgs>(Request_SearchConnectionsCompleted);
        }

        private void SearchConnectionsCompleted(QueryConnectionsResult result)
        {
        }

        private void Request_SearchConnectionsCompleted(object sender, SearchConnectionsCompletedEventArgs e)
        {
            efaRequest.SearchConnectionsCompleted -= Request_SearchConnectionsCompleted;
            SetConnections(e.Connections);
        }

        public void ShowDetailView()
        {
            navigationService.Navigate(new Uri("/Views/DetailView.xaml", UriKind.RelativeOrAbsolute));
        }

        public void SetConnections(ObservableCollection<Connection> connections)
        {
            Utility.UIThread.Invoke(() =>
            {
                CurrentConnection.Clear();
            });

            DateTime queryDateTime;
            DateTime.TryParse(QueryTime, out queryDateTime);

            foreach (var connection in connections)
            {
                if (CurrentConnection.StartTime.Year < 1900 || connection.DepartureTime < CurrentConnection.StartTime)
                {
                    Utility.UIThread.Invoke(() =>
                    {
                        CurrentConnection.StartTime = connection.DepartureTime;
                        CurrentConnection.From = connection.From.Name;
                        CurrentConnection.To = connection.To.Name;
                    });
                }

                Trip ucTrip = new Trip();
                ucTrip.Departure = connection.DepartureTime;
                ucTrip.Arrival = connection.ArrivalTime;

                int waitTime = (int)(connection.DepartureTime - queryDateTime).TotalMinutes;
                ucTrip.WaitTime = waitTime > 0 ? waitTime : 0; 

                foreach (Connection.Trip trip in connection.Parts)
                {
                    UCPart part = new UCPart();
                    part.Departure = trip.DepartureTime;
                    part.Arrival = trip.ArrivalTime;
                    part.Line = trip.Line.Label;

                    switch (trip.Line.LineType)
                    {
                        case "IC":
                        case "ICE":
                            part.TransportType = TransportType.ICE;
                            break;
                        case "Stadtbus":
                            part.TransportType = TransportType.Bus;
                            break;
                        case "U-Bahn":
                            part.TransportType = TransportType.Subway;
                            break;
                        case "Straßenbahn":
                            part.TransportType = TransportType.Tram;
                            break;
                        case "S-Bahn":
                        case "R-Bahn":
                            part.TransportType = TransportType.Train;
                            break;
                        default:
                            part.TransportType = TransportType.ICE;
                            break;
                    }

                    DateTime tempDateTime = CurrentConnection.StartTime;
                    tempDateTime = tempDateTime.AddMinutes(CurrentConnection.StartTime.Minute / 10 * 10 - CurrentConnection.StartTime.Minute);

                    var timeSpan = part.Departure - tempDateTime;
                    part.MarginTop = new Thickness(0, Convert.ToInt32((timeSpan.Hours * 60 + timeSpan.Minutes) * 4.0), 0, 0);

                    timeSpan = part.Arrival - part.Departure;
                    part.RectangleHeight = Convert.ToInt32((timeSpan.Hours * 60 + timeSpan.Minutes) * 4.0);

                    ucTrip.Parts.Add(part);
                }

                Utility.UIThread.Invoke(() =>
                {
                    CurrentConnection.Trips.Add(ucTrip);
                });
            }

            if (queryDateTime != null)
            {
                TimeSpan queryToStart = queryDateTime - CurrentConnection.StartTime;
                double minutes = queryToStart.Hours * 60 + queryToStart.Minutes;

                Utility.UIThread.Invoke(() =>
                {
                    RedLineMargin = new Thickness(minutes * 3, 0, 0, 0);
                });
            }

            Utility.UIThread.Invoke(() =>
            {
                TimeSlots = CreateTimeSlots();
            });
        }
        
        private List<string> CreateTimeSlots()
        {
            if (CurrentConnection != null && CurrentConnection.StartTime.Year > 1900)
            {
                List<string> timeSlots = new List<string>();

                //int tripLengthInMinutes = 0;
                //TimeSpan tripLength;

                DateTime arrival = CurrentConnection.StartTime;
                foreach (var trip in CurrentConnection.Trips)
                {
                    // save longest trip time
                    //tripLength = trip.Arrival - trip.Departure;
                    //if (tripLengthInMinutes < tripLength.Hours * 60 + tripLength.Minutes)
                    //{
                    //    tripLengthInMinutes = tripLength.Hours * 60 + tripLength.Minutes;
                    //}

                    // save latest arrival time

                    if (trip.Arrival > arrival)
                        arrival = trip.Arrival;
                }

                DateTime tempDateTime = CurrentConnection.StartTime;
                tempDateTime = tempDateTime.AddMinutes(CurrentConnection.StartTime.Minute / 10 * 10 - CurrentConnection.StartTime.Minute);

                do
                {
                    timeSlots.Add(string.Format(tempDateTime.ToString("HH:mm")));
                    tempDateTime = tempDateTime.AddMinutes(10);
                } while (tempDateTime < arrival.AddMinutes(10));

                return timeSlots;
            }

            return null;
        }
    }


    public class UCConnection : INotifyPropertyChanged
    {
        public void Clear()
        {
            Trips.Clear();
            StartTime = new DateTime();
        }

        private ObservableCollection<Trip> _trips = new ObservableCollection<Trip>();
        public ObservableCollection<Trip> Trips
        {
            get { return _trips; }
        }

        private DateTime _startTime;
        public DateTime StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                NotifyPropertyChanged("StartTime");
            }
        }

        private string _from;
        public string From
        {
            get { return _from; }
            set
            {
                _from = value;
                NotifyPropertyChanged("From");
            }
        }

        private string _to;
        public string To
        {
            get { return _to; }
            set
            {
                _to = value;
                NotifyPropertyChanged("To");
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

    public class Trip
    {
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public List<UCPart> Parts { get; set; }

        public int WaitTime { get; set; }

        public TimeSpan TripDuration
        {
            get
            {
                return Arrival - Departure;
            }
        }

        public Trip()
        {
            Parts = new List<UCPart>();
        }

        public void ShowDetailView()
        {
            //navigationService.Navigate(new Uri("/Views/DetailView.xaml", UriKind.RelativeOrAbsolute));
        }

    }

    public class UCPart : INotifyPropertyChanged
    {
        public string Line { get; set; }
        public DateTime TripStartTime { get; set; }
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public TransportType TransportType { get; set; }

        public Brush LineColor
        {
            get
            {
                switch (TransportType)
                {
                    case (TransportType.Bus):
                        return new SolidColorBrush(Colors.Red);
                    case (TransportType.ICE):
                        return new SolidColorBrush(Colors.White);
                    case (TransportType.Subway):
                        return new SolidColorBrush(Colors.Blue);
                    case (TransportType.Train):
                        return new SolidColorBrush(Colors.Green);
                    case (TransportType.Tram):
                        return new SolidColorBrush(Colors.Brown);
                    default:
                        return new SolidColorBrush(Colors.White);
                }
            }
        }

        public int PartDuration
        {
            get 
            { 
                return (int)(Arrival - Departure).TotalMinutes; 
            }
        }

        private Thickness _marginTop;
        public Thickness MarginTop
        {
            get
            {
                return _marginTop;
            }
            set
            {
                _marginTop = value;
                NotifyPropertyChanged("MarginTop");
            }
        }

        private int _rectangleHeight;
        public int RectangleHeight
        {
            get
            {
                return _rectangleHeight;
            }
            set
            {
                _rectangleHeight = value;
                NotifyPropertyChanged("RectangleHeight");
            }
        }

        public Style PartStyle
        {
            get
            {
                switch (TransportType)
                {
                    case (TransportType.Bus):
                        return Application.Current.Resources["TripPartRectangleRedStyle"] as Style;
                    case (TransportType.ICE):
                        return Application.Current.Resources["TripPartRectangleWhiteStyle"] as Style;
                    case (TransportType.Subway):
                        return Application.Current.Resources["TripPartRectangleBlueStyle"] as Style;
                    case (TransportType.Train):
                        return Application.Current.Resources["TripPartRectangleGreenStyle"] as Style;
                    case (TransportType.Tram):
                        return Application.Current.Resources["TripPartRectanglePinkStyle"] as Style;
                    default:
                        return Application.Current.Resources["TripPartRectangleYellowStyle"] as Style;
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

    public enum TransportType
    {
        Subway,
        Train,
        Bus,
        Tram,
        ICE
    }
}
