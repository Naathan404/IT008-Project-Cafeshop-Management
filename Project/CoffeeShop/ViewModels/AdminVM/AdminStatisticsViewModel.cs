using CoffeeShop.Helper;
using CoffeeShop.Models;
using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Org.BouncyCastle.Pqc.Crypto.Frodo;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace CoffeeShop.ViewModels.AdminVM
{
    public class AdminStatisticsViewModel : BaseViewModel
    {
        CultureInfo viVn = new CultureInfo("vi-VN");
        List<Brush> _matchaPalette = new List<Brush>();
        List<Brush> _coffeePalette = new List<Brush>();
        List<Brush> _flanPalette = new List<Brush>();

        // Commands
        public ICommand ResetToCurrentDayCommand { get; set; }
        public ICommand ChangeChartValueCommand { get; set; }

        #region Variables & Properties
        // Variables & Properties
        private bool _isShowRevenue;
        public bool IsShowRevenue
        {
            get => _isShowRevenue;
            set
            {
                _isShowRevenue = value;
                OnPropertyChanged();
                _ = LoadPageAsync();
            }
        }

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                if (value > EndDate)
                    EndDate = value;
                _startDate = value;
                OnPropertyChanged();
                _ = LoadPageAsync();
            }
        }
        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                if (value < StartDate)
                    StartDate = value;
                _endDate = value;
                OnPropertyChanged();
                _ = LoadPageAsync();
            }
        }

        private PackIconKind _showRevenueIcon;
        public PackIconKind ShowRevenueIcon
        {
            get => _showRevenueIcon;
            set
            {
                _showRevenueIcon = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection _paymentMethodSeries = new SeriesCollection();
        public SeriesCollection PaymentMethodSeries
        {
            get => _paymentMethodSeries;
            set
            {
                _paymentMethodSeries = value;
                OnPropertyChanged();
            }
        }
        private SeriesCollection _serviceOptionSeries = new SeriesCollection();
        public SeriesCollection ServiceOptionSeries
        {
            get => _serviceOptionSeries;
            set
            {
                _serviceOptionSeries = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection _topProductSeries = new SeriesCollection();
        public SeriesCollection TopProductsSeries
        {
            get => _topProductSeries;
            set
            {
                _topProductSeries = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection _categorySeries = new SeriesCollection();
        public SeriesCollection CategorySeries
        {
            get => _categorySeries;
            set
            {
                _categorySeries = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection _discountSeries = new SeriesCollection();
        public SeriesCollection DiscountSeries
        {
            get => _discountSeries;
            set
            {
                _discountSeries = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection _customerSeries = new SeriesCollection();
        public SeriesCollection CustomerSeries
        {
            get => _customerSeries;
            set
            {
                _customerSeries = value;
                OnPropertyChanged();
            }
        }

        // Fix for CS8618: Initialize _revenueLabels to an empty array to ensure non-nullability.
        private string[] _revenueLabels = Array.Empty<string>();
        public string[] RevenueLabels
        {
            get => _revenueLabels;
            set
            {
                _revenueLabels = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection _revenueSeries = new SeriesCollection();
        public SeriesCollection RevenueSeries
        {
            get => _revenueSeries;
            set
            {
                _revenueSeries = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection _percentageFormatter = new SeriesCollection();
        public SeriesCollection PercentageFormatter
        {
            get => _percentageFormatter;
            set
            {
                _percentageFormatter = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public AdminStatisticsViewModel()
        {
            StartDate = DateTime.Today.AddDays(-7);
            EndDate = DateTime.Today;
            IsShowRevenue = false;
            ShowRevenueIcon = PackIconKind.CashOff;

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
        }

        private void LoadColorPalette()
        {
            // Load matcha color
            var green0 = new BrushConverter().ConvertFrom(value: ColorPalette.Green0) as SolidColorBrush;
            var green1 = new BrushConverter().ConvertFrom(value: ColorPalette.Green1) as SolidColorBrush;
            var green2 = new BrushConverter().ConvertFrom(value: ColorPalette.Green2) as SolidColorBrush;
            var green3 = new BrushConverter().ConvertFrom(value: ColorPalette.Green3) as SolidColorBrush;
            var green4 = new BrushConverter().ConvertFrom(value: ColorPalette.Green4) as SolidColorBrush;
            var green5 = new BrushConverter().ConvertFrom(value: ColorPalette.Green5) as SolidColorBrush;
            var green6 = new BrushConverter().ConvertFrom(value: ColorPalette.Green6) as SolidColorBrush;
            var green7 = new BrushConverter().ConvertFrom(value: ColorPalette.Green7) as SolidColorBrush;
            var green8 = new BrushConverter().ConvertFrom(value: ColorPalette.Green8) as SolidColorBrush;
            var green9 = new BrushConverter().ConvertFrom(value: ColorPalette.Green9) as SolidColorBrush;
            var green10 = new BrushConverter().ConvertFrom(value: ColorPalette.Green10) as SolidColorBrush;
            if (green0 != null) _matchaPalette.Add(green0);
            if (green1 != null) _matchaPalette.Add(green1);
            if (green2 != null) _matchaPalette.Add(green2);
            if (green3 != null) _matchaPalette.Add(green3);
            if (green4 != null) _matchaPalette.Add(green4);
            if (green5 != null) _matchaPalette.Add(green5);
            if (green6 != null) _matchaPalette.Add(green6);
            if (green7 != null) _matchaPalette.Add(green7);
            if (green8 != null) _matchaPalette.Add(green8);
            if (green9 != null) _matchaPalette.Add(green9);
            if (green10 != null) _matchaPalette.Add(green10);

            // Load coffee palette
            var espresso = new BrushConverter().ConvertFrom(value: ColorPalette.Espresso) as SolidColorBrush;
            var caramel = new BrushConverter().ConvertFrom(value: ColorPalette.Caramel) as SolidColorBrush;
            var latte = new BrushConverter().ConvertFrom(value: ColorPalette.Latte) as SolidColorBrush;
            var cream = new BrushConverter().ConvertFrom(value: ColorPalette.Cream) as SolidColorBrush;
            var milkCoffee = new BrushConverter().ConvertFrom(value: ColorPalette.MilkCoffee) as SolidColorBrush;
            if (espresso != null) _coffeePalette.Add(espresso);
            if (caramel != null) _coffeePalette.Add(caramel);
            if (latte != null) _coffeePalette.Add(latte);
            if (cream != null) _coffeePalette.Add(cream);
            if (milkCoffee != null) _coffeePalette.Add(milkCoffee);

            // Load flan palette
            var flan6 = new BrushConverter().ConvertFrom("#84994F") as SolidColorBrush;
            var flan5 = new BrushConverter().ConvertFrom("#FFE797") as SolidColorBrush;
            var flan4 = new BrushConverter().ConvertFrom("#FCB53B") as SolidColorBrush;
            var flan3 = new BrushConverter().ConvertFrom("#F25912") as SolidColorBrush;
            var flan2 = new BrushConverter().ConvertFrom("#A72703") as SolidColorBrush;
            var flan1 = new BrushConverter().ConvertFrom("#3B060A") as SolidColorBrush;
            if(flan1 != null) _flanPalette.Add(flan1);
            if(flan2 != null) _flanPalette.Add(flan2);
            if(flan3 != null) _flanPalette.Add(flan3);
            if(flan4 != null) _flanPalette.Add(flan4);
            if(flan5 != null) _flanPalette.Add(flan5);
            if(flan6 != null) _flanPalette.Add(flan6);
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

            await LoadRevenueChart(StartDate.Value, actualEnd);
            await LoadTopProductChart(StartDate.Value, actualEnd);
            await LoadCategoryChart(StartDate.Value, actualEnd);
            await LoadDiscountChart(StartDate.Value, actualEnd);
            await LoadPaymentMethodChart(StartDate.Value, actualEnd);
            await LoadServiceOptionChart(StartDate.Value, actualEnd);
            await LoadCustomerChart(StartDate.Value, actualEnd);
        }

        #region Load Data For Charts
        private async Task LoadRevenueChart(DateTime from, DateTime to)
        {
            RevenueLabels = Array.Empty<string>();
            var labels = new List<string>();
            for (DateTime date = from; date < to; date = date.AddDays(1))
            {
                labels.Add(date.ToString("dd/MM"));
            }

            using (var db = new CoffeeShopContext())
            {
                var rawData = await db.Orders
                    .Where(o => o.OrderDate >= from && o.OrderDate < to)
                    .Select(o => new
                    {
                        Datee = o.OrderDate.Date,
                        Amount = o.TotalAmount,
                        Method = o.PaymentMethod
                    })
                    .ToListAsync();

                var cashValues = new ChartValues<decimal>();
                var bankingValues = new ChartValues<decimal>();
                var totalValues = new ChartValues<decimal>();

                for (DateTime date = from; date < to; date = date.AddDays(1))
                {
                    decimal cashTotal = rawData
                        .Where(x => x.Datee == date && x.Method == "Tiền mặt")
                        .Sum(x => x.Amount);
                    decimal bankingTotal = rawData
                        .Where(x => x.Datee == date && x.Method == "Chuyển khoản")
                        .Sum(x => x.Amount);
                    decimal total = cashTotal + bankingTotal;

                    cashValues.Add(cashTotal);
                    bankingValues.Add(bankingTotal);
                    totalValues.Add(total);
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
                        LabelPoint = point => point.Y > 0 ? point.Y.ToString("N0", viVn) : "",
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
                        LabelPoint = point => point.Y > 0 ? point.Y.ToString("N0", viVn) + " " + point.X : "",
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
                        LabelPoint = point => point.Y > 0 ? point.Y.ToString("N0", viVn) : "",
                        Foreground = Brushes.Black,
                        ScalesYAt = 0
                    },
                    
                };
                
                RevenueLabels = labels.ToArray();
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
