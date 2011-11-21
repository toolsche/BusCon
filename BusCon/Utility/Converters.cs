using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace BusCon.Utility
{
    public class DepartureToWidthConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {   
            var startTime = ((DateTime)value).AddMinutes(-15);
            var tripStart = (DateTime)value;
            int waitTime = (int)(tripStart - startTime).TotalMinutes;
            return waitTime * 6;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class DepartureToMarginConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int waitTime = (int)value;
            //var startTime = ((DateTime)value).AddMinutes(-15);
            //var tripStart = (DateTime)value;
            //int waitTime = (int)(tripStart - startTime).TotalMinutes;
            return new Thickness(waitTime * 6, 3, 2, 0);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class DepartureToWidthConverter2 : IMultiValueConverter
    {
        #region IValueConverter Members
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var startTime = values[1] != null ? (DateTime)values[1] : DateTime.Now;
            var tripStart = (DateTime)values[0];
            int waitTime = (int)(startTime - tripStart).TotalMinutes;
            
            if (waitTime < 0)
                return 0;

            if (waitTime > 100)
                return 100;

            return waitTime * 6;
        }
        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class DepartureToMarginConverter2 : IMultiValueConverter
    {
        #region IValueConverter Members
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var startTime = values[1] != null ? (DateTime)values[1] : DateTime.Now;
            var tripStart = (DateTime)values[0];
            int waitTime = (int)(tripStart - startTime).TotalMinutes;
            //if (waitTime <= 0)
            // waitTime = 0;
            return new Thickness(waitTime * 2, 3, 2, 0);
        }
        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class DurationToStringConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeSpan = (TimeSpan)value;
            return timeSpan.Hours.ToString("00") + ":" + timeSpan.Minutes.ToString("00");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class DurationToWidthConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value * 6;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class WidthSansMarginConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(double) || value == null) { return null; }
            double dParentWidth = Double.Parse(value.ToString(), CultureInfo.InvariantCulture);
            double dAdjustedWidth = dParentWidth - 180;
            if (dAdjustedWidth < 30)
            {
                dAdjustedWidth = 30;
            }
            if (dAdjustedWidth > 200)
            {
                dAdjustedWidth = 200;
            }
            return dAdjustedWidth;
            //return (dAdjustedWidth < 100 ? 100 : dAdjustedWidth);
        }

        /// <summary>
        /// Invert a boolean value.
        /// </summary>
        /// <param name="value">Boolean value.</param>
        /// <param name="targetType">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Inverted Value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Boolean.Parse(value.ToString()))
                return Visibility.Visible;
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
