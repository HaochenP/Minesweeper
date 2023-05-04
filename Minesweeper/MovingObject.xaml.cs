using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MovingObject.xaml
    /// </summary>
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
            /*
            var parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mainWindow)
            {
                mainWindow.amongusTimer.Stop();
            }*/
        }
    }
}
