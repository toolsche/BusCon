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
using System.Collections.ObjectModel;
using BusCon.PTE.DTO;

namespace BusCon.Utility
{
    public sealed class SearchConnectionsCompletedEventArgs : AsyncCompletedEventArgs
    {
        #region Fields

        private ObservableCollection<Connection> m_connections;

        #endregion

        internal SearchConnectionsCompletedEventArgs(Exception error, Boolean cancelled, Object userState, ObservableCollection<Connection> connections)
            : base(error, cancelled, userState)
        {
            #region Initialize the state.

            if (connections != null)
            {
                m_connections = connections;
            }

            #endregion
        }

        #region Properties

        /// <summary>
        /// Gets the Connections representing the search result.
        /// </summary>
        public ObservableCollection<Connection> Connections
        {
            get
            {
                RaiseExceptionIfNecessary();

                return m_connections;
            }
        }

        #endregion
    }
}
