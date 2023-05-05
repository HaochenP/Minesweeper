
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Minesweeper
{
    public partial class MovingObject : UserControl
    {
        public MovingObject()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += MovingObjectMouseDown;
        }

        private void MovingObjectMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
