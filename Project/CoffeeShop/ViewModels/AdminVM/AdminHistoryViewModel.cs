using CoffeeShop.Models;
using CoffeeShop.Service.DTOs;
using CoffeeShop.View.General;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace CoffeeShop.ViewModels.AdminVM
{
    public partial class AdminHistoryViewModel : BaseViewModel
    {
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
                MessageBox.Show("Chức năng in hóa đơn đang được phát triển.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            ExportExcelCommand = new RelayCommand<object>((p) =>
            {
                MessageBox.Show("Chức năng xuất excel đang được phát triển.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (SelectedPeriod == "Hôm nay")
            {
                _fromDate = DateTime.Today;
                _toDate = DateTime.Today;
                OnPropertyChanged(nameof(FromDate));
                OnPropertyChanged(nameof(ToDate));
            }
            else if (SelectedPeriod == "Tuần này")
            {
                int diff = (7 + (DateTime.Today.DayOfWeek - DayOfWeek.Monday)) % 7;
                _fromDate = DateTime.Today.AddDays(-1 * diff);
                _toDate = _fromDate.Value.AddDays(6);
                OnPropertyChanged(nameof(FromDate));
                OnPropertyChanged(nameof(ToDate));
            }
            else if (SelectedPeriod == "Tháng này")
            {
                _fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                _toDate = _fromDate.Value.AddMonths(1).AddDays(-1);
                OnPropertyChanged(nameof(FromDate));
                OnPropertyChanged(nameof(ToDate));
            }
            else if (SelectedPeriod == "Tùy chọn")
            {
                // Do nothing, let user choose dates
            }
            DateTime filterFrom = FromDate ?? DateTime.Today;
            DateTime filterTo = (ToDate ?? DateTime.Today).AddDays(1);
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
                            Discount = r.DiscountMoney ?? 0,
                            Total = r.TotalAmount,           
                            PaymentMethod = r.PaymentMethod,
                            TableName = r.TableName
                        }).ToList();
                    }
                }, token);

                if(token.IsCancellationRequested) return;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Orders.Clear();
                    foreach (var item in resultOrders)
                    {
                        Orders.Add(item);
                    }
                });
            }
            catch(TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi tải lịch sử: " + ex.Message);
            }
            finally
            {
                if(!token.IsCancellationRequested)
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
            public decimal Discount { get; set; }
            public string PaymentMethod { get; set; } = null!;
            public string TableName { get; set; } = null!;
        }
    }
}
