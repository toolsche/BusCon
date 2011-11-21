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
using System.Collections.Generic;

namespace BusCon.PTE.DTO
{
    public sealed class NearbyStationsResult
    {
        public readonly Status status;
        public readonly List<Location> stations;

        public NearbyStationsResult(List<Location> stations)
        {
            this.status = Status.OK;
            this.stations = stations;
        }

        public NearbyStationsResult(Status status)
        {
            this.status = status;
            this.stations = null;
        }

        public enum Status
        {
            OK, 
            INVALID_STATION, 
            SERVICE_DOWN
        }
    }
}
