using CoffeeShop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for Staff_Order.xaml
    /// </summary>
    public partial class Staff_Order : Page
    {
        public Staff_Order()
        {
            InitializeComponent();
        }
        private void ImgItem_Loaded(object sender, RoutedEventArgs e)
        {
            var img = sender as Image;
            if (img == null) return;
            img.SizeChanged += (s, e) =>
            {
                img.Clip = new RectangleGeometry()
                {
                    Rect = new Rect(0, 0, img.ActualWidth, img.ActualHeight),
                    RadiusX = 15,
                    RadiusY = 15
                };
            };
        }

        private void bdrItemSizeS_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void bdrItemSizeM_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void bdrItemSizeL_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
