using System;
using System.Collections.Generic;
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

namespace CoffeeShop.View.Admin
{
    public partial class StatisticPage : Page
    {
        private DateTime? _startDateTime;
        private DateTime? _endDateTime;
        public StatisticPage()
        {
            InitializeComponent();
        }

        private void SelectedDateChangedEvt(object sender, SelectionChangedEventArgs e)
        {
            _startDateTime = dpStartDate.SelectedDate != null ? dpStartDate.SelectedDate.Value : null;
            _endDateTime = dpEndDate.SelectedDate != null ? dpEndDate.SelectedDate.Value.AddDays(1) : null;
            MessageBox.Show(_startDateTime.ToString() + " - " + _endDateTime.ToString());
        }
    }
}
