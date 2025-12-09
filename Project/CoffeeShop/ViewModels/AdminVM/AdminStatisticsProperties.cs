using CoffeeShop.Helper;
using LiveCharts;
using MaterialDesignThemes.Wpf;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Media;


namespace CoffeeShop.ViewModels.AdminVM
{
    /// <summary>
    /// Lớp này chứa các thuộc tính và biến cho AdminStatisticsViewModel
    /// </summary>
    public partial class AdminStatisticsViewModel : BaseViewModel
    {
        private CultureInfo viVn = new CultureInfo("vi-VN");
        private List<Brush> _matchaPalette = new List<Brush>();
        private List<Brush> _coffeePalette = new List<Brush>();
        private List<Brush> _flanPalette = new List<Brush>();
        public Func<double, string> CurrencyFormatter { get; set; }
        public Func<double, string> PercentFormatter { get; set; }

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
                {
                    _endDate = value;
                    OnPropertyChanged(nameof(EndDate));
                }
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
                {
                    _startDate = value;
                    OnPropertyChanged(nameof(StartDate));
                }    
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

        private string _leftXAxisRevenueChartName;
        public string LeftXAxisRevenueChartName
        {
            get => _leftXAxisRevenueChartName;
            set
            {
                _leftXAxisRevenueChartName = value;
                OnPropertyChanged();
            }
        }

        private string _totalRevenue;
        public string TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                _totalRevenue = value;
                OnPropertyChanged();
            }
        }

        private string _totalOrders;
        public string TotalOrders
        {
            get => _totalOrders;
            set
            {
                _totalOrders = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection _peakHourSeries;
        public SeriesCollection PeakHourSeries
        {
            get => _peakHourSeries;
            set
            {
                _peakHourSeries = value;
                OnPropertyChanged();
            }
        }
        private string[] _hourLabels;
        public string[] HourLabels
        {
            get => _hourLabels;
            set
            {
                _hourLabels = value;
                OnPropertyChanged();
            }
        }
        private string[] _dayOfWeekLabels;
        public string[] DayOfWeekLabels
        {
            get => _dayOfWeekLabels;
            set
            {
                _dayOfWeekLabels = value;
                OnPropertyChanged();
            }
        }

        private string[] _dayLabels;
        public string[] DayLabels
        {
            get => _dayLabels;
            set
            {
                _dayLabels = value;
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

        private string _subTotal;
        public string SubTotal
        {
            get => _subTotal;
            set
            {
                _subTotal = value;
                OnPropertyChanged();
            }
        }

        private string _discountTotal;
        public string DiscountTotal
        {
            get => _discountTotal;
            set
            {
                _discountTotal = value;
                OnPropertyChanged();
            }
        }

        private string _finalTotal; 
        public string FinalTotal
        {
            get => _finalTotal;
            set
            {
                _finalTotal = value;
                OnPropertyChanged();
            }
        }

        private string _cashRevenue;
        public string CashRevenue
        {
            get => _cashRevenue;
            set
            {
                _cashRevenue = value;
                OnPropertyChanged();
            }
        }

        private string _bankingRevenue;
        public string BankingRevenue
        {
            get => _bankingRevenue;
            set
            {
                _bankingRevenue = value;
                OnPropertyChanged();
            }
        }

        private string _moringRevenue;
        public string MorningRevenue
        {
            get => _moringRevenue;
            set
            {
                _moringRevenue = value;
                OnPropertyChanged();
            }
        }

        private string _noonRevenue;
        public string NoonRevenue
        {
            get => _noonRevenue;
            set
            {
                _noonRevenue = value;
                OnPropertyChanged();
            }
        }

        private string _afternoonRevenue;
        public string AfternoonRevenue
        {
            get => _afternoonRevenue;
            set
            {
                _afternoonRevenue = value;
                OnPropertyChanged();
            }
        }

        private string _eveningRevenue;
        public string EveningRevenue
        {
            get => _eveningRevenue;
            set
            {
                _eveningRevenue = value;
                OnPropertyChanged();
            }
        }

        private string _orderTotal;
        public string OrderTotal
        {
            get => _orderTotal;
            set
            {
                _orderTotal = value;
                OnPropertyChanged();
            }
        }

        #endregion
        /// <summary>
        /// Khởi tạo bảng màu sử dụng trong các biểu đồ
        /// </summary>
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
            if (flan1 != null) _flanPalette.Add(flan1);
            if (flan2 != null) _flanPalette.Add(flan2);
            if (flan3 != null) _flanPalette.Add(flan3);
            if (flan4 != null) _flanPalette.Add(flan4);
            if (flan5 != null) _flanPalette.Add(flan5);
            if (flan6 != null) _flanPalette.Add(flan6);
        }
    }
}
