using CoffeeShop.Models;
using CoffeeShop.View.Controls;
using CoffeeShop.ViewModels.GeneralVM;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Esf;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace CoffeeShop.View.General
{
    /// <summary>
    /// Interaction logic for OrderDetails.xaml
    /// </summary>
    public partial class OrderDetailWindow : Window
    {
        public OrderDetailWindow(int orderID)
        {
            InitializeComponent();
            this.DataContext = new OrderDetailViewModel(orderID);
        }

        private void border_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border bdr)
            {
                bdr.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#766839"));
                var tb = bdr.Child as TextBlock;
                if (tb != null)
                    tb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EDE2D3"));

                bdr.Effect = new DropShadowEffect
                {
                    Color = (Color)ColorConverter.ConvertFromString("#766839"),
                    Direction = 315,
                    ShadowDepth = 4,
                    BlurRadius = 10,
                    Opacity = 0.6
                };
            }
        }

        private void border_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border bdr)
            {
                bdr.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EDE2D3"));
                var tb = bdr.Child as TextBlock;
                if (tb != null)
                    tb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#766839"));

                bdr.Effect = null;
            }
        }
        private void bdrQuit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
