using Microsoft.Phone.Tasks;
using System;
using System.Windows;
using BusCon.ViewModels;
using System.Collections.Generic;
using Caliburn.Micro;

namespace BusCon.Utility
{
    public class GlobalExceptionHandler
    {
        private readonly IEventAggregator eventAggregator;

        //public static GlobalExceptionHandler(IEventAggregator eventAggregator)
        //{
        //    this.eventAggregator = eventAggregator;
        //}

        public static IEnumerable<IResult> HandleException(Exception ex)
        {
            var showDialog = new ShowDialog<ExceptionViewModel>();
            yield return showDialog;

            var sendException = showDialog.Dialog.SendException;
            if (sendException)
            {
                EmailComposeTask emailCompose = new EmailComposeTask();
                emailCompose.To = "toolsche@gmail.com";
                emailCompose.Subject = ex.Message;

                string exceptionString = ex.ToString();
                for (Exception innerException = ex.InnerException; innerException != null; innerException = innerException.InnerException)
                    exceptionString = exceptionString + "\n\n" + "INNER EXCEPTION:\n" + ((object)innerException).ToString();

                string exceptionWithVersion = exceptionString + "APPLICATION VERSION: " + AppResources.ApplicationVersion;
                emailCompose.Body = exceptionWithVersion;
                emailCompose.Show();

                //yield return new ShowDialog<MessageViewModel>()
                //    .Init(x => x.Message = "The user entered: " + result);
            }
        }

        //public IEnumerable<IResult> HandleException(Exception ex)
        //{
        //    var question = new Dialog<Answer>(DialogType.Question,
        //                      "Isn't this a nice way to create a Dialog Window?",
        //                      Answer.Yes,
        //                      Answer.No);

        //    yield return question.AsResult();

        //    if (question.GivenResponse == Answer.Yes)
        //        eventAggregator.RequestTask<EmailComposeTask>(ConfigureContactTask);
        //    else
        //        yield break;
        //}

        //private void ConfigureContactTask()
        //{ 
        //}

        //public static void HandleException(Exception ex)
        //{
        //    //eventAggregator.RequestTask<EmailComposeTask>(x =>
        //    //{
        //    //    x.To = "info@site.org";
        //    //    x.Subject = "Comment from app";
        //    //});

        //    Deployment.Current.Dispatcher.BeginInvoke((System.Action)(() =>
        //    {
        //        if (MessageBox.Show(AppResources.MessageError, AppResources.CaptionError, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        //            return;

        //        EmailComposeTask emailCompose = new EmailComposeTask();
        //        emailCompose.To = "toolsche@gmail.com";
        //        emailCompose.Subject = ex.Message;

        //        string exceptionString = ex.ToString();
        //        for (Exception innerException = ex.InnerException; innerException != null; innerException = innerException.InnerException)
        //            exceptionString = exceptionString + "\n\n" + "INNER EXCEPTION:\n" + ((object)innerException).ToString();

        //        //if (CurrentConnection.LocationFrom != null && CurrentConnection.LocationTo != null)
        //        //    exceptionString = 
        //        //        exceptionString +
        //        //        "\n\nLocationFrom = " + CurrentConnection.LocationFrom.ToDebugString() +
        //        //        "\nLocationTo = " + CurrentConnection.LocationTo.ToDebugString() +
        //        //        "\nTime = " + CurrentConnection.Time.ToString();

        //        string exceptionWithVersion = exceptionString + "APPLICATION VERSION: " + AppResources.ApplicationVersion;
        //        emailCompose.Body = exceptionWithVersion;
        //        emailCompose.Show();
        //    }));
        //}
    }
}
