using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoffeeShop.View.Controls
{
    /// <summary>
    /// Interaction logic for RevenueTooltip.xaml
    /// </summary>
    public partial class RevenueTooltip : UserControl, IChartTooltip
    {
        private TooltipData _data = null!; // CS8618 fix: allow null at initialization

        public RevenueTooltip()
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
