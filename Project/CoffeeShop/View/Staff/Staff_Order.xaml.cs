using CoffeeShop.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CoffeeShop.View.Staff
{
    public partial class Staff_Order : Page
    {
        private ICollectionView itemsView;
        ObservableCollection<OrderItem> _items = new ObservableCollection<OrderItem>();
        ObservableCollection<OrderTable> _tables = new ObservableCollection<OrderTable>();
        ObservableCollection<OrderCustomer> _customers = new ObservableCollection<OrderCustomer>();
        ObservableCollection<OrderDetailDisplay> _orders { get; set; } = new ObservableCollection<OrderDetailDisplay>();
        
        public Staff_Order()
        {
            InitializeComponent();
            LoadOrderItem();
            LoadCustomer();
            LoadTable();
            cbTable.ItemsSource = _tables;
            
            dtgListOrder.ItemsSource = _orders;
        }
        // Hàm tìm kiếm phần tử cha trong Visual Tree
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;

            return FindParent<T>(parentObject);
        }
    }
}
