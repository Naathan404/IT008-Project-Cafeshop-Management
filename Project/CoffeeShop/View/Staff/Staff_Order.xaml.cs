using CoffeeShop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

            //Thêm bàn
            cbTable.ItemsSource = new List<string>()
            {
                "Không","Bàn 1", "Bàn 2", "Bàn 3", "Bàn 4", "Bàn 5", "Bàn 6", "Bàn 7", "Bàn 8", "Bàn 9"
            };
            cbTable.SelectedIndex = 0; //Set mặc định là Không
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

        private void ItemsContainer_SizeChanged(object sender, SizeChangedEventArgs e) //Căn chỉnh items
        {
            if (sender is not ScrollViewer sv) return;

            // Tìm UniformGrid
            if (sv.Content is not UniformGrid ug) return;

            double w = sv.ActualWidth;

            // Tránh lỗi khi width chưa đo được
            if (w <= 50) return;

            int minItemWidth = 150;

            // Tính số cột
            int columns = Math.Max(1, (int)(w / minItemWidth));

            ug.Columns = columns;
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
