using CoffeeShop.Models;
using CoffeeShop.Service;
using CoffeeShop.Service.DTOs;
using CoffeeShop.View.Controls;
using CoffeeShop.View.General;
using CoffeeShop.ViewModels.AdminVM;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using static CoffeeShop.View.Controls.CustomMessageBox;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class StaffHistoryViewModel : BaseViewModel
    {
        CultureInfo viVn = new CultureInfo("vn-VN");
        CancellationTokenSource _cts;
        public ICommand RefreshPageCommand { get; set; }
        public ICommand ShowOrderDetailCommand { get; set; }
        public ICommand PrintCommand { get; set; }

        private DateTime? _fromTime;
        public DateTime? FromTime
        {
            get => _fromTime;
            set
            {
                if (_fromTime != value)
                {
                    _fromTime = value;
                    OnPropertyChanged();

                    if (SelectedPeriod != "Tùy chọn")
                    {
                        _selectedPeriod = "Tùy chọn";
                        OnPropertyChanged(nameof(SelectedPeriod));
                    }
                    _ = LoadOrderHistory();
                }
            }
        }

        private DateTime? _toTime;
        public DateTime? ToTime
        {
            get => _toTime;
            set
            {
                if (_toTime != value)
                {
                    _toTime = value;
                    OnPropertyChanged();

                    if (SelectedPeriod != "Tùy chọn")
                    {
                        _selectedPeriod = "Tùy chọn";
                        OnPropertyChanged(nameof(SelectedPeriod));
                    }

                    _ = LoadOrderHistory();
                }
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
        public ObservableCollection<string> Periods { get; } = new ObservableCollection<string>
        {
            "Hôm nay",
            "6h - 10h",
            "10h - 14h",
            "14h - 18h",
            "18h - 22h",
            "Tùy chọn"
        };

        private string _selectedPeriod = "Hôm nay";
        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                _selectedPeriod = value;
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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        private string _totalRevenue = "0 đ";
        public string TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                _totalRevenue = value;
                OnPropertyChanged();
            }
        }

        private string _totalOrders = "0";
        public string TotalOrders
        {
            get => _totalOrders;
            set
            {
                _totalOrders = value;
                OnPropertyChanged();
            }
        }

        private string _totalCash = "0 đ";
        public string TotalCash
        {
            get => _totalCash;
            set
            {
                _totalCash = value;
                OnPropertyChanged();
            }
        }

        private string _totalBankTransfer = "0 đ";
        public string TotalBankTransfer
        {
            get => _totalBankTransfer;
            set
            {
                _totalBankTransfer = value;
                OnPropertyChanged();
            }
        }

        private string _totalDiscount = "0 đ";
        public string TotalDiscount
        {
            get => _totalDiscount;
            set
            {
                _totalDiscount = value;
                OnPropertyChanged();
            }
        }


        public StaffHistoryViewModel()
        {
            FromTime = null;
            ToTime = null;
            SelectedPaymentMethod = PaymentMethods.First();

            EventAggregator.Instance.Subscribe<OrderCompletedMessage>(async (msg) =>
            {
                await LoadOrderHistory();
            });

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
                OrderDetailWindow orderDetailWindow = new OrderDetailWindow(SelectedOrder.OrderID);
                orderDetailWindow.ShowDialog();
            });

            PrintCommand = new RelayCommand<object>(async (p) =>
            {
                //CustomMessageBox.Show("Chức năng in hóa đơn đang được phát triển.", "Thông báo", MessageButtons.OK, MessageType.Info);
                // Khởi tạo hóa đơn
                string fileName = $"Bill_{SelectedOrder!.OrderID}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string fullPath = Path.Combine(folderPath, fileName);

                var exporter = new BillExporter(SelectedOrder!.OrderID);
                await exporter.ExportToExcel(fullPath);

                // Tự động mở file Excel
                Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
            });
            _ = LoadOrderHistory();
        }

        private async Task LoadOrderHistory()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                Orders.Clear();
            });
            if (SelectedPeriod == "6h - 10h")
            {
                _fromTime = DateTime.Today.AddHours(6);
                _toTime = DateTime.Today.AddHours(10);
                OnPropertyChanged(nameof(FromTime));
                OnPropertyChanged(nameof(ToTime));
            }
            else if (SelectedPeriod == "10h - 14h")
            {
                _fromTime = DateTime.Today.AddHours(10);
                _toTime = DateTime.Today.AddHours(14);
                OnPropertyChanged(nameof(FromTime));
                OnPropertyChanged(nameof(ToTime));
            }
            else if (SelectedPeriod == "14h - 18h")
            {
                _fromTime = DateTime.Today.AddHours(14);
                _toTime = DateTime.Today.AddHours(18);
                OnPropertyChanged(nameof(FromTime));
                OnPropertyChanged(nameof(ToTime));
            }
            else if (SelectedPeriod == "18h - 22h")
            {
                _fromTime = DateTime.Today.AddHours(18);
                _toTime = DateTime.Today.AddHours(22);
                OnPropertyChanged(nameof(FromTime));
                OnPropertyChanged(nameof(ToTime));
            }
            else if (SelectedPeriod == "Hôm nay")
            {
                _fromTime = DateTime.Today.AddHours(6);
                _toTime = DateTime.Today.AddHours(22);
                OnPropertyChanged(nameof(FromTime));
                OnPropertyChanged(nameof(ToTime));
            }
            DateTime filterFrom = FromTime ?? DateTime.Today;
            DateTime filterTo = ToTime ?? DateTime.Today.AddDays(1);
            string keyword = CustomerNameFilter.Trim().ToLower();
            bool hasKeyword = !string.IsNullOrEmpty(keyword);

            try
            {
                IsLoading = true;
                var resultOrders = await Task.Run(async () =>
                {
                    if (token.IsCancellationRequested) return new List<OrderHistory>();
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

                        if (hasKeyword)
                        {
                            orders = orders.Where(o => (o.Customer != null && o.Customer.CustomerName.ToLower().Contains(keyword))
                                                        || (o.Customer == null && "Khách vãng lai".ToLower().Contains(keyword)));
                        }

                        if (!string.IsNullOrEmpty(SelectedPaymentMethod) && SelectedPaymentMethod != "Tất cả")
                        {
                            orders = orders.Where(o => o.PaymentMethod == SelectedPaymentMethod);
                        }

                        var rawData = await orders
                                .OrderByDescending(o => o.OrderDate)
                                .Select(o => new
                                {
                                    o.OrderId,
                                    o.DisplayID,
                                    CustomerName = o.Customer != null ? o.Customer.CustomerName : "Khách vãng lai",
                                    StaffName = o.Staff != null ? o.Staff.StaffName : "N/A",
                                    TableName = o.Table != null ? o.Table.TableName : "Mang về",
                                    o.OrderDate,
                                    o.TotalAmount,
                                    o.PaymentMethod,
                                    o.DiscountMoney,
                                    o.SubTotal
                                })
                                .ToListAsync(token);

                        TotalOrders = rawData.Count().ToString();
                        TotalRevenue = rawData.Sum(r => r.TotalAmount).ToString("N0", viVn) + " đ";
                        TotalCash = rawData.Where(r => r.PaymentMethod == "Tiền mặt").Sum(r => r.TotalAmount).ToString("N0", viVn) + " đ";
                        TotalBankTransfer = rawData.Where(r => r.PaymentMethod == "Chuyển khoản").Sum(r => r.TotalAmount).ToString("N0", viVn) + " đ";
                        TotalDiscount = rawData.Sum(r => r.DiscountMoney)?.ToString("N0", viVn) + " đ";

                        return rawData.Select(r => new OrderHistory
                        {
                            OrderID = r.OrderId,
                            DisplayID = r.DisplayID,
                            CustomerName = r.CustomerName,
                            EmployeeName = r.StaffName,
                            OrderDate = r.OrderDate.ToString("dd/MM/yy HH:mm:ss"),
                            SubTotal = r.SubTotal,
                            Discount = r.DiscountMoney?.ToString("N0", viVn) ?? "0",
                            Total = r.TotalAmount,
                            PaymentMethod = r.PaymentMethod,
                            TableName = r.TableName
                        }).ToList();
                    }
                }, token);

                if (token.IsCancellationRequested) return;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Orders.Clear();
                    foreach (var item in resultOrders)
                    {
                        Orders.Add(item);
                    }
                });
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi tải lịch sử: " + ex.Message);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    IsLoading = false;
            }
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
                            UnitPrice = detail.UnitPrice,
                            TotalPrice = (detail.Quantity * detail.UnitPrice),
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
            public decimal Total { get; set; }
            public decimal SubTotal { get; set; }
            public string Discount { get; set; } = null!;
            public string PaymentMethod { get; set; } = null!;
            public string TableName { get; set; } = null!;
        }
    }
}