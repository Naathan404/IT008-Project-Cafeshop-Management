using CoffeeShop.Models;
using System.Collections.ObjectModel;
using CoffeeShop.ViewModels.StaffVM;
using System.Windows;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InsertMaterial : Window
    {
        // Constructor 1: Chế độ thêm mới
        public InsertMaterial(ObservableCollection<DepotItem> itemsCollection)
        {
            InitializeComponent();
            // CHỈ CẦN TẠO VÀ GÁN VIEWMODEL
            this.DataContext = new InsertMaterialViewModel(itemsCollection);
        }

        // Constructor 2: CHẾ ĐỘ CẬP NHẬT
        public InsertMaterial(DepotItem selectedItem, ObservableCollection<DepotItem> itemsCollection)
        {
            InitializeComponent();
            // CHỈ CẦN TẠO VÀ GÁN VIEWMODEL
            this.DataContext = new InsertMaterialViewModel(selectedItem, itemsCollection);
        }
    }
}
