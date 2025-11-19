using System;
using System.Collections.Generic;
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
            myCard.Tag = "/Assets/Images/imgItemExample.jpg"; // đường dẫn tới ảnh trong project
            myCard.SizeChanged += ImgItem_Loaded;
            myCard.Content = "Cà phê";
        }
        private void ImgItem_Loaded(object sender, RoutedEventArgs e)
        {
            var img = sender as Image;
            if (img == null) return;

            double cropWidth = img.ActualWidth;
            double cropHeight = img.ActualHeight;

            double x = (img.ActualWidth - cropWidth) / 2;
            double y = (img.ActualHeight - cropHeight) / 2;

            img.Clip = new RectangleGeometry()
            {
                Rect = new Rect(x, y, cropWidth, cropHeight),
                RadiusX = 15,
                RadiusY = 15
            };
        }

    }
}
