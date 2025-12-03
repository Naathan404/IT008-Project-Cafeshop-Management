using CoffeeShop.Models;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;
using CoffeeShop.Helper;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.AdminVM
{
    public class AdminStatisticsViewModel : BaseViewModel
    {
        public ICommand ResetToCurrentDayCommand { get; set; }

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                if(value > EndDate)
                    EndDate = value;
                _startDate = value;
                OnPropertyChanged();
                LoadPage();
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
                LoadPage();
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

        public AdminStatisticsViewModel()
        {
            StartDate = DateTime.Today.AddDays(-7);
            EndDate = DateTime.Today;

            ResetToCurrentDayCommand = new RelayCommand<object>((p) =>
            {
                StartDate = DateTime.Today;
                EndDate = DateTime.Today;
            });
        }

        #region Load Data To Charts
        private void LoadPage()
        {
            if (StartDate == null || EndDate == null)
                return;
            DateTime actualEnd = EndDate.Value.AddDays(1);

            LoadPieChartPaymentMethod(StartDate.Value, actualEnd);
            LoadPieChartServiceOption(StartDate.Value, actualEnd);
            LoadPieChartTop5BestSeller(StartDate.Value, actualEnd);
        }

        private void LoadPieChartPaymentMethod(DateTime from, DateTime to)
        {
            using (var db = new CoffeeShopContext())
            {
                var data = db.Orders
                    .Where(o => o.OrderDate >= from && o.OrderDate < to)
                    .GroupBy(o => o.PaymentMethod)
                    .Select(g => new {Method = g.Key, Count =  g.Count() })
                    .ToList();

                var series = new SeriesCollection();
                foreach (var item in data)
                {
                    series.Add(new PieSeries
                    {
                        //Title = item.Method,
                        Title = $"{item.Method}: {item.Count}",
                        Values = new ChartValues<int> { item.Count },
                        DataLabels = true,
                        LabelPoint = p => p.Participation.ToString("P1"),
                    });
                }    

                PaymentMethodSeries = series;
            }
        }

        private void LoadPieChartServiceOption(DateTime from, DateTime to)
        {
            using (var db = new CoffeeShopContext())
            {
                var vip = db.Orders
                    .Where(o => o.OrderDate >= from && o.OrderDate < to && o.TableId != null)
                    .Count();
                var guests = db.Orders
                    .Where(o => o.OrderDate >= from && o.OrderDate < to && o.TableId == null)
                    .Count();
                var series = new SeriesCollection();
                series.Add(new PieSeries
                {
                    Title = $"Thành viên: {vip}",
                    Values = new ChartValues<int> { vip },
                    DataLabels = true,
                    LabelPoint = p => p.Participation.ToString("P1"),
                    Fill = new BrushConverter().ConvertFrom(value: ColorPalette.MilkCoffee) as SolidColorBrush,
                });
                series.Add(new PieSeries
                {
                    Title = $"Vãng lai: {guests}",
                    Values = new ChartValues<int> { guests },
                    DataLabels = true,
                    LabelPoint = p => p.Participation.ToString("P1"),
                    Fill = new BrushConverter().ConvertFrom(value: ColorPalette.Matcha) as SolidColorBrush
                });

                ServiceOptionSeries = series;
            }    
        }

        private void LoadPieChartTop5BestSeller(DateTime from, DateTime to)
        {
            var colorList = new List<Brush>();
            var espresso = new BrushConverter().ConvertFrom(value: ColorPalette.Espresso) as SolidColorBrush;
            var caramel = new BrushConverter().ConvertFrom(value: ColorPalette.Caramel) as SolidColorBrush;
            var latte = new BrushConverter().ConvertFrom(value: ColorPalette.Latte) as SolidColorBrush;
            var cream = new BrushConverter().ConvertFrom(value: ColorPalette.Cream) as SolidColorBrush;
            var milkCoffee = new BrushConverter().ConvertFrom(value: ColorPalette.MilkCoffee) as SolidColorBrush;
            if (espresso != null) colorList.Add(espresso);
            if (caramel != null) colorList.Add(caramel);
            if (latte != null) colorList.Add(latte);
            if (cream != null) colorList.Add(cream);
            if (milkCoffee != null) colorList.Add(milkCoffee);

            using (var db = new CoffeeShopContext())
            {
                var data = db.OrderDetails
                    .Where(o => o.Order.OrderDate >= from && o.Order.OrderDate < to)
                    .GroupBy(o => o.Price.Item.ItemName)
                    .Select(g => new { Key = g.Key, Quantity = g.Sum(o => o.Quantity) })
                    .OrderByDescending(o => o.Quantity)
                    .ToList();

                var top5 = data.Take(5).ToList();
                var others = data.Skip(5).Sum(o => o.Quantity);

                int idx = 0;
                var series = new SeriesCollection();
                foreach (var item in top5)
                {
                    series.Add(new PieSeries
                    {
                        Title = $"{item.Key}: {item.Quantity}",
                        Values = new ChartValues<int> { item.Quantity },
                        DataLabels = true,
                        LabelPoint = item => item.Participation.ToString("P1"),
                        Fill = idx < colorList.Count ? colorList[idx++] : new BrushConverter().ConvertFrom(value: ColorPalette.GreenGray) as SolidColorBrush
                    });
                }

                TopProductsSeries = series;
            }
        }
        #endregion

    }
}
