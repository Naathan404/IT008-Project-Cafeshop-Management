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
                new OrderItem { ItemId = 10, ItemName = "Cà phê dừa", CategoryId = 1, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 35000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },

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
                new OrderItem { ItemId = 21, ItemName = "Matcha đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 50000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 22, ItemName = "Socola đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 43000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 48000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 23, ItemName = "Cookies & Cream đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 47000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 52000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 24, ItemName = "Caramel đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 50000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 25, ItemName = "Vanilla đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 43000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 48000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 26, ItemName = "Dâu đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 50000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 27, ItemName = "Xoài đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 40000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 45000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 50000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 28, ItemName = "Coffee đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 38000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 43000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 48000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 29, ItemName = "Mocha đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 47000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 52000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
                new OrderItem { ItemId = 30, ItemName = "Oreo đá xay", CategoryId = 3, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 47000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 52000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },

                // Category 4: Trà (Tea)
                new OrderItem { ItemId = 31, ItemName = "Trà đào cam sả", CategoryId = 4, IsAvailable = true, ItemPrices = new List<ItemPrice> { new ItemPrice { Price = 32000, Size = new CoffeeShop.Models.Size { SizeId = 1, SizeName = "S" } }, new ItemPrice { Price = 37000, Size = new CoffeeShop.Models.Size { SizeId = 2, SizeName = "M" } }, new ItemPrice { Price = 42000, Size = new CoffeeShop.Models.Size { SizeId = 3, SizeName = "L" } } } },
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

        #region Datagrid Events
        #endregion

        #region ItemCard Events
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
        private void Item_Loaded(object sender, RoutedEventArgs e)
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

        private void Item_MouseDown(object sender, RoutedEventArgs e)
        {
            ItemWindow itemWindow = new ItemWindow();
            itemWindow.ShowDialog();
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

        private void bdrItemSize_MouseEnter(object sender, MouseEventArgs e) //Đổi màu background khi rê chuột vào
        {
            if (sender is Border border)
            {
                border.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#766839"));
                var txtb = border.Child as TextBlock;
                txtb.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#EDE2D3"));
            }
        }

        private void bdrItemSize_MouseLeave(object sender, MouseEventArgs e) //Trả lại màu ban đầu khi không trò con chuột vào
        {
            if (sender is Border border)
            {
                border.Background = Brushes.Transparent; // trả về nền mặc định
                var txtb = border.Child as TextBlock;
                txtb.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#766839"));
            }
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
