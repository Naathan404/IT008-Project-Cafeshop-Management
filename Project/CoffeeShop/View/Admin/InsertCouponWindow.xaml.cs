using CoffeeShop.Service.DTOs;
using CoffeeShop.ViewModels.AdminVM;
using System.Windows;

namespace CoffeeShop.View.Admin
{
    /// <summary>
    /// Interaction logic for InsertCouponWindow.xaml
    /// </summary>
    public partial class InsertCouponWindow : Window
    {
        // Sửa lại dòng này để nhận tham số item
        public InsertCouponWindow(CouponDTO? item = null)
        {
            InitializeComponent();

            // Gán DataContext và truyền item vào ViewModel
            this.DataContext = new InsertCouponViewModel(item);
        }
    }
}
