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
using BusCon.PTE;
using BusCon.PTE.DTO;
using System.Linq;
using System.Collections.Generic;
using BusCon.Utility;
using System.Net.NetworkInformation;
using System.Collections.ObjectModel;

namespace BusCon.ViewModels
{
    public class DepartureViewModel : Screen
    {
        readonly INavigationService navigationService;
        private ConnectionQueryViewModel queryVM;

        public ObservableCollection<DepartureResultViewModel> Departures { get; set; }

        public DepartureViewModel(INavigationService navigationService, ConnectionQueryViewModel queryVM)
        {
            this.IsDataConnectionAvailable = true;

            DateTimeField = DateTime.Now;
            this.navigationService = navigationService;
            this.queryVM = queryVM;
            this.Departures = new ObservableCollection<DepartureResultViewModel>();
        }


        public int HistoryIndex { get; set; }

        private List<StationIdentifier> HistoryItems = new List<StationIdentifier> 
        { 
            { new StationIdentifier { DepartureId = 3003357, DepartureStation = "Anton-Bruckner-Str., Erlangen", ArrivalId = 3000703, ArrivalStation = "Nürnberg, Gostenhof" }},
            { new StationIdentifier { DepartureId = 3003373, DepartureStation = "Schenkstr., Erlangen", ArrivalId = 3003859, ArrivalStation = "St. Johann, Erlangen-Büchenbach" }},
            { new StationIdentifier { DepartureId = 3003373, DepartureStation = "Erlangen, Stintzingstr.", ArrivalId = 3003859, ArrivalStation = "Nürnberg, Plärrer" }}
        };


        private int _departureId;
        private string _departureField;
        private bool _isDataConnectionAvailable;
        private bool _isDownloadingDepartures;
        private bool _hasDepartures;

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

        public bool HasDepartures
        {
            get
            {
                return this._hasDepartures;
            }
            private set
            {
                this._hasDepartures = value;
                this.NotifyOfPropertyChange("HasDepartures");
            }
        }

        public bool IsDataConnectionAvailable
        {
            get
            {
                return this._isDataConnectionAvailable;
            }
            private set
            {
                this._isDataConnectionAvailable = value;
                this.NotifyOfPropertyChange("IsDataConnectionAvailable");
            }
        }

        public bool IsDownloadingDepartures
        {
            get
            {
                return this._isDownloadingDepartures;
            }
            private set
            {
                this._isDownloadingDepartures = value;
                this.NotifyOfPropertyChange("IsDownloadingDepartures");
            }
        }

        DateTime? dateTimeField;
        public DateTime? DateTimeField
        {
            get { return dateTimeField; }
            set { dateTimeField = value; NotifyOfPropertyChange(() => DateTimeField); }
        }

        public string DateField { get; set; }
        public string TimeField { get; set; }

        public void GoToSearchViewDeparture()
        {
            navigationService.UriFor<StationsViewModel>().WithParam(x => x.SearchSource, "DepartureView").Navigate();
        }

        protected override void OnActivate()
        {
            if (HistoryIndex != -1)
            {
                DepartureField = HistoryItems[HistoryIndex].DepartureStation;
                _departureId = HistoryItems[HistoryIndex].DepartureId;
            }
            base.OnActivate();
        }

        public void SearchDepartures()
        {
            DateTime? dateTime = DateTimeField;
            DateTime depDateTime = dateTime != null ? (DateTime)dateTime : DateTime.Now;

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                this.IsDataConnectionAvailable = true;
                this.IsDownloadingDepartures = true;
                this.Departures.Clear();

                queryVM.StationId = _departureId;
                queryVM.MaxDepartures = 20;
                queryVM.Equivs = false;
                queryVM.ForceUpdate = false;

                queryVM.QueryDepartures(new Action<QueryDeparturesResult>(this.OnSearchDeparturesCompleted));
            }
            else
            {
                this.IsDataConnectionAvailable = false;
                this.IsDownloadingDepartures = false;
            }
        }

        private void OnSearchDeparturesCompleted(QueryDeparturesResult result)
        {
            if (result.Status == QueryDeparturesResult.DepartureResultStatuses.OK && result.StationDepartures.Count > 0)
            {
                List<DepartureResultViewModel> list = new List<DepartureResultViewModel>();
                foreach (Departure departure in result.StationDepartures[0].Departures)
                {
                    DepartureResultViewModel departureViewModel = new DepartureResultViewModel()
                    {
                        Line = departure.Line,
                        Station = departure.Destination,
                        InMin = departure.Countdown,
                        PlannedDepartureTime = new DateTime?(departure.DepartureTime)
                    };
                    if (!list.Contains(departureViewModel))
                        list.Add(departureViewModel);
                }

                IOrderedEnumerable<DepartureResultViewModel> orderedDepartures = Enumerable
                    .OrderBy<DepartureResultViewModel, int>
                    (
                        (IEnumerable<DepartureResultViewModel>)list, 
                        (Func<DepartureResultViewModel, int>)(c => c.InMin)
                    );

                AsyncHelper.RunOnMainThread((System.Action)(() =>
                {
                    this.IsDownloadingDepartures = false;
                    if (Enumerable.Count<DepartureResultViewModel>((IEnumerable<DepartureResultViewModel>)orderedDepartures) <= 0)
                    {
                        this.HasDepartures = false;
                    }
                    else
                    {
                        foreach (DepartureResultViewModel item_1 in (IEnumerable<DepartureResultViewModel>)orderedDepartures)
                            this.Departures.Add(item_1);
                    }
                }));
            }
            else
            {
                if (result.Status == QueryDeparturesResult.DepartureResultStatuses.INVALID_STATION)
                    return;
                int num = (int)result.Status;
            }
        }
    }
}
