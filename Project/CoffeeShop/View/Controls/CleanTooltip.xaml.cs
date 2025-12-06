using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Windows.Controls;

namespace CoffeeShop.View.Controls
{
    /// <summary>
    /// Interaction logic for CleanTooltip.xaml
    /// </summary>
    public partial class CleanTooltip : UserControl, IChartTooltip
    {
        private TooltipData _data = null!; // CS8618 fix: allow null at initialization

        public CleanTooltip()
        {
            InitializeComponent();
            // Quan trọng: DataContext là chính nó để XAML nhận được dữ liệu
            DataContext = this;
        }

        public event PropertyChangedEventHandler? PropertyChanged; // CS8618 fix: nullable event

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
