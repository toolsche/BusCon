using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using BusCon.ViewModels;

namespace BusCon 
{
    public class MainPageViewModel
    {
        readonly INavigationService navigationService;

        public MainPageViewModel(INavigationService navigationService, IWindowManager windowManager, Func<MessageViewModel> messageViewModelFactory)
        {
            this.navigationService = navigationService;
            this.windowManager = windowManager;
            this.messageViewModelFactory = messageViewModelFactory;
        }

        public void ShowDepartureView()
        {
            navigationService.Navigate(new Uri("/Views/DepartureView.xaml", UriKind.RelativeOrAbsolute));
        }

        public void ShowConnectionView()
        {
            navigationService.Navigate(new Uri("/Views/SearchView.xaml", UriKind.RelativeOrAbsolute));
        }

        public void ShowHistoryView()
        {
            navigationService.Navigate(new Uri("/Views/HistoryView.xaml", UriKind.RelativeOrAbsolute));
        }

        public void ShowSettingsView()
        {
            navigationService.Navigate(new Uri("/Views/SettingsView.xaml", UriKind.RelativeOrAbsolute));
        }

        public void ShowConnectionsViewNew()
        {
            navigationService.Navigate(new Uri("/Views/ConnectionsViewNew.xaml", UriKind.RelativeOrAbsolute));
        }

        #region OriginalCode
        IWindowManager windowManager;
        Func<MessageViewModel> messageViewModelFactory;

        //public void GotoPageTwo()
        //{
        //    navigationService.UriFor<HistoryViewModel>()
        //        .WithParam(x => x.NumberOfTabs, 5)
        //        .Navigate();
        //}

        //public void ShowMessageAsPopup() {
        //    var msgVM = messageViewModelFactory();
        //    msgVM.Message = "A message shown in the popup.\nSecond row of message shown in the popup.";
        //    windowManager.ShowPopup(msgVM);
        //}

        public void ShowMessageAsDialog()
        {
            var msgVM = messageViewModelFactory();
            msgVM.Message = "A message shown in the dialog.\nSecond row of message shown in the dialog.";
            windowManager.ShowDialogWithFeedback(msgVM);
        }

        public IEnumerable<IResult> ShowInputDialog()
        {
            var showDialog = new ShowDialog<DialogViewModel>();
            yield return showDialog;

            var result = showDialog.Dialog.Result;
            if (result != null)
            {
                yield return new ShowDialog<MessageViewModel>()
                    .Init(x => x.Message = "The user entered: " + result);
            }
        }

        public void ShowMessageBox()
        {
            MessageBox.Show("Some message...", "WP7 native Message Box", MessageBoxButton.OK);
        }
        #endregion
    }
}