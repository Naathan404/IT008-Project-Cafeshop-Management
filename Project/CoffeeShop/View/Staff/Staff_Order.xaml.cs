using CoffeeShop.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using static CoffeeShop.View.Staff.Staff_Order;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for Staff_Order.xaml
    /// </summary>
    public partial class Staff_Order : Page
    {
        private ICollectionView itemsView;
        ObservableCollection<OrderItem> _items = new ObservableCollection<OrderItem>();
        public Staff_Order()
        {
            InitializeComponent();
            _items = GetSampleItems();
            LoadSampleData();
            dtgListOrder.ItemsSource = _items;
        }

        //Class for Sample datas
        public class OrderItem
        {
            public int ItemId { get; set; }
            public string ItemName { get; set; }
            public int CategoryId { get; set; }
            public int Quantity { get; set; }
            public bool IsAvailable { get; set; }
            public virtual ICollection<ItemPrice> ItemPrices { get; set; } = new List<ItemPrice>();
            public string? Note { get; set; }
            public string ImagePath { get; set; } = "/Assets/Images/imgItemExample.jpg";
        }
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;

            return FindParent<T>(parentObject);
        }


        #region Sample data
        //Sample data
        public static ObservableCollection<OrderItem> GetSampleItems()
        {
            return new ObservableCollection<OrderItem>() {
                // Category 1: Cà phê (Coffee)
                new OrderItem { ItemId = 1, ItemName = "Espresso", CategoryId = 1, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 30000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 2, ItemName = "Americano", CategoryId = 1, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 32000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 37000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 3, ItemName = "Cappuccino", CategoryId = 1, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 4, ItemName = "Latte", CategoryId = 1, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 5, ItemName = "Mocha", CategoryId = 1, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 43000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 48000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 6, ItemName = "Caramel Macchiato", CategoryId = 1, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 50000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 7, ItemName = "Cà phê sữa đá", CategoryId = 1, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 28000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 33000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 8, ItemName = "Cà phê đen", CategoryId = 1, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 25000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 30000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 9, ItemName = "Bạc xỉu", CategoryId = 1, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 30000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 10, ItemName = "Cà phê dừa", CategoryId = 1, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                // Cà phê dừa: S L
                // Category 2: Trà sữa (Milk Tea)
                new OrderItem { ItemId = 11, ItemName = "Trà sữa truyền thống", CategoryId = 2, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 30000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 12, ItemName = "Trà sữa trân châu đường đen", CategoryId = 2, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 13, ItemName = "Trà sữa matcha", CategoryId = 2, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 43000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 48000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 14, ItemName = "Trà sữa socola", CategoryId = 2, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 15, ItemName = "Trà sữa khoai môn", CategoryId = 2, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 16, ItemName = "Trà sữa đào", CategoryId = 2, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 43000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 48000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 17, ItemName = "Trà sữa dâu", CategoryId = 2, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 43000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 48000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 18, ItemName = "Trà sữa Ô long", CategoryId = 2, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 32000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 37000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 19, ItemName = "Trà sữa thái xanh", CategoryId = 2, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 20, ItemName = "Trà sữa hokkaido", CategoryId = 2, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 50000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },

                // Category 3: Đá xay (Blended/Frappe)
                new OrderItem { ItemId = 21, ItemName = "Matcha đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } } } },
                //Matcha đá xay: S
                new OrderItem { ItemId = 22, ItemName = "Socola đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 43000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 48000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 23, ItemName = "Cookies & Cream đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 47000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 52000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 24, ItemName = "Caramel đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 50000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 25, ItemName = "Vanilla đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 43000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 48000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 26, ItemName = "Dâu đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 50000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 27, ItemName = "Xoài đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 50000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 28, ItemName = "Coffee đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 43000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 48000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 29, ItemName = "Mocha đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 47000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 52000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 30, ItemName = "Oreo đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 47000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 52000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },

                // Category 4: Trà (Tea) //Trà đào cam sả: S M
                new OrderItem { ItemId = 31, ItemName = "Trà đào cam sả", CategoryId = 4, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 32000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 37000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } } } },
                new OrderItem { ItemId = 32, ItemName = "Trà chanh leo", CategoryId = 4, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 30000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 33, ItemName = "Trà vải", CategoryId = 4, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 30000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 34, ItemName = "Trà dâu", CategoryId = 4, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 32000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 37000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 35, ItemName = "Trà xanh", CategoryId = 4, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 25000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 30000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 36, ItemName = "Trá Ô long", CategoryId = 4, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 28000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 33000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 37, ItemName = "Trà sen vàng", CategoryId = 4, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 30000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 38, ItemName = "Trà bưởi hồng", CategoryId = 4, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 32000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 37000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 39, ItemName = "Trà cam mật ong", CategoryId = 4, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 32000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 37000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 40, ItemName = "Trà atiso", CategoryId = 4, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 28000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 33000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },

                // Category 5: Nước ép (Juice)
                new OrderItem { ItemId = 41, ItemName = "Nước ép cam", CategoryId = 5, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 42, ItemName = "Nước ép dưa hấu", CategoryId = 5, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 30000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 43, ItemName = "Nước ép dứa", CategoryId = 5, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 32000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 37000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 44, ItemName = "Nước ép ổi", CategoryId = 5, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 32000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 37000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 45, ItemName = "Nước ép cà rót", CategoryId = 5, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 46, ItemName = "Nước ép táo", CategoryId = 5, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 47, ItemName = "Nước ép chanh leo", CategoryId = 5, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 32000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 37000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 48, ItemName = "Nước ép dâu", CategoryId = 5, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 43000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 48000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },

                // Category 6: Sinh tố (Smoothie)
                new OrderItem { ItemId = 1, ItemName = "Sinh tố Dâu", CategoryId = 6, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" }}, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" }}, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" }}}},

                new OrderItem { ItemId = 2, ItemName = "Sinh tố Xoài", CategoryId = 6, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 36000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" }},
                        new ItemPrice { Price = 41000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" }},
                        new ItemPrice { Price = 46000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" }}
                }},

                new OrderItem { ItemId = 3, ItemName = "Sinh tố Bơ", CategoryId = 6, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" }},
                        new ItemPrice { Price = 43000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" }},
                        new ItemPrice { Price = 48000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" }}
                }},

                new OrderItem { ItemId = 4, ItemName = "Sinh tố Chuối", CategoryId = 6, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 33000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" }},
                        new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" }},
                        new ItemPrice { Price = 43000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" }}
                }},

                new OrderItem { ItemId = 5, ItemName = "Sinh tố Đu đủ", CategoryId = 6, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 32000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" }},
                        new ItemPrice { Price = 37000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" }},
                        new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" }}
                }},

                new OrderItem { ItemId = 6, ItemName = "Sinh tố Mãng cầu", CategoryId = 6, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 34000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" }},
                        new ItemPrice { Price = 39000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" }},
                        new ItemPrice { Price = 44000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" }}
                }},

                new OrderItem { ItemId = 7, ItemName = "Sinh tố Việt quất", CategoryId = 6, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" }},
                        new ItemPrice { Price = 47000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" }},
                        new ItemPrice { Price = 52000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" }}
                }},

                new OrderItem { ItemId = 8, ItemName = "Sinh tố Dưa gang", CategoryId = 6, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 30000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" }},
                        new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" }},
                        new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" }}
                }},

                new OrderItem { ItemId = 9, ItemName = "Sinh tố Dừa", CategoryId = 6, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 36000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" }},
                        new ItemPrice { Price = 41000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" }},
                        new ItemPrice { Price = 46000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" }}
                }},

                new OrderItem { ItemId = 10, ItemName = "Sinh tố Thập cẩm", CategoryId = 6, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" }},
                        new ItemPrice { Price = 50000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" }},
                        new ItemPrice { Price = 55000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" }}
                }},

                // Category 7: Food
                new OrderItem { ItemId = 11, ItemName = "Bánh mì thịt", CategoryId = 7, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 25000, SizeId = null, Size = null}
                }},

                new OrderItem { ItemId = 12, ItemName = "Hamburger bò", CategoryId = 7, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 45000, SizeId = null, Size = null}
                }},

                new OrderItem { ItemId = 13, ItemName = "Hamburger gà", CategoryId = 7, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 42000, SizeId = null, Size = null}
                }},

                new OrderItem { ItemId = 14, ItemName = "Khoai tây chiên", CategoryId = 7, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 30000, SizeId = null, Size = null}
                }},

                new OrderItem { ItemId = 15, ItemName = "Xúc xích nướng", CategoryId = 7, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 35000, SizeId = null, Size = null}
                }},

                new OrderItem { ItemId = 16, ItemName = "Gà viên chiên", CategoryId = 7, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 39000, SizeId = null, Size = null}
                }},

                new OrderItem { ItemId = 17, ItemName = "Bánh flan", CategoryId = 7, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 18000, SizeId = null, Size = null}
                }},

                new OrderItem { ItemId = 18, ItemName = "Bánh su kem", CategoryId = 7, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 28000, SizeId = null, Size = null}
                }},

                new OrderItem { ItemId = 19, ItemName = "Bánh quy bơ", CategoryId = 7, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 25000, SizeId = null, Size = null}
                }},

                new OrderItem { ItemId = 20, ItemName = "Pizza mini", CategoryId = 7, IsAvailable = true,
                    ItemPrices = new List<ItemPrice> {
                        new ItemPrice { Price = 50000, SizeId = null, Size = null}
                }},

            };
        }
        #endregion

        #region tabItem Loading
        //Load items vào từng tabItem theo categoryId
        private void LoadItem(ObservableCollection<OrderItem> itemsList) 
        {
            if (itemsList == null || itemsList.Count == 0)
                return;
            for (int i = 1; i <= itemsList.Max(x => x.CategoryId); i++)
            {
                // Lấy tên TabItem và UniformGrid
                var ic = tabMain.FindName("icCategory" + i) as ItemsControl;
                if (ic != null)
                {
                    ic.ItemsSource = null; // Xóa dữ liệu cũ trước khi nạp dữ liệu mới
                    ic.ItemsSource = new ObservableCollection<OrderItem>(itemsList.Where(x => x.CategoryId == i));
                }
            }
        }

        //Load sample data
        private void LoadSampleData()
        {
            //Load items cho từng tabItem
            LoadItem(_items);
            //Load bàn
            cbTable.ItemsSource = new List<string>()
            {
                "Không","Bàn 1", "Bàn 2", "Bàn 3", "Bàn 4", "Bàn 5", "Bàn 6", "Bàn 7", "Bàn 8", "Bàn 9"
            };
        }

        #endregion

        #region Datagrid Events
        private void AddItemToDataGrid(OrderItem item)
        {
            if (item == null) return;

            // Kiểm tra item đã tồn tại trong DataGrid chưa (theo ItemId)
            var existingItem = _items.FirstOrDefault(x => x.ItemId == item.ItemId);
            if (existingItem != null)
            {
                // Nếu muốn, tăng số lượng thay vì thêm mới
                existingItem.Quantity += 1;
                // Cập nhật giá nếu cần
                existingItem.SelectedPrice = item.SelectedPrice;
            }
            else
            {
                // Thêm item mới
                _orderItems.Add(item);
            }
        }

        #endregion

        #region ItemCard Events
        //private void ImgItem_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var img = sender as Image;
        //    if (img == null) return;
        //    img.SizeChanged += (s, e) =>
        //    {
        //        img.Clip = new RectangleGeometry()
        //        {
        //            Rect = new Rect(0, 0, img.ActualWidth, img.ActualHeight),
        //            RadiusX = 15,
        //            RadiusY = 15
        //        };
        //    };
        //}
        private void DisplayPrice(string txtblName, Border bdrItem, decimal price) // Hiển thị giá lên TextBlock
        {
            var txtbPrice = bdrItem.FindName(txtblName) as TextBlock;
            if (txtbPrice != null)
                txtbPrice.Text = String.Format("{0:N0} VND", price);
        }
        private void ItemPrice_Changed(TextBlock txbl, decimal price)
        {
            if (txbl == null)
                return;

            txbl.Text = string.Format("{0:N0} VND", price);
        }
        private decimal GetPriceFromBorder(Border bdrItemSize) // Lấy giá theo size được click
        {
            if (bdrItemSize == null) return 0;

            // Lấy tên size từ TextBlock bên trong
            if (!(bdrItemSize.Child is TextBlock txtSize)) return 0;
            string sizeName = txtSize.Text;

            // Lấy item từ DataContext của StackPanel cha
            var stackPanel = FindParent<StackPanel>(bdrItemSize);
            if (stackPanel == null) return 0;

            var item = stackPanel.DataContext as OrderItem;
            if (item == null || item.ItemPrices == null) return 0;

            // Tìm price tương ứng size
            var selectedPrice = item.ItemPrices.FirstOrDefault(p => p.Size.SizeName == sizeName)?.Price ?? 0;

            return selectedPrice;
        }

        private void ItemSize_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is StackPanel stackPanel)
            {
                var item = stackPanel.DataContext as OrderItem;
                if (item == null || item.ItemPrices == null) return;

                //Load size button tương ứng với item   
                var sizeList = item.ItemPrices.Select(p => p.Size).ToList(); // list size cua item
                if (sizeList.Count() == 0)
                    return;
                if (sizeList.Count == 1 && item.CategoryId == 7) // categoryId == 7 --> food không có size
                    return;

                int numOfSize = sizeList.Count;
                if (numOfSize > 0)
                {
                    var stkSizeName = stackPanel.FindName("stpnItemSize") as StackPanel;
                    stkSizeName.Visibility = Visibility.Visible;
                    for (int i = 0; i < numOfSize; i++)
                    {
                        string bdrName = "bdrItemSize" + i.ToString();
                        var bdr = stackPanel.FindName(bdrName) as Border;
                        if (bdr != null)
                        {
                            var textBlock = bdr.Child as TextBlock;
                            if (textBlock != null)
                            {
                                textBlock.Text = sizeList[i].SizeName;
                                bdr.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
            }
        }
        private void ItemPrice_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock txtblItemPrice)
            {
                var stackPanel = FindParent<StackPanel>(txtblItemPrice);
                if (stackPanel == null) return;
                var item = stackPanel.DataContext as OrderItem;
                if (item == null || item.ItemPrices == null) return;

                // Hiển thị giá mặc định (giá size đầu tiên)
                var defaultPrice = item.ItemPrices.FirstOrDefault();
                if (defaultPrice != null)
                {
                    ItemPrice_Changed(txtblItemPrice, defaultPrice.Price);
                }
            }
        }        
        private void Item_MouseDown(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var element = sender as FrameworkElement;
            var data = element.DataContext;

            if (data == null)
                return;
            var thisitem = data as OrderItem;
            var detailWindow = new ItemWindow(thisitem);
            detailWindow.ShowDialog();
        }

        private void ItemsContainer_SizeChanged(object sender, SizeChangedEventArgs e) //Căn chỉnh items
        {
            if (sender is not ScrollViewer sv) return;
            if (sv.Content is not UniformGrid ug) return;

            double w = sv.ActualWidth;
            if (w <= 50) return;

            int minItemWidth = 150;
            int columns = Math.Max(1, (int)(w / minItemWidth));
            ug.Columns = columns;
        }

        private void bdrItemSize_MouseEnter(object sender, MouseEventArgs e) // Đổi màu background khi rê chuột vào
        {
            if (sender is Border border && !(border.Tag as bool? ?? false))
            {
                border.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#D4BA98"));
                var txtb = border.Child as TextBlock;
                txtb.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#340D05"));
            }
        }

        private void bdrItemSize_MouseLeave(object sender, MouseEventArgs e) //Trả lại màu ban đầu khi không trò con chuột vào
        {
            if (sender is Border border && !(border.Tag as bool? ?? false))
            {
                border.Background = Brushes.Transparent; // trả về nền mặc định
                var txtb = border.Child as TextBlock;
                txtb.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#766839"));
            }
        }



        private void bdrItemSize_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true; // Ngăn event bubble lên parent

            if (sender is Border bdrSize)
            {
                // Cập nhật giá
                decimal price = GetPriceFromBorder(bdrSize);
                DisplayPrice("txtblItemPrice", bdrSize, price);

                // Tìm StackPanel chứa các size
                var stkSizePanel = FindParent<StackPanel>(bdrSize);
                if (stkSizePanel == null) return;

                // Bỏ chọn tất cả size khác
                foreach (var b in stkSizePanel.Children.OfType<Border>())
                {
                    b.Tag = false;
                    b.Background = Brushes.Transparent;
                    if (b.Child is TextBlock t)
                        t.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#766839"));
                }

                // Chọn Border được click
                bdrSize.Tag = true;
                bdrSize.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#766839"));
                if (bdrSize.Child is TextBlock txt)
                    txt.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#EDE2D3"));
            }
        }


        // Tìm kiếm item với từ khóa
        private void SearchItems(string keyword) 
        {
            var result = _items.Where(i => i.ItemName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            LoadItem(new ObservableCollection<OrderItem>(result));
        }

        private void txblSearchItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchItems(txblSearchItem.Text.Trim());
        }

        #endregion

        #region Choose Customer

        private void icAddCustomer_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void icSearchCustomer_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
        #endregion
        
    }
}
