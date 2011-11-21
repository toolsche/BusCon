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
using System.ComponentModel;
using BusCon.ViewModels;
using System.Collections.Generic;

namespace BusCon.Utility
{
    public sealed class SearchLocationsCompletedEventArgs : AsyncCompletedEventArgs
    {
        #region Fields

        private ItemViewModel m_station;
        private List<ItemViewModel> m_stations;

        #endregion

        internal SearchLocationsCompletedEventArgs(Exception error, Boolean cancelled, Object userState, ItemViewModel station)
            : base(error, cancelled, userState)
        {
            #region Initialize the state.

            if (station != null)
            {
                m_station = station;
            }

            #endregion
        }

        internal SearchLocationsCompletedEventArgs(Exception error, Boolean cancelled, Object userState, List<ItemViewModel> stations)
            : base(error, cancelled, userState)
        {
            #region Initialize the state.

            if (stations != null)
            {
                m_stations = stations;
            }

            #endregion
        }

        #region Properties

        /// <summary>
        /// Gets the Stations representing the search result.
        /// </summary>
        public List<ItemViewModel> Stations
        {
            get
            {
                RaiseExceptionIfNecessary();

                return m_stations;
            }
        }

        /// <summary>
        /// Gets the Station representing the search result.
        /// </summary>
        public ItemViewModel Station
        {
            get
            {
                RaiseExceptionIfNecessary();

                return m_station;
            }
        }

        #endregion
    }
}
