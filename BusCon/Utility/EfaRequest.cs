using System;
using System.Net;
using System.Windows;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.IO;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Device.Location;

using BusCon.ViewModels;
using System.Threading;
using BusCon.PTE.DTO;

namespace BusCon.Utility
{
    public class EfaRequest
    {
        public string API_BASE = "http://efa.mobilitaetsverbund.de/web/";
        const string TRIP_REQUEST = "XSLT_TRIP_REQUEST2?";
        const string DEPARTURE_REQUEST = "XSLT_DM_REQUEST?";
        const string COORD_REQUEST = "XML_COORD_REQUEST?";
        private String DEPARTURE_URI;

        public ObservableCollection<ItemViewModel> Items { get; private set; }
        public ObservableCollection<ItemViewModel> NearbyItems { get; private set; }
        public ObservableCollection<Connection> Connections { get; private set; }

        public Connection Connection { get; set; }
        public TextBox ResultTextBox { get; set; }

        private static EfaRequest instance;
        public static EfaRequest Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EfaRequest();
                }
                return instance;
            }
        }

        private EfaRequest()
        {
            this.Items = new ObservableCollection<ItemViewModel>();
            this.NearbyItems = new ObservableCollection<ItemViewModel>();
            this.Connections = new ObservableCollection<Connection>();
            this.Connection = new Connection();
            _adressResolver = new Maps.CustomAdressResolver();

            DEPARTURE_URI = DEPARTURE_REQUEST
                        + "outputFormat=XML&coordOutputFormat=WGS84&type_dm=stop&name_dm={0}&itOptionsActive=1&ptOptionsActive=1&useProxFootSearch=1&mergeDep=1&useAllStops=1&mode=direct";
        }

        #region SearchNearbyLocationsCompleted

        public event EventHandler<ResolveAddressCompletedEventArgs> ResolveAdressCompleted;
        public event EventHandler<SearchLocationsCompletedEventArgs> SearchNearbyLocationsCompleted;
        public event EventHandler<SearchConnectionsCompletedEventArgs> SearchConnectionsCompleted;

        private void OnSearchNearbyLocationsCompleted(SearchLocationsCompletedEventArgs e)
        {
            if (SearchNearbyLocationsCompleted != null)
            {
                SearchNearbyLocationsCompleted(this, e);
            }
        }

        private void OnSearchConnectionsCompleted(SearchConnectionsCompletedEventArgs e)
        {
            if (SearchConnectionsCompleted != null)
            {
                SearchConnectionsCompleted(this, e);
            }
        }

        private Maps.CustomAdressResolver _adressResolver;

        #endregion

        public void LoadStations(string query)
        {
            this.Items.Clear();
            //this._query = query;
            UriBuilder fullUri = new UriBuilder(API_BASE + TRIP_REQUEST);

            // Common request fields (required)
            fullUri.Query = "outputFormat=XML"
                //+ "&coordOutputFormat=WGS84"
            + "&type_origin=any"
            + "&name_origin=" + query;

            // initialize a new WebRequest
            HttpWebRequest getStationsRequest = (HttpWebRequest)WebRequest.Create(fullUri.Uri);   ///this is main
            //ResultTextBox.Text = ResultTextBox.Text + (ResultTextBox.Text.Length > 0 ? "\n\n" : string.Empty) + fullUri.Uri;

            // set up the state object for the async request
            QueryUpdateState queryUpdateState = new QueryUpdateState();
            queryUpdateState.AsyncRequest = getStationsRequest;

            // start the asynchronous request
            getStationsRequest.BeginGetResponse(new AsyncCallback(AsyncResponseLoadStations), queryUpdateState); // this is still main thread
        }


        /// <summary>
        /// Handle the found stations response returned from the async request
        /// </summary>
        /// <param name="asyncResult"></param>
        private void AsyncResponseLoadStations(IAsyncResult asyncResult)  ///now this is background
        {
            // get the state information
            QueryUpdateState queryUpdateState = (QueryUpdateState)asyncResult.AsyncState;
            HttpWebRequest getStationsRequest = (HttpWebRequest)queryUpdateState.AsyncRequest;

            // end the async request
            queryUpdateState.AsyncResponse = (HttpWebResponse)getStationsRequest.EndGetResponse(asyncResult);

            Stream streamResult;

            try
            {
                // get the stream containing the response from the async call
                streamResult = queryUpdateState.AsyncResponse.GetResponseStream();

                // load the XML
                XElement xmlStationsResponse = XElement.Load(streamResult);
                XElement originElem = xmlStationsResponse.Descendants("itdOdv").Where(elem => elem.Attribute("usage").Value.Equals("origin")).FirstOrDefault();

                List<ItemViewModel> stations = new List<ItemViewModel>();

                foreach (var xmlStation in originElem.Descendants("odvNameElem"))
                {
                    if (xmlStation.HasAttributes)
                    {
                        var station = new ItemViewModel();
                        station.SelectionBorderThickness = new Thickness(0);
                        station.City = xmlStation.Attribute("locality").Value;
                        station.StationName = xmlStation.Attribute("objectName").Value;

                        Location loc = new Location(LocationType.STATION, int.Parse(xmlStation.Attribute("id").Value));
                        station.Location = loc;
                        //temp.ThumbNailImage = new BitmapImage(new Uri("Images/Haltestelle.gif"));

                        stations.Add(station);
                    }
                }

                OnSearchNearbyLocationsCompleted(new SearchLocationsCompletedEventArgs(null, false, "items", stations));
            }
            catch (FormatException fe)
            {
                throw new Exception("Fehler bei GetResponseStream()", fe.InnerException);
                // there was some kind of error processing the response from the web
                // additional error handling would normally be added here

            }

            //long applicationCurrentMemoryUsage = (long)DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage");
        }


        public void LoadDepartures(int stopId)
        {
            UriBuilder fullUri = new UriBuilder(API_BASE + string.Format(DEPARTURE_URI, stopId));

            ResultTextBox.Text = ResultTextBox.Text + "\n\nDEPARTURES:\n-----------\n" + fullUri.Uri;

            HttpWebRequest getConnectionsRequest = (HttpWebRequest)WebRequest.Create(fullUri.Uri);   ///this is main

            // set up the state object for the async request
            QueryUpdateState queryUpdateState = new QueryUpdateState();
            queryUpdateState.AsyncRequest = getConnectionsRequest;

            // start the asynchronous request
            getConnectionsRequest.BeginGetResponse(new AsyncCallback(HandleDeparturesResponse), queryUpdateState); // this is still main thread
        }


        /// <summary>
        /// Handle the departures request response
        /// </summary>
        /// <param name="asyncResult"></param>
        private void HandleDeparturesResponse(IAsyncResult asyncResult)  ///now this is background
        {
            // get the state information
            QueryUpdateState queryUpdateState = (QueryUpdateState)asyncResult.AsyncState;
            HttpWebRequest getStationsRequest = (HttpWebRequest)queryUpdateState.AsyncRequest;

            // end the async request
            queryUpdateState.AsyncResponse = (HttpWebResponse)getStationsRequest.EndGetResponse(asyncResult);

            Stream streamResult;

            try
            {
                // get the stream containing the response from the async call
                streamResult = queryUpdateState.AsyncResponse.GetResponseStream();

                // load the XML
                XElement xmlDeparturesResponse = XElement.Load(streamResult);
                ObservableCollection<Departure> departureList = new ObservableCollection<Departure>();

                XElement originElem = xmlDeparturesResponse.Descendants("itdDepartureList").FirstOrDefault();
                if (originElem == null) return;
                
                DateTime? predictedTime = new DateTime?();

                int count = 0;
                foreach (var xmlStation in originElem.Descendants("itdDeparture"))
                {
                    if (xmlStation.HasAttributes && count < 8)
                    {
                        // get departure time
                        XElement plannedTimeElement = xmlStation.Descendants("itdDateTime").FirstOrDefault();
                        XElement itdRTDateTimeElement = xmlStation.Descendants("itdRTDateTime").FirstOrDefault();

                        DateTime plannedTime = ProcessItdDateTime(plannedTimeElement);

                        if (itdRTDateTimeElement != null)
                            predictedTime = ProcessItdDateTime(itdRTDateTimeElement);

                        // get serving line info
                        XElement itdServingLine = xmlStation.Descendants("itdServingLine").FirstOrDefault();

                        var departure = new Departure(plannedTime, itdServingLine.Attribute("number").Value, xmlStation.Attribute("stopName").Value, 0, itdServingLine.Attribute("direction").Value);

                        count++;
                    }
                }

                if (departureList.Count == 0)
                {
                    ResultTextBox.Dispatcher.BeginInvoke(
                      new System.Action(
                        delegate()
                        {
                            ResultTextBox.Text = ResultTextBox.Text + "\n\n" + "Keine Abfahrten.";
                        }
                    ));
                    return;
                }

                string stopId = xmlDeparturesResponse.Descendants("odvNameInput").FirstOrDefault().Value;
                //ItemViewModel station = NearbyItems.First(x => x.Location.Id.Equals(int.Parse(stopId)));

                foreach (Departure item in departureList)
                {
                    ResultTextBox.Dispatcher.BeginInvoke(
                      new System.Action(
                        delegate()
                        {
                            ResultTextBox.Text = ResultTextBox.Text + "\n" + item.Line + " -> " + item.Destination + " (Abfahrt: " + item.DepartureTime.ToString("HH:mm") + ")";
                        }
                    ));

                    System.Threading.Thread.Sleep(50);
                }
            }
            catch (FormatException fe)
            {
                throw new Exception("Fehler bei GetResponseStream()", fe.InnerException);
                // there was some kind of error processing the response from the web
                // additional error handling would normally be added here
            }
        }

        private DateTime ProcessItdDateTime(XElement itdDateTime)
        {
            XElement itdDate = itdDateTime.Descendants("itdDate").FirstOrDefault();
            int year = int.Parse(itdDate.Attribute("year").Value);
            int month = int.Parse(itdDate.Attribute("month").Value);
            int day = int.Parse(itdDate.Attribute("day").Value);
            if (year == 0)
                return new DateTime();

            XElement itdTime = itdDateTime.Descendants("itdTime").FirstOrDefault();
            int hour = int.Parse(itdTime.Attribute("hour").Value);
            int minute = int.Parse(itdTime.Attribute("minute").Value);
            if (hour == 24)
            {
                hour = 0;
                ++day;
            }

            return new DateTime(year, month, day, hour, minute, 0);
        }

        #region LOCATION and NEARBY STATIONS

        private GeoCoordinateWatcher _geoWatcher;

        public void LocateNearbyStations(Object userState)
        {
            _geoWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            _geoWatcher.MovementThreshold = 20.0;
            _geoWatcher.StatusChanged += OnStatusChanged;

            ThreadPool.QueueUserWorkItem((p) => GetCoordAsync());
        }

        private void GetCoordAsync() // background
        {
            if (_geoWatcher.TryStart(false, TimeSpan.FromSeconds(5)) == false)
            {
                CompleteGetCoordAsync(error: new TimeoutException());
            }
        }

        private void OnStatusChanged(Object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Ready)
            {
                GeoCoordinate coordinate = _geoWatcher.Position.Location;

                //Cache.Default.Insert(CACHE_KEY, coordinate, DateTime.Now.AddMinutes(CACHE_DURATION), Cache.NoSlidingExpiration);

                CompleteGetCoordAsync(coord: coordinate);
            }
            else if (e.Status == GeoPositionStatus.Disabled)
            {
            }
            else if (e.Status == GeoPositionStatus.Initializing)
            {
            }
            else if (e.Status == GeoPositionStatus.NoData)
            {
            }
        }

        private void CompleteGetCoordAsync(Exception error = null, GeoCoordinate coord = null)
        {
            _geoWatcher.Stop();
            ResolveAddressFromCoord(error, coord);
        }

        private void ResolveAddressFromCoord(Exception error, GeoCoordinate coord)
        {
            //SetMyPosition(coord);
            // {49.5879201889038, 11.017370223999}

            //Maps.CustomAdressResolver car = new Maps.CustomAdressResolver();
            //if (ResolveAdressCompleted != null)
            //    car.ResolveAddressCompleted += new EventHandler<ResolveAddressCompletedEventArgs>(ResolveAdressCompleted);

            if (coord != null)
            {
                _adressResolver.ResolveAddressCompleted += ResolveAdressCompleted;
                _adressResolver.ResolveAddressAsync(coord);

                NearbyStations(null, coord.Latitude, coord.Longitude, 750, 20);
            }
            // double latitude, double longitude
            // {49.5879201889038, 11.017370223999}

            //NearbyStations(null, 49.5879201889038, 11.017370223999, 750, 20);
        }

        public void NearbyStations(string stationId, double lat, double lon, int maxDistance, int maxStations)
        {
            if (lat != 0 || lon != 0)
                XmlCoordRequest(lat, lon, maxDistance, maxStations);
        }


        public void XmlCoordRequest(double lat, double lon, int maxDistance, int maxStations)
        {
            UriBuilder fullUri = new UriBuilder(API_BASE + COORD_REQUEST);

            // Common request fields (required)
            fullUri.Query = "coord="
            + String.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:0.000000}:{1:0.000000}:WGS84", lon, lat)
            + "outputFormat=XML"
            + "&coordOutputFormat=WGS84&coordListOutputFormat=STRING"
            + "&max=" + (maxStations != 0 ? maxStations : 50)
            + "&inclFilter=1&radius_1=" + (maxDistance != 0 ? maxDistance : 1320)
            + "&type_1=STOP";

            //if (additionalQueryParameter != null)
            //            uri = uri + "&" additionalQueryParameter;

            // efa.mobilitaetsverbund.de/web/XML_COORD_REQUEST?coord=11.021090:49.587970:WGS84&coordOutputFormat=WGS84&coordListOutputFormat=STRING&max=20&inclFilter=1&radius_1=1000&type_1=STOP

            // initialize a new WebRequest
            HttpWebRequest getStationsRequest = (HttpWebRequest)WebRequest.Create(fullUri.Uri);   ///this is main

            // set up the state object for the async request
            QueryUpdateState queryUpdateState = new QueryUpdateState();
            queryUpdateState.AsyncRequest = getStationsRequest;

            // start the asynchronous request
            getStationsRequest.BeginGetResponse(new AsyncCallback(HandleCoordResponse), queryUpdateState); // this is still main thread
        }


        /// <summary>
        /// Handle the first response returned from the async request
        /// </summary>
        /// <param name="asyncResult"></param>
        private void HandleCoordResponse(IAsyncResult asyncResult)  ///now this is background
        {
            // get the state information
            QueryUpdateState queryUpdateState = (QueryUpdateState)asyncResult.AsyncState;
            HttpWebRequest getStationsRequest = (HttpWebRequest)queryUpdateState.AsyncRequest;

            // end the async request
            queryUpdateState.AsyncResponse = (HttpWebResponse)getStationsRequest.EndGetResponse(asyncResult);
            
            string status = queryUpdateState.AsyncResponse.StatusDescription;
            HttpStatusCode statusCode = queryUpdateState.AsyncResponse.StatusCode;

            Stream streamResult;

            try
            {
                // get the stream containing the response from the async call
                streamResult = queryUpdateState.AsyncResponse.GetResponseStream();

                // load the XML
                XElement xmlStationsResponse = XElement.Load(streamResult);
                XElement originElem = xmlStationsResponse.Descendants("coordInfoItemList").FirstOrDefault();

                if (originElem != null)
                {
                    List<ItemViewModel> stations = new List<ItemViewModel>();

                    foreach (var xmlStation in originElem.Descendants("coordInfoItem"))
                    {
                        if (xmlStation.HasAttributes)
                        {
                            var station = new ItemViewModel();
                            station.SelectionBorderThickness = new Thickness(0);

                            station.City = xmlStation.Attribute("locality").Value;
                            station.StationName = xmlStation.Attribute("name").Value;

                            int id = -1;
                            if (xmlStation.Attribute("id") != null)
                                id = int.Parse(xmlStation.Attribute("id").Value);

                            int distance = -1;
                            if (xmlStation.Attribute("distance") != null)
                                distance = int.Parse(xmlStation.Attribute("distance").Value);

                            int lon = -1, lat = -1;
                            XElement coordElem = xmlStation.Descendants("itdCoordinateString").FirstOrDefault();
                            if (coordElem != null)
                            {
                                string[] coordString = coordElem.Value.Split(',');
                                if (coordString.Length == 2)
                                {
                                    lon = int.Parse(coordString[0]);
                                    lat = int.Parse(coordString[1]);
                                }
                            }

                            station.Location = new Location(LocationType.STATION, id, lat, lon);
                            station.Distance = distance + "m";

                            stations.Add(station);
                        }
                    }

                    OnSearchNearbyLocationsCompleted(new SearchLocationsCompletedEventArgs(null, false, "nearbyItems", stations));
                }
            }
            catch (FormatException fe)
            {
                throw new Exception("Fehler bei GetResponseStream()", fe.InnerException);
                // there was some kind of error processing the response from the web
                // additional error handling would normally be added here

            }
        }

        #endregion


        public void RetrieveConnections(DateTime time, int originId, int destinationId)
        {
            //if (time == null) Connection.Time = DateTime.Now;
            //else Connection.Time = time;

            UriBuilder fullUri = new UriBuilder(API_BASE + TRIP_REQUEST);
            fullUri.Query = "language=de"
                + "&outputFormat=XML"
                + "&coordListOutputFormat=STRING"
                + "&coordOutputFormat=WGS84"
                + "&calcNumberOfTrips=4"
                + "&type_origin=stop&name_origin=" + originId
                + "&type_destination=stop&name_destination=" + destinationId
                + "&itdDate=" + string.Format("{0:yyyyMMdd}", time)
                + "&itdTime=" + string.Format("{0:HHmm}", time)
                + "&itdTripDateTimeDepArr=dep"

                + "&ptOptionsActive=1"
                + "&changeSpeed=normal"
                // 0="R-Bahn", 1="IR", 2="U-Bahn", 4="Tram, 5="Stadtbus", 6="Regional-Bus", 7="", 11="Others"
                + "&includedMeans=checkbox&inclMOT_0=on&inclMOT_1=on&inclMOT_2=on&inclMOT_4=on&inclMOT_5=on&inclMOT_7=on&inclMOT_11=on"
                + "&locationServerActive=1"
                + "&useRealtime=1";

            HttpWebRequest getConnectionsRequest = (HttpWebRequest)WebRequest.Create(fullUri.Uri);   ///this is main

            // set up the state object for the async request
            QueryUpdateState queryUpdateState = new QueryUpdateState();
            queryUpdateState.AsyncRequest = getConnectionsRequest;

            // start the asynchronous request
            getConnectionsRequest.BeginGetResponse(new AsyncCallback(HandleConnectionsResponse), queryUpdateState); // this is still main thread
        }

        /// <summary>
        /// Handle the response from the connections query
        /// </summary>
        /// <param name="asyncResult"></param>
        private void HandleConnectionsResponse(IAsyncResult asyncResult)  ///now this is background
        {
            // get the state information
            QueryUpdateState queryUpdateState = (QueryUpdateState)asyncResult.AsyncState;
            HttpWebRequest getConnectionsRequest = (HttpWebRequest)queryUpdateState.AsyncRequest;

            // end the async request
            queryUpdateState.AsyncResponse = (HttpWebResponse)getConnectionsRequest.EndGetResponse(asyncResult);

            Stream streamResult;
            XElement itdTripRequest;
            XElement routesElem;

            ObservableCollection<Connection> connectionList = new ObservableCollection<Connection>();

            try
            {
                // get the stream containing the response from the async call
                streamResult = queryUpdateState.AsyncResponse.GetResponseStream();

                // load the XML

                XElement tripResponse = XElement.Load(streamResult);

                //int totalResults = xmlStations.Descendants("odvNameElem").Count();


                itdTripRequest = tripResponse.Descendants("itdTripRequest").FirstOrDefault();
                routesElem = itdTripRequest.Descendants("itdRouteList").FirstOrDefault();
            }
            catch (FormatException fe)
            {
                throw new Exception("Fehler bei GetResponseStream()", fe.InnerException);
                // there was some kind of error processing the response from the web
                // additional error handling would normally be added here
            }

            // <itdRouteList>
            //   <itdRoute changes="2" vehicleTime="27" publicDuration="00:46">
            //     <itdPartialRouteList>
            //       <itdPartialRoute timeMinute="6">
            //         <itdPoint stopID="3003357" name="Erlangen Anton-Bruckner-Str." nameWO="Anton-Bruckner-Str." usage="departure" x="11017366" y="49588410" mapName="WGS84" locality="Erlangen">
            //           <itdDateTime>
            //             <itdDate year="2011" month="4" day="26" weekday="3"/>
            //             <itdTime hour="9" minute="48"/>
            //           </itdDateTime>
            //         </itdPoint>
            //         <itdPoint stopID="3003110" name="Erlangen Hauptbahnhof" nameWO="Hauptbahnhof" usage="arrival" x="11002674" y="49596106" mapName="WGS84" locality="Erlangen">
            //           <itdDateTime>
            //             <itdDate year="2011" month="4" day="26" weekday="3"/>
            //             <itdTime hour="9" minute="54"/>
            //           </itdDateTime>
            //         </itdPoint>
            //         <itdMeansOfTransport name="Stadtbus 287" shortname="287" symbol="287" type="3" motType="5" productName="Stadtbus" destination="Steudach Westfriedhof" destID="3003785" />
            //         <itdStopSeq>
            //           <itdPoint stopID="3003357" name="Erlangen Anton-Bruckner-Str." x="" y="" mapName="NAV4"/>
            //           <itdPoint stopID="3003375" name="Erlangen Sophienstr." x="" y="" mapName="NAV4"/>
            //           <itdPoint stopID="3003151" name="Erlangen Arcaden" x="" y="" mapName="NAV4"/>
            //           <itdPoint stopID="3003110" name="Erlangen Hauptbahnhof" x="" y="" mapName="NAV4"/>
            //         </itdStopSeq>
            //       </itdPartialRoute>
            //       <itdPartialRoute/>
            //       <itdPartialRoute/>
            //     </itdPartialRouteList>
            //   </itdRoute>

            //<itdMeansOfTransport name="ICE 1517 Intercity-Express" shortname="1517" symbol="" type="6" motType="0" trainType="ICE" trainName="Intercity-Express" 
            //    destination="München Hauptbahnhof" network="ddb" TTB="1" STT="1" ROP="0" destID="1008803" spTr="">
            //  <motDivaParams line="98028" project="j11" direction="H" supplement="X" network="ddb" />
            //  <itdOperator>
            //    <code>00</code>
            //    <name>Daten DB AG (sonst)</name>
            //  </itdOperator>
            //</itdMeansOfTransport>

            if (routesElem != null)
            {
                int connectionIndex = 1;

                foreach (var route in routesElem.Descendants("itdRoute"))
                {
                    if (route.HasAttributes)
                    {
                        //temp.Changes = int.Parse(route.Attribute("changes").Value);
                        //temp.Duration = route.Attribute("publicDuration").Value + "h";

                        IEnumerable<XElement> partialRoutes = route.Descendants("itdPartialRoute");

                        List<Connection.Part> parts = new List<Connection.Part>();
                        foreach (XElement partialRoute in partialRoutes)
                        {
                            // Means of Transport: Linie 287, Büchenbach/ER Mönaustr.
                            XElement partMotElem = partialRoute.Descendants("itdMeansOfTransport").FirstOrDefault();

                            bool isFootWay = false;

                            int timeMinute = -1;
                            // check if it's footway
                            if (partMotElem.Attribute("type").Value.Equals("99"))
                            {
                                timeMinute = int.Parse(partialRoute.Attribute("timeMinute").Value);
                                isFootWay = true;
                            }

                            Line line = null;
                            Location destination = null;
                            string[] colors = { "#FFFFFF", "#000000" };

                            if (!isFootWay)
                            {
                                string lineName = partMotElem.Attribute("shortname").Value;

                                string lineType = partMotElem.Attribute("productName") != null ? partMotElem.Attribute("productName").Value : partMotElem.Attribute("trainType").Value;
                                switch (lineType)
                                {
                                    case "IC":
                                    case "ICE":
                                        colors[0] = "#000000";
                                        colors[1] = "#FFFFFF";
                                        lineName = lineType + " " + lineName;
                                        break;
                                    case "Stadtbus":
                                        colors[1] = "#DC143C";
                                        break;
                                    case "U-Bahn":
                                        colors[1] = "#191970";
                                        break;
                                    case "Straßenbahn":
                                        colors[1] = "#330066";
                                        break;
                                    case "S-Bahn":
                                    case "R-Bahn":
                                        colors[1] = "#3CB371";
                                        break;
                                    default:
                                        colors[1] = "FFAAFF";
                                        break;
                                }

                                line = new Line(lineName, colors, lineType);
                                destination = new Location(LocationType.STATION, int.Parse(partMotElem.Attribute("destID").Value), lineType, partMotElem.Attribute("destination").Value);
                            }
                            else
                            {
                                colors[1] = "#AAAAAA";
                                line = new Line("Fussweg", colors, "Walk");
                                destination = new Location(LocationType.STATION, -1, "Fussweg", "");
                            }

                            // Departure: Erlangen, Sophienstr.
                            XElement partDepElem = partialRoute.Descendants("itdPoint").Where(x => x.Attribute("usage").Value.Equals("departure")).FirstOrDefault();
                            Location departure = new Location(LocationType.STATION, int.Parse(partDepElem.Attribute("stopID").Value), partDepElem.Attribute("locality").Value, partDepElem.Attribute("name").Value);

                            XElement partDepTimeElem = partDepElem.Descendants("itdTime").FirstOrDefault();
                            XElement partepDateElem = partDepElem.Descendants("itdDate").FirstOrDefault();

                            DateTime partDepTime = new DateTime(int.Parse(partepDateElem.Attribute("year").Value), int.Parse(partepDateElem.Attribute("month").Value), int.Parse(partepDateElem.Attribute("day").Value),
                                int.Parse(partDepTimeElem.Attribute("hour").Value), int.Parse(partDepTimeElem.Attribute("minute").Value), 0);

                            // Arrival: Nürnberg, Gostenhof
                            XElement partArrElem = partialRoute.Descendants("itdPoint").Where(x => x.Attribute("usage").Value.Equals("arrival")).LastOrDefault();
                            Location arrival = new Location(LocationType.STATION, int.Parse(partArrElem.Attribute("stopID").Value), partArrElem.Attribute("locality").Value, partArrElem.Attribute("name").Value);

                            XElement partArrTimeElem = partArrElem.Descendants("itdTime").FirstOrDefault();
                            XElement partArrDateElem = partArrElem.Descendants("itdDate").FirstOrDefault();

                            DateTime partArrTime = new DateTime(int.Parse(partArrDateElem.Attribute("year").Value), int.Parse(partArrDateElem.Attribute("month").Value), int.Parse(partArrDateElem.Attribute("day").Value),
                                int.Parse(partArrTimeElem.Attribute("hour").Value) == 24 ? 0 : int.Parse(partArrTimeElem.Attribute("hour").Value), int.Parse(partArrTimeElem.Attribute("minute").Value), 0);

                            //if (!isFootWay)
                            parts.Add(new Connection.Trip(line, destination, partDepTime, "departurePos", departure, partArrTime, "arrivalPos", arrival, new List<Stop>(), new List<PTE.DTO.Point>()));
                            //else
                            //    parts.Add(new Connection.Trip.FootWay(timeMinute, departure, arrival));
                        }

                        try
                        {
                            // DEPARTURE
                            XElement departureElem = route.Descendants("itdPoint").Where(x => x.Attribute("usage").Value.Equals("departure")).FirstOrDefault();
                            Location from = new Location(LocationType.STATION, int.Parse(departureElem.Attribute("stopID").Value), departureElem.Attribute("locality").Value, departureElem.Attribute("name").Value);

                            XElement depTimeElem = departureElem.Descendants("itdTime").FirstOrDefault();
                            XElement depDateElem = departureElem.Descendants("itdDate").FirstOrDefault();

                            DateTime departureTime = new DateTime(int.Parse(depDateElem.Attribute("year").Value), int.Parse(depDateElem.Attribute("month").Value), int.Parse(depDateElem.Attribute("day").Value),
                                int.Parse(depTimeElem.Attribute("hour").Value), int.Parse(depTimeElem.Attribute("minute").Value), 0);

                            // ARRIVAL
                            XElement arrivalElem = route.Descendants("itdPoint").Where(x => x.Attribute("usage").Value.Equals("arrival")).LastOrDefault();
                            Location to = new Location(LocationType.STATION, int.Parse(arrivalElem.Attribute("stopID").Value), arrivalElem.Attribute("locality").Value, arrivalElem.Attribute("name").Value);

                            XElement arrTimeElem = arrivalElem.Descendants("itdTime").FirstOrDefault();
                            XElement arrDateElem = arrivalElem.Descendants("itdDate").FirstOrDefault();

                            DateTime arrivalTime = new DateTime(int.Parse(arrDateElem.Attribute("year").Value), int.Parse(arrDateElem.Attribute("month").Value), int.Parse(arrDateElem.Attribute("day").Value),
                                int.Parse(arrTimeElem.Attribute("hour").Value), int.Parse(arrTimeElem.Attribute("minute").Value), 0);

                            // LINIE
                            XElement motElem = route.Descendants("itdMeansOfTransport").FirstOrDefault();

                            string publicDuration = route.Attribute("publicDuration").Value;
                            connectionList.Add(new Connection(connectionIndex.ToString(), publicDuration, departureTime, arrivalTime, from, to, parts, new List<IFare>()));

                            connectionIndex++;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Fehler im HandleConnectionsResponse", ex.InnerException);
                        }
                    }
                }
            }


            OnSearchConnectionsCompleted(new SearchConnectionsCompletedEventArgs(null, false, "UserState", connectionList));

            //if (connectionList.Count == 0)
            //{
            //    ResultTextBox.Dispatcher.BeginInvoke(
            //      new Action(
            //        delegate()
            //        {
            //            ResultTextBox.Text = ResultTextBox.Text + "\n\nKeine Verbindung gefunden.";
            //        }
            //    ));

            //    return;
            //}

            //foreach (Connection item in connectionList)
            //{
            //    ResultTextBox.Dispatcher.BeginInvoke(
            //      new Action(
            //        delegate()
            //        {
            //            ResultTextBox.Text = ResultTextBox.Text + "\n\n" + item.DepartureTime.ToString("HH:mm") + " -> " + item.ArrivalTime.ToString("HH:mm") + " (Dauer: " + item.PublicDuration + ")" + ":\n";

            //            foreach (Connection.Trip trip in item.Parts)
            //            {
            //                ResultTextBox.Text = ResultTextBox.Text + "\n" + trip.DepartureTime.ToString("HH:mm") + " - [" + trip.Line.Label + "] -> " + trip.Departure.Name + "<->"+ trip.Arrival.Name;
            //            }
            //        }
            //    ));

            //    System.Threading.Thread.Sleep(50);
            //}


            //ResultTextBox.Dispatcher.BeginInvoke(
            //  new Action(
            //    delegate()
            //    {
            //        ConnectionVisual.SetConnections(connectionList);
            //    }));
        }



        /// <summary>
        /// State information for our BeginGetResponse async call
        /// </summary>
        public class QueryUpdateState
        {
            public HttpWebRequest AsyncRequest { get; set; }
            public HttpWebResponse AsyncResponse { get; set; }
        }
    }
}
