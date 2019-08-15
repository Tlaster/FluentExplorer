using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Humanizer;

namespace FluentExplorer.Common.Converters
{
    class DateHumanizerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTimeOffset dateTimeOffset)
            {
                if (dateTimeOffset.Hour > 1)
                {
                    return dateTimeOffset.ToString("f");
                }
                return dateTimeOffset.Humanize();
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
