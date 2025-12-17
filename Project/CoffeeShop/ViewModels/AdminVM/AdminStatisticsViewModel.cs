using CoffeeShop.Helper;
using CoffeeShop.Models;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using System.Windows.Media;

namespace CoffeeShop.ViewModels.AdminVM
{
    /// <summary>
    /// Lớp này chứa các phương thức và xử lý logic cho trang thống kê của admin
    /// </summary>
    public partial class AdminStatisticsViewModel : BaseViewModel
    {
        /// <summary>
        /// Constructor khởi tạo giá trị mặc định và lệnh cho AdminStatisticsViewModel
        /// </summary>
#pragma warning disable CS8618
        public AdminStatisticsViewModel()
#pragma warning restore CS8618
        {
            StartDate = DateTime.Today.AddDays(-7);
            EndDate = DateTime.Today;
            IsShowRevenue = true;
            ShowRevenueIcon = PackIconKind.CashOff;
            CurrencyFormatter = value => value.ToString("N0");
            PercentFormatter = value => value.ToString("P0");

            ResetToCurrentDayCommand = new RelayCommand<object>((p) =>
            {
                StartDate = DateTime.Today;
                EndDate = DateTime.Today;
            });

            ChangeChartValueCommand = new RelayCommand<object>((p) =>
            {
                IsShowRevenue = !IsShowRevenue;
                if (IsShowRevenue) ShowRevenueIcon = PackIconKind.Cash;
                else ShowRevenueIcon = PackIconKind.CashOff;
            });

            LoadColorPalette();
            _ = LoadPeakHourChart();
        }

        /// <summary>
        /// Load statistics page's data and charts
        /// </summary>
        /// <returns></returns>
        private async Task LoadPageAsync()
        {
            if (StartDate == null || EndDate == null)
                return;
            DateTime actualEnd = EndDate.Value.AddDays(1);

            await LoadCurrentDayStatistics();
            await LoadTotalStatistics(StartDate.Value, actualEnd);
            await LoadRevenueChart(StartDate.Value, actualEnd);
            await LoadTopProductChart(StartDate.Value, actualEnd);
            await LoadCategoryChart(StartDate.Value, actualEnd);
            await LoadDiscountChart(StartDate.Value, actualEnd);
            await LoadPaymentMethodChart(StartDate.Value, actualEnd);
            await LoadServiceOptionChart(StartDate.Value, actualEnd);
            await LoadCustomerChart(StartDate.Value, actualEnd);
        }

        #region Load Data For Charts

        private async Task LoadCurrentDayStatistics()
        {
            using (var db = new CoffeeShopContext())
            {
                var data = await db.Orders
                    .Where(o => o.OrderDate >= DateTime.Today && o.OrderDate < DateTime.Today.AddDays(1))
                    .ToListAsync();

                SubTotal = data.Sum(x => x.SubTotal).ToString("C0", viVn);
                DiscountTotal = data.Sum(x => x.DiscountMoney ?? 0).ToString("C0", viVn);
                FinalTotal = data.Sum(x => x.TotalAmount).ToString("C0", viVn);
                CashRevenue = data.Where(o => o.PaymentMethod.Contains("Tiền mặt")).Sum(x => x.TotalAmount).ToString("C0", viVn);
                BankingRevenue = data.Where(o => o.PaymentMethod.Contains("Chuyển khoản")).Sum(x => x.TotalAmount).ToString("C0", viVn);
                OrderTotal = data.Count().ToString();
                MorningRevenue = data.Where(o => o.OrderDate >= DateTime.Today.AddHours(6) && o.OrderDate < DateTime.Today.AddHours(10)).Sum(x => x.TotalAmount).ToString("C0", viVn);
                NoonRevenue = data.Where(o => o.OrderDate >= DateTime.Today.AddHours(10) && o.OrderDate < DateTime.Today.AddHours(14)).Sum(x => x.TotalAmount).ToString("C0", viVn);
                AfternoonRevenue = data.Where(o => o.OrderDate >= DateTime.Today.AddHours(14) && o.OrderDate < DateTime.Today.AddHours(18)).Sum(x => x.TotalAmount).ToString("C0", viVn);
                EveningRevenue = data.Where(o => o.OrderDate >= DateTime.Today.AddHours(18) && o.OrderDate < DateTime.Today.AddHours(22)).Sum(x => x.TotalAmount).ToString("C0", viVn);
            }
        }

        /// <summary>
        /// Load total revenue and total orders
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private async Task LoadTotalStatistics(DateTime from, DateTime to)
        {
            using (var db = new CoffeeShopContext())
            {
                var orders = await db.Orders
                    .Where(o => o.OrderDate >= from && o.OrderDate < to)
                    .ToListAsync();
                TotalRevenue = orders.Sum(o => o.TotalAmount).ToString("C0", viVn);
                TotalOrders = orders.Count().ToString("N0");
            }
        }

        /// <summary>
        /// Load data for the peak hour chart
        /// </summary>
        /// <returns></returns>
        private async Task LoadPeakHourChart()
        {
            var dayOfWeekLabels = new[] { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
            var hourLabels = new List<string>();
            for (int i = 8; i <= 21; i++) hourLabels.Add($"{i}:00");
            HourLabels = hourLabels.ToArray();
            DayOfWeekLabels = dayOfWeekLabels;

            using (var db = new CoffeeShopContext())
            {
                var rawDate = await db.Orders
                    .Where(o => o.OrderDate >= DateTime.Today.AddDays(-7) && o.OrderDate <= DateTime.Today)
                    .Select(o => new
                    {
                        Day = o.OrderDate.DayOfWeek,
                        Hourr = o.OrderDate.Hour,
                        Total = o.TotalAmount
                    })
                    .ToListAsync();

                var heatValues = new ChartValues<HeatPoint>();
                for(int d = 0; d < 7; d++)
                {
                    for(int h = 0; h <= 14; h++)
                    {
                        int realHour = h + 8;
                        DayOfWeek realDay = (d == 6) ? DayOfWeek.Sunday : (DayOfWeek)(d + 1);
                        var totalAmount = rawDate
                            .Where(x => x.Day == realDay && x.Hourr == realHour)
                            .Sum(x => x.Total);
                        heatValues.Add(new HeatPoint(h, d, (double)totalAmount));
                    }
                }

                SeriesCollection series = new SeriesCollection
                {
                    new HeatSeries
                    {
                        Title = "Doanh thu:",
                        Values = heatValues,
                        DataLabels = false,
                        LabelPoint = (point => (point.Weight > 0 ? point.Weight.ToString("C0", viVn) : "0 đ")),

                        GradientStopCollection = new GradientStopCollection
                        {
                            new GradientStop((Color)ColorConverter.ConvertFromString("#FFFFF0") ,0),
                            new GradientStop((Color)ColorConverter.ConvertFromString("#5CB338"), 1)
                        }
                    }
                };
                PeakHourSeries = series;
            }
        }

        /// <summary>
        /// Load data for the revenue chart
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private async Task LoadRevenueChart(DateTime from, DateTime to)
        {
            DayLabels = Array.Empty<string>();
            var dayLabels = new List<string>();    
            for (DateTime date = from; date < to; date = date.AddDays(1))
            {
                dayLabels.Add(date.ToString("dd/MM"));
            }

            using (var db = new CoffeeShopContext())
            {
                var rawData = await db.Orders
                    .Where(o => o.OrderDate >= from && o.OrderDate < to)
                    .Select(o => new
                    {
                        Datee = o.OrderDate.Date,
                        Amount = o.TotalAmount,
                        Method = o.PaymentMethod,
                    })
                    .ToListAsync();

                var cashValues = new ChartValues<decimal>();
                var bankingValues = new ChartValues<decimal>();
                var totalValues = new ChartValues<decimal>();
                var growthValues = new ChartValues<decimal>();
               

                for (DateTime date = from; date < to; date = date.AddDays(1))
                {
                    decimal cashTotal = IsShowRevenue ? 
                        rawData.Where(x => x.Datee == date && x.Method == "Tiền mặt").Sum(x => x.Amount) :
                        rawData.Where(x => x.Datee == date && x.Method == "Tiền mặt").Count();
                    decimal bankingTotal = IsShowRevenue ?
                        rawData.Where(x => x.Datee == date && x.Method == "Chuyển khoản").Sum(x => x.Amount) :
                        rawData.Where(x => x.Datee == date && x.Method == "Chuyển khoản").Count();
                    decimal total = cashTotal + bankingTotal;
                    cashValues.Add(cashTotal);
                    bankingValues.Add(bankingTotal);
                    totalValues.Add(total);
                }

                for(int i = 0; i < totalValues.Count; i++)
                {
                    if(i == 0)
                    {
                        growthValues.Add(0);
                    }
                    else
                    {
                        decimal previous = totalValues[i - 1];
                        decimal current = totalValues[i];
                        if (previous == 0)
                        {
                            growthValues.Add(current > 0 ? 1 : 0);
                        }
                        else
                        {
                            decimal growth = (current - previous) / previous;
                            growthValues.Add(growth);
                        }
                    }
                }

                SeriesCollection revenueSeries = new SeriesCollection
                {
                    // Tiền mặt
                    new StackedColumnSeries
                    {
                        ScalesYAt = 0,
                        Title = "Tiền mặt:",
                        Values = cashValues,
                        DataLabels = true,
                        LabelPoint = point => point.Y > 0 ? (IsShowRevenue ? point.Y.ToString("C0", viVn) : point.Y.ToString() + " đơn") : "",
                        Fill = new BrushConverter().ConvertFrom("#f1515e") as SolidColorBrush,
                        StackMode = StackMode.Values
                    },
                    // Chuyển khoản
                    new StackedColumnSeries
                    {
                        ScalesYAt = 0,
                        Title = "Chuyển khoản:",
                        Values = bankingValues,
                        DataLabels = true,
                        LabelPoint = point => point.Y > 0 ? (IsShowRevenue ? point.Y.ToString("C0", viVn) : point.Y.ToString() + " đơn") : "",
                        Fill = new BrushConverter().ConvertFrom("#1dbde6") as SolidColorBrush,
                        StackMode = StackMode.Values
                    },

                    new LineSeries
                    {
                        Title = "Tổng:",
                        Values = totalValues,
                        StrokeThickness = 0,
                        PointGeometry = null,
                        Fill = Brushes.Transparent,
                        DataLabels = true,
                        LabelPoint = point => point.Y > 0 ? (IsShowRevenue ? point.Y.ToString("C0", viVn) : point.Y.ToString() + " đơn") : "",
                        Foreground = Brushes.Black,
                        ScalesYAt = 0
                    },

                    new LineSeries
                    {
                        Title = "Tốc độ tăng trưởng:",
                        Values = growthValues,
                        DataLabels = false,
                        Fill = Brushes.Transparent,
                        Stroke = new BrushConverter().ConvertFrom(ColorPalette.Green2) as SolidColorBrush,
                        ScalesYAt = 1,
                        Opacity = 0.7,
                    }

                };

                LeftXAxisRevenueChartName = IsShowRevenue ? "Doanh thu (VND)" : "Số đơn hàng";
                DayLabels = dayLabels.ToArray();
                RevenueSeries = revenueSeries;
            }
        }

        /// <summary>
        /// // Load data for the top products chart
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private async Task LoadTopProductChart(DateTime from, DateTime to)
        {
            using (var db = new CoffeeShopContext())
            {
                var data = await db.OrderDetails
                    .Where(o => o.Order.OrderDate >= from && o.Order.OrderDate < to)
                    .GroupBy(o => o.Price.Item.ItemName)
                    .Select(g => new { Key = g.Key, Quantity = g.Sum(o => o.Quantity), Revenue = g.Sum(x => x.TotalPrice) })
                    .OrderByDescending(o => IsShowRevenue ? o.Revenue : o.Quantity)
                    .ToListAsync();

                var top5 = data.Take(5).ToList();
                var others = data.Skip(5).Sum(o => o.Quantity);

                int idx = 0;
                var series = new SeriesCollection();
                foreach (var item in top5)
                {
                    series.Add(new PieSeries
                    {
                        Title = IsShowRevenue ? $"{item.Key}: {item.Revenue.ToString("C0", viVn)}" : $"{item.Key}: {item.Quantity}",
                        Values = IsShowRevenue ? new ChartValues<decimal> { item.Revenue } : new ChartValues<int> { item.Quantity },
                        DataLabels = true,
                        LabelPoint = item => item.Participation.ToString("P1"),
                        Fill = idx < _coffeePalette.Count ? _coffeePalette[idx++] : new BrushConverter().ConvertFrom(value: ColorPalette.GreenGray) as SolidColorBrush
                    });
                }

                TopProductsSeries = series;
            }
        }

        /// <summary>
        /// Load data for the categories chart
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private async Task LoadCategoryChart(DateTime from, DateTime to)
        {
            using (var db = new CoffeeShopContext())
            {
                var data = await db.OrderDetails
                    .Where(od => od.Order.OrderDate >= from && od.Order.OrderDate < to) // Lọc ngày trước
                    .Include(od => od.Price).ThenInclude(p => p.Item).ThenInclude(i => i.Category) // Nối bảng
                    .GroupBy(od => od.Price.Item.Category.CategoryName) // Gom nhóm theo tên Danh mục
                    .Select(g => new
                    {
                        Key = g.Key,
                        Quantity = g.Sum(od => od.Quantity),
                        Revenue = g.Sum(od => od.TotalPrice)
                    })
                    .ToListAsync();
                var sortedData = data.OrderByDescending(x => IsShowRevenue ? x.Revenue : x.Quantity).ToList();

                var series = new SeriesCollection();
                int idx = 0;
                foreach (var item in sortedData)
                {
                    series.Add(new PieSeries
                    {
                        Title = IsShowRevenue ? $"{item.Key}: {item.Revenue.ToString("C0", viVn)}" : $"{item.Key}: {item.Quantity}",
                        Values = IsShowRevenue ? new ChartValues<decimal> { item.Revenue } : new ChartValues<int> { item.Quantity },
                        DataLabels = true,
                        LabelPoint = item => item.Participation.ToString("P1"),
                        Fill = idx < _matchaPalette.Count ? _matchaPalette[idx++] : new BrushConverter().ConvertFrom(value: ColorPalette.GreenGray) as SolidColorBrush
                    });
                }

                CategorySeries = series;
            }
        }

        /// <summary>
        /// Load data for the discounts chart
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private async Task LoadDiscountChart(DateTime from, DateTime to)
        {
            using (var db = new CoffeeShopContext())
            {
                var data = await db.Orders
                    .Include(o => o.Discount)
                    .Where(o => o.OrderDate >= from && o.OrderDate < to && o.DiscountId != null)
                    .GroupBy(o => o.Discount!.DiscountCode)
                    .Select(g => new
                    {
                        Key = g.Key,
                        Count = g.Count(),
                        Revenue = g.Sum(x => x.DiscountMoney ?? 0)
                    })
                    .ToListAsync();

                var series = new SeriesCollection();
                int idx = 0;
                foreach (var item in data)
                {
                    if (item == null) return;
                    series.Add(new PieSeries
                    {
                        Title = IsShowRevenue ? $"{item.Key} : {item.Revenue.ToString("C0", viVn)}" : $"{item.Key} : {item.Count}",
                        Values = IsShowRevenue ? new ChartValues<decimal> { item.Revenue } : new ChartValues<int> { item.Count },
                        DataLabels = true,
                        LabelPoint = point => point.Participation.ToString("P1"),
                        Fill = idx < _flanPalette.Count ? _flanPalette[idx++] : new BrushConverter().ConvertFrom(value: ColorPalette.GreenGray) as SolidColorBrush
                    });
                }
                
                DiscountSeries = series;
            }
        }

        /// <summary>
        /// Load data for the payment methods chart
        /// </summary>
        /// <param name="from"></param>
        /// <to></to>
        private async Task LoadPaymentMethodChart(DateTime from, DateTime to)
        {
            using (var db = new CoffeeShopContext())
            {
                var data = await db.Orders
                    .Where(o => o.OrderDate >= from && o.OrderDate < to)
                    .GroupBy(o => o.PaymentMethod)
                    .Select(g => new { Method = g.Key, Count = g.Count(), Revenue = g.Sum(x => x.TotalAmount) })
                    .ToListAsync();

                var series = new SeriesCollection();
                foreach (var item in data)
                {
                    series.Add(new PieSeries
                    {
                        Title = IsShowRevenue ? $"{item.Method}: {item.Revenue.ToString("C0", viVn)}" : $"{item.Method}: {item.Count}",
                        Values = IsShowRevenue ? new ChartValues<decimal> { item.Revenue } : new ChartValues<int> { item.Count },
                        DataLabels = true,
                        LabelPoint = p => p.Participation.ToString("P1"),
                    });
                }

                PaymentMethodSeries = series;
            }
        }

        /// <summary>
        /// Load data for the service options chart
        /// </summary>
        /// <param name="from"></param>
        /// <to></to>
        private async Task LoadServiceOptionChart(DateTime from, DateTime to)
        {
            using (var db = new CoffeeShopContext())
            {
                var stats = await db.Orders
                    .Where(o => o.OrderDate >= from && o.OrderDate < to)
                    .GroupBy(o => o.TableId != null)
                    .Select(g => new
                    {
                        IsDineIn = g.Key,
                        Count = g.Count(),
                        Revenue = g.Sum(x => x.TotalAmount)
                    })
                    .ToListAsync();

                var dineInGroup = stats.FirstOrDefault(x => x.IsDineIn == true);
                var takeAwayGroup = stats.FirstOrDefault(x => x.IsDineIn == false);
                var dineIn = dineInGroup?.Count ?? 0;
                var dineInRevenue = dineInGroup?.Revenue ?? 0;
                var takeAway = takeAwayGroup?.Count ?? 0;
                var takeAwayRevenue = takeAwayGroup?.Revenue ?? 0;

                var series = new SeriesCollection();
                series.Add(new PieSeries
                {
                    Title = IsShowRevenue ? $"Tại quán: {dineInRevenue.ToString("C0", viVn)}" : $"Tại quán: {dineIn}",
                    Values = IsShowRevenue ? new ChartValues<decimal> { dineInRevenue } : new ChartValues<int> { dineIn },
                    DataLabels = true,
                    LabelPoint = p => p.Participation.ToString("P1"),
                    Fill = new BrushConverter().ConvertFrom("#b0413e") as SolidColorBrush,
                });
                series.Add(new PieSeries
                {
                    Title = IsShowRevenue ? $"Mang đi: {takeAwayRevenue.ToString("C0", viVn)}" : $"Mang đi: {takeAway}",
                    Values = IsShowRevenue ? new ChartValues<decimal> { takeAwayRevenue } : new ChartValues<int> { takeAway },
                    DataLabels = true,
                    LabelPoint = p => p.Participation.ToString("P1"),
                    Fill = new BrushConverter().ConvertFrom("#fcaa67") as SolidColorBrush
                });

                ServiceOptionSeries = series;
            }
        }

        /// <summary>
        /// Load data for the customers chart
        /// </summary>
        /// <param name="from"></param>
        /// <to></to>
        /// <returns></returns>
        private async Task LoadCustomerChart(DateTime from, DateTime to)
        {
            using (var db = new CoffeeShopContext())
            {
                var stats = await db.Orders
                    .Where(o => o.OrderDate >= from && o.OrderDate < to)
                    .GroupBy(o => o.CustomerId != null)
                    .Select(g => new
                    {
                        IsMember = g.Key,
                        Count = g.Count(),
                        Revenue = g.Sum(x => x.TotalAmount)
                    })
                    .ToListAsync();

                var memberGroup = stats.FirstOrDefault(x => x.IsMember == true);
                var guestGroup = stats.FirstOrDefault(x => x.IsMember == false);
                var member = memberGroup?.Count ?? 0;
                var memberRevenue = memberGroup?.Revenue ?? 0;
                var guests = guestGroup?.Count ?? 0;
                var guestRevenue = guestGroup?.Revenue ?? 0;

                var series = new SeriesCollection();
                series.Add(new PieSeries
                {
                    Title = IsShowRevenue ? $"Vãng lai: {guestRevenue.ToString("C0", viVn)}" : $"Vãng lai: {guests}",
                    Values = IsShowRevenue ? new ChartValues<decimal> { guestRevenue } : new ChartValues<int> { guests },
                    DataLabels = true,
                    LabelPoint = p => p.Participation.ToString("P1"),
                    Fill = new BrushConverter().ConvertFrom("#00cc7c") as SolidColorBrush
                });
                series.Add(new PieSeries
                {
                    Title = IsShowRevenue ? $"Thành viên: {memberRevenue.ToString("C0", viVn)}" : $"Thành viên: {member}",
                    Values = IsShowRevenue ? new ChartValues<decimal> { memberRevenue } : new ChartValues<int> { member },
                    DataLabels = true,
                    LabelPoint = p => p.Participation.ToString("P1"),
                    Fill = new BrushConverter().ConvertFrom("#0f68a9") as SolidColorBrush,
                });

                CustomerSeries = series;
            }
        } 

        #endregion

    }
}
