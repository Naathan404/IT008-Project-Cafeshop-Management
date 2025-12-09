using CoffeeShop.Models;
using CoffeeShop.View.General;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;


namespace CoffeeShop.View.Admin
{
    /// <summary>
    /// Interaction logic for HistoryManagementPage.xaml
    /// </summary>
    public partial class HistoryManagementPage : Page
    {
        private List<OrderHistory> orderHistoryItems = new List<OrderHistory>();
        CultureInfo viVn = new CultureInfo("vn-VN");

        public HistoryManagementPage()
        {
            InitializeComponent();
            LoadOrderHistory();
        }

        private void LoadOrderHistory()
        {
            using (var db = new CoffeeShopContext())
            {
                var orders = db.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Staff)
                    .ToList();
                foreach (var order in orders)
                {
                    orderHistoryItems.Add(new OrderHistory
                    {
                        OrderID = order.OrderId,
                        DisplayID = order.DisplayID,
                        CustomerName = order.Customer != null ? order.Customer.CustomerName : "Khách vãng lai",
                        EmployeeName = order.Staff.StaffName,
                        OrderDate = order.OrderDate.ToString("HH:mm:ss"),
                        Total = order.TotalAmount.ToString("N0", viVn),
                        PaymentMethod = order.PaymentMethod
                    });
                }
                dgOrdersHistory.ItemsSource = orderHistoryItems;
            }
        }

        public class OrderHistory
        {
            public int OrderID { get; set; }
            public string DisplayID { get; set; } = null!;
            public string CustomerName { get; set; } = null!;
            public string EmployeeName { get; set; } = null!;
            public string OrderDate { get; set; } = null!;
            public string Total { get; set; } = null!;
            public string PaymentMethod { get; set; } = null!;
        }

        private void TextChangedEvt(object sender, TextChangedEventArgs e)
        {
            FilterData();
        }

        private void SelectedTimeChangedEvt(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            FilterData();
        }

        private void FilterData()
        {
            string keyword = txbCustomerName.Text.Trim().ToLower();
            DateTime? start = timePickerStartTime.SelectedTime;
            DateTime? end = timePickerEndTime.SelectedTime;
            orderHistoryItems.Clear();


            using (var db = new CoffeeShopContext())
            {
                var query = db.Orders
                              .Include(o => o.Customer)
                              .Include(o => o.Staff)
                              .AsQueryable();

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(o => (o.Customer != null &&
                                             o.Customer.CustomerName.ToLower().Contains(keyword))
                                             || (o.Customer == null && "Khách vãng lai".ToLower().Contains(keyword)));
                }

                if (start.HasValue)
                {
                    DateTime startDateTime = DateTime.Today.Add(start.Value.TimeOfDay);
                    query = query.Where(o => o.OrderDate >= startDateTime);
                }
                if (end.HasValue)
                {
                    DateTime endDateTime = DateTime.Today.Add(end.Value.TimeOfDay);
                    query = query.Where(o => o.OrderDate <= endDateTime);
                }

                var orders = query.ToList();

                foreach (var order in orders)
                {
                    orderHistoryItems.Add(new OrderHistory
                    {
                        OrderID = order.OrderId,
                        DisplayID = order.DisplayID,
                        CustomerName = order.Customer != null ? order.Customer.CustomerName : "Khách vãng lai",
                        EmployeeName = order.Staff.StaffName,
                        OrderDate = order.OrderDate.ToString("HH:mm:ss"),
                        Total = order.TotalAmount.ToString("N0", viVn),
                        PaymentMethod = order.PaymentMethod
                    });
                }

                dgOrdersHistory.ItemsSource = orderHistoryItems;
                dgOrdersHistory.Items.Refresh();
            }
        }

        private void DetailClickEvt(object sender, RoutedEventArgs e)
        {
            OrderHistory? selectedItem = dgOrdersHistory.SelectedItem as OrderHistory;
            if (selectedItem == null)
                return;
            OrderDetailWindow orderDetailWindow = new OrderDetailWindow(selectedItem.OrderID);
            orderDetailWindow.ShowDialog();
        }
    }
}
