using System;
using System.Collections.Generic;

using Caliburn.Micro;
using BusCon.PTE.DTO;
using BusCon.Utility;

namespace BusCon.ViewModels
{
    public class SearchViewModel : Screen
    {
        public int HistoryIndex { get; set; }

        private List<StationIdentifier> HistoryItems = new List<StationIdentifier> 
        { 
            { new StationIdentifier { DepartureId = 3003357, DepartureStation = "Anton-Bruckner-Str., Erlangen", ArrivalId = 3000703, ArrivalStation = "Nürnberg, Gostenhof" }},
            { new StationIdentifier { DepartureId = 3003373, DepartureStation = "Schenkstr., Erlangen", ArrivalId = 3003859, ArrivalStation = "St. Johann, Erlangen-Büchenbach" }},
            { new StationIdentifier { DepartureId = 3003373, DepartureStation = "Erlangen, Stintzingstr.", ArrivalId = 3003859, ArrivalStation = "Nürnberg, Plärrer" }}
        };

        readonly INavigationService navigationService;

        string _departureField;
        public string DepartureField
        {
            get { return _departureField; }
            set
            {
                string[] temp = value.Split('$');
                if (temp.Length == 2)
                {
                    HistoryIndex = -1;
                    _departureField = temp[0];
                    _departureId = int.Parse(temp[1]);
                }
                else
                {
                    _departureField = value;
                }
                NotifyOfPropertyChange(() => DepartureField);
            }
        }

        string _arrivalField;
        public string ArrivalField
        {
            get { return _arrivalField; }
            set
            {
                string[] temp = value.Split('$');
                if (temp.Length == 2)
                {
                    HistoryIndex = -1;
                    _arrivalField = temp[0];
                    _arrivalId = int.Parse(temp[1]);
                }
                else
                {
                    _arrivalField = value;
                }
                NotifyOfPropertyChange(() => ArrivalField);
            }
        }

        public string DateField { get; set; }

        public string TimeField { get; set; }

        DateTime? dateTimeField;
        public DateTime? DateTimeField
        {
            get { return dateTimeField; }
            set { dateTimeField = value; NotifyOfPropertyChange(() => DateTimeField); }
        }

        private int _departureId;
        private int _arrivalId;

        private EfaRequest efa { get; set; }
        private ConnectionQueryViewModel queryVM;

        public SearchViewModel(INavigationService navigationService, ConnectionQueryViewModel queryVM)
        {
            //DepartureField = "Anton-Bruckner-Str., Erlangen";
            //_departureId = 3003357;

            //ArrivalField = "Nürnberg, Gostenhof";
            //_arrivalId = 3000703;

            DateTimeField = DateTime.Now;

            this.navigationService = navigationService;
            this.queryVM = queryVM;
            efa = EfaRequest.Instance;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        protected override void OnActivate()
        {
            if (HistoryIndex != -1)
            {
                DepartureField = HistoryItems[HistoryIndex].DepartureStation;
                _departureId = HistoryItems[HistoryIndex].DepartureId;
                ArrivalField = HistoryItems[HistoryIndex].ArrivalStation;
                _arrivalId = HistoryItems[HistoryIndex].ArrivalId;
            }
            base.OnActivate();
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
        }

        public void GoToSearchViewDeparture()
        {
            navigationService.UriFor<StationsViewModel>().WithParam(x => x.SearchSource, "Departure").Navigate();
        }

        public void SwitchDirection()
        {
            string depTempField = DepartureField;
            int depTempId = _departureId;

            DepartureField = ArrivalField;
            _departureId = _arrivalId;

            ArrivalField = depTempField;
            _arrivalId = depTempId;
        }

        public void GoToSearchViewArrival()
        {
            navigationService.UriFor<StationsViewModel>().WithParam(x => x.SearchSource, "Arrival").Navigate();
        }

        public void SearchConnections()
        {
            //MessageBox.Show("Searching..." + " DepStop: " + departureId + " => ArrStop: " + arrivalId);

            //efa.Connection.OriginId = Convert.ToInt32("3003357");
            //efa.Connection.DestinationId = Convert.ToInt32("3000703");

            DateTime? dateTime = DateTimeField;
            DateTime depDateTime = dateTime != null ? (DateTime)dateTime : DateTime.Now;

            queryVM.Date = depDateTime;

            //efa.RetrieveConnections(depDateTime, _departureId, _arrivalId);
            navigationService.Navigate(new Uri(String.Format("/Views/ConnectionView.xaml?QueryTime={0}", depDateTime.ToString("HH:mm")), UriKind.RelativeOrAbsolute));
        }
    }
}
