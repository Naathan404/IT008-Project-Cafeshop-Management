using CoffeeShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CoffeeShop.ViewModels.StaffVM;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for ReportDepotWindow.xaml
    /// </summary>
    public partial class ReportDepotWindow : Window
    {
        public ReportDepotWindow(List<DepotItem> reportData, string filePath)
        {
            InitializeComponent();
            this.DataContext = new ReportDepotViewModel( filePath);
        }
    }
}
