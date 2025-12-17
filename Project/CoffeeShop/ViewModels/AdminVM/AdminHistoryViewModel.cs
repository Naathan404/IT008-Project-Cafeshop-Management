using CoffeeShop.Models;
using CoffeeShop.Service.DTOs;
using CoffeeShop.View.General;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.AdminVM
{
    public class AdminHistoryViewModel : BaseViewModel
    {
        CultureInfo viVn = new CultureInfo("vn-VN");
        public ICommand RefreshPageCommand { get; set; }
        public ICommand ShowOrderDetailCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand ExportCommand { get; set; }

        private DateTime? _fromDate;
        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged();
                _ = LoadOrderHistory();
            }
        }

        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged();
                _ = LoadOrderHistory();
            }
        }

        public ObservableCollection<string> PaymentMethods { get; } = new ObservableCollection<string>
        {
            "Tất cả",
            "Tiền mặt",
            "Chuyển khoản"
        };

        private string _selectedPaymentMethod = "";
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                _selectedPaymentMethod = value;
                OnPropertyChanged();
                _ = LoadOrderHistory();
            }
        }

        public Visibility OrderDetailVisibility
        {
            get => SelectedOrder != null ? Visibility.Visible : Visibility.Collapsed;
        }

        private ObservableCollection<OrderHistory> _orders = new ObservableCollection<OrderHistory>();
        public ObservableCollection<OrderHistory> Orders
        {
            get => _orders;
            set
            {
                _orders = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<OrderDetailDTO> _orderDetails = new ObservableCollection<OrderDetailDTO>();
        public ObservableCollection<OrderDetailDTO> OrderDetails
        {
            get => _orderDetails;
            set
            {
                _orderDetails = value;
                OnPropertyChanged();
            }
        }

        private OrderHistory? _selectedOrder = null;
        public OrderHistory? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(OrderDetailVisibility));
                _ = LoadOrderDetail();
            }
        }

        private string _customerNameFilter = "";
        public string CustomerNameFilter
        {
            get => _customerNameFilter;
            set
            {
                _customerNameFilter = value;
                OnPropertyChanged();
                _ = LoadOrderHistory();
            }
        }


        public AdminHistoryViewModel()
        {
            FromDate = null;
            ToDate = null;
            SelectedPaymentMethod = PaymentMethods.First();

            RefreshPageCommand = new RelayCommand<object>(
                async (p) =>
                {
                    await LoadOrderHistory();
                },
                (p) => true
            );

            ShowOrderDetailCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedOrder == null) return;
                OrderDetailWindow orderDetailWindow = new OrderDetailWindow(SelectedOrder!.OrderID);
                orderDetailWindow.ShowDialog();
            });

            PrintCommand = new RelayCommand<object>((p) =>
            {
                // In hóa đơn
            });

            ExportCommand = new RelayCommand<object>((p) =>
            {
                // In hóa đơn
            });

            _ = LoadOrderHistory();
        }

        private async Task LoadOrderHistory()
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                Orders.Clear();
            });

            DateTime filterFrom = FromDate.HasValue ? FromDate.Value : DateTime.Today;
            DateTime filterTo = ToDate.HasValue ? ToDate.Value.AddDays(1) : DateTime.Today.AddDays(1);
            var resultOrders = await Task.Run(async () =>
            {
                using (var db = new CoffeeShopContext())
                {
                    var orders = db.Orders
                        .AsNoTracking()
                        .IgnoreQueryFilters()
                        .Include(o => o.Customer)
                        .Include(o => o.Staff)
                        .Include(o => o.Table)
                        .Where(o => o.OrderDate >= filterFrom && o.OrderDate < filterTo)
                        .AsQueryable();

                    if (!string.IsNullOrEmpty(CustomerNameFilter))
                    {
                        string keyword = CustomerNameFilter.Trim().ToLower();
                        orders = orders.Where(o => (o.Customer != null &&
                                                    o.Customer.CustomerName.ToLower().Contains(keyword))
                                                    || (o.Customer == null && "Khách vãng lai".ToLower().Contains(keyword)));
                    }

                    if (!string.IsNullOrEmpty(SelectedPaymentMethod) && SelectedPaymentMethod != "Tất cả")
                    {
                        orders = orders.Where(o => o.PaymentMethod == SelectedPaymentMethod);
                    }

                    var orderList = await orders.OrderByDescending(o => o.OrderDate).ToListAsync();
                    var dtoList = new List<OrderHistory>();

                    foreach (var order in orderList)
                    {
                        dtoList.Add(new OrderHistory
                        {
                            OrderID = order.OrderId,
                            DisplayID = order.DisplayID,
                            CustomerName = order.Customer != null ? order.Customer.CustomerName : "Khách vãng lai",
                            EmployeeName = order.Staff.StaffName,
                            OrderDate = order.OrderDate.ToString("dd/MM/yy HH:mm:ss"),
                            Total = order.TotalAmount.ToString("N0", viVn),
                            PaymentMethod = order.PaymentMethod,
                            TableName = order.Table != null ? order.Table.TableName : "Không"
                        });
                    }

                    return dtoList;
                }
            });

            Orders = new ObservableCollection<OrderHistory>(resultOrders);
        }

        private async Task LoadOrderDetail()
        {
            var resultList = await Task.Run(() =>
            {
                using (var db = new CoffeeShopContext())
                {
                    var orderDetails = db.OrderDetails
                        .AsNoTracking()
                        .Where(od => od.OrderId == (SelectedOrder != null ? SelectedOrder.OrderID : null))
                        .Include(od => od.Price)
                        .ThenInclude(p => p.Item)
                        .Include(od => od.Price)
                        .ThenInclude(p => p.Size)
                        .ToList();
                    var dtoList = new List<OrderDetailDTO>();
                    foreach (var detail in orderDetails)
                    {
                        dtoList.Add(new OrderDetailDTO
                        {
                            ItemName = detail.Price.Item.ItemName,
                            SizeName = detail.Price.Size != null ? detail.Price.Size.SizeName : "---",
                            Quantity = detail.Quantity,
                            UnitPrice = detail.UnitPrice.ToString("N0", viVn),
                            TotalPrice = (detail.Quantity * detail.UnitPrice).ToString("N0", viVn),
                            Note = detail.Note
                        });
                    }
                    return dtoList;
                }
            });
            OrderDetails = new ObservableCollection<OrderDetailDTO>(resultList);
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
            public string TableName { get; set; } = null!;
        }
    }
}
