using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace Minesweeper
{
    public class RevealedNeighbouringMinesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isRevealed = (bool)values[0];
            int neighboringMines = (int)values[1];
            bool isMine = (bool)values[2];

            if (isRevealed)
            {
                if (!isMine)
                { 
                return neighboringMines > 0 ? neighboringMines.ToString() : string.Empty;
                } 
                else
                {
                    return "💣";
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
