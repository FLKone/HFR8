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
using System.Windows.Data;

namespace HFR7.HFRClasses
{
    public class String2UriConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new Uri("/ListTopics.xaml?souscaturi=/hfr/" + GetCatName.ShortNameFromId(GetCatName.IdFromPlainName((string)value)) + "/liste_sujet-1.htm&listpagenumber=1&idcat=" + GetCatName.IdFromPlainName((string)value) + "&pivot=drap&from=favorislist", UriKind.Relative);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
