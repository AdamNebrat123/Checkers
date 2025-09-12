using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Converters
{
    public class MarkerSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double boardSize)
            {
                // כל משבצת = boardSize/8; marker = 30% מהמשבצת
                return (boardSize / 8) * 0.3;
            }
            return 12; // fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
