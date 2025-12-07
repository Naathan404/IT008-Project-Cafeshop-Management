using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Windows.Controls;

namespace CoffeeShop.View.Controls
{
    /// <summary>
    /// Interaction logic for PeakHourTooltip.xaml
    /// </summary>
    public partial class PeakHourTooltip : UserControl, IChartTooltip
    {
        private TooltipData _data = null!;
        public PeakHourTooltip()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public TooltipData Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged("Data");
            }
        }

        public TooltipSelectionMode? SelectionMode { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
