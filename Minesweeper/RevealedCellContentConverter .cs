using System;
using System.Globalization;
using System.Windows.Data;

namespace Minesweeper
{
    public class RevealedCellContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int neighboringMines = (int)value;
            return neighboringMines > 0 ? neighboringMines.ToString() : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
