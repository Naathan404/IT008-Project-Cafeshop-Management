using CoffeeShop.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;

namespace CoffeeShop.View.Staff
{
    public class NotificationBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    /// <summary>
    /// Interaction logic for Staff_Depot.xaml
    /// </summary>
    public partial class Staff_Depot : Page
    {
        private ICollectionView itemsView;
        ObservableCollection<DepotItem> depotItems = new ObservableCollection<DepotItem>();
        public Staff_Depot()
        {
            InitializeComponent();
            itemsView = CollectionViewSource.GetDefaultView(depotItems);
            LoadDepotItem();
            cbUnitFilter.SelectedIndex = 0; // cb Filter mặc định chọn tất cả
            LoadFilterUnit();
        }

        // Class for adding datas
        public class DepotItem : NotificationBase
        {
            public int _materialId;
            public string? _materialName;
            public decimal _quantity;
            public string _unit;
            public string? _note;

            // --- CÁC PROPERTY (Thuộc tính có logic) ---
            public int MaterialId;
            // 1. MaterialName
            public string? MaterialName
            {
                get => _materialName;
                set
                {
                    if (_materialName != value) // Chỉ thông báo nếu giá trị thực sự thay đổi
                    {
                        _materialName = value;
                        OnPropertyChanged(); // Gọi hàm thông báo
                    }
                }
            }

            // 2. Quantity
            public decimal Quantity
            {
                get => _quantity;
                set
                {
                    if (_quantity != value)
                    {
                        _quantity = value;
                        OnPropertyChanged(); // Gọi hàm thông báo
                    }
                }
            }

            // 3. Unit
            public string Unit
            {
                get => _unit;
                set
                {
                    if (_unit != value)
                    {
                        _unit = value;
                        OnPropertyChanged();
                    }
                }
            }

            // 4. Note
            public string? Note
            {
                get => _note;
                set
                {
                    if (_note != value)
                    {
                        _note = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public void LoadDepotItem()
        {
            depotItems.Clear();
            using (var db = new CoffeeShopContext())
            {
                var items = db.Inventories.ToList();
                foreach (var item in items)
                {
                    depotItems.Add(new DepotItem
                    {
                        _materialId = item.MaterialId,
                        _materialName = item.MaterialName ?? string.Empty,
                        _quantity = item.Quantity,
                        _unit = item.Unit ?? string.Empty,
                        _note = item.Note ?? string.Empty
                    });
                }
                dgDepot.ItemsSource = depotItems;
            }
        }

        private void LoadFilterUnit()
        {
            using (var db = new CoffeeShopContext())
            {
                var unit = db.Inventories
                        .Select(item => item.Unit) // Chọn ra đơn vị trong item
                        .Distinct() // Loại bỏ trùng lặp
                        .ToList(); // Chuyển sang List để thêm vào comboBox
                unit.Insert(0, "All"); // Thêm All vào đầu danh sách của cb
                cbUnitFilter.ItemsSource = unit;
            }
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            popupFilter.IsOpen = !popupFilter.IsOpen;
            e.Handled = true;
        }

        private void btnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterData();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txbMin.Clear();
            txbMax.Clear();
            cbUnitFilter.SelectedIndex = 0;
            FilterData();
        }

        private void txbSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterData();
        }

        private void FilterData()
        {
            string searchTerm = txbSearchBar.Text;
            int minValue = 0;
            int maxValue = 0;
            string? unit = cbUnitFilter.SelectedItem as string;
            depotItems.Clear();


            using (var db = new CoffeeShopContext())
            {
                var query = db.Inventories.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(o => o != null && o.MaterialName.Contains(searchTerm));
                }
                // Lọc theo số lượng (quantity)
                if (int.TryParse(txbMin.Text, out minValue))
                {
                    query = query.Where(o => o != null && o.Quantity >= minValue);
                }
                if (int.TryParse(txbMax.Text, out maxValue))
                {
                    query = query.Where(o => o != null && o.Quantity <= maxValue);
                }
                // Lọc theo đơn vị
                if (unit.ToLower() != "all") // Nếu chọn tất cả thì ko cần lọc
                {
                    query = query.Where(o => o != null && o.Unit.ToLower() == unit.ToLower());
                }
                var items = query.ToList();

                // Thêm item lại từ đầu (Clear bảng rồi add lại những item sau khi lọc)
                depotItems.Clear();
                foreach (var item in items)
                {
                    depotItems.Add(new DepotItem
                    {
                        _materialId = item.MaterialId,
                        MaterialName = item.MaterialName,
                        Quantity = item.Quantity,
                        Unit = item.Unit,
                        Note = item.Note
                    });
                }

                // Cập nhật item lại vào dg và refresh
                dgDepot.ItemsSource = depotItems;
                dgDepot.Items.Refresh();
            }
        }

        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txbSearchBar.IsFocused)
            {
                btnUpdate.Focus();
            }
        }


        #region Các nút chức năng
        // Cập nhật dữ liệu của row đang chọn trong dg
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra hàng được chọn
            if (dgDepot.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một vật tư để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Lấy đối tượng đang được chọn
            DepotItem selectedItem = dgDepot.SelectedItem as DepotItem;

            // 3. Gọi constructor Sửa: truyền cả đối tượng đang được chọn và Collection
            InputWindow inputWindow = new InputWindow(selectedItem, depotItems);
            inputWindow.ShowDialog();
        }

        // Thêm item mới 
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            InputWindow inputWindow = new InputWindow(depotItems);
            inputWindow.ShowDialog();
        }

        // Xóa item đang chọn
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem có đang chọn item nào không
            if (dgDepot.SelectedItem == null) return;

            var selectedItem = dgDepot.SelectedItem as DepotItem;

            if (MessageBox.Show($"Bạn có chắc muốn xóa: {selectedItem.MaterialName}?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var db = new CoffeeShopContext())
                {
                    Inventory deletedItem = db.Inventories.Find(selectedItem._materialId);

                    if (deletedItem != null) // Tim thay item
                    {
                        // Xóa item
                        db.Inventories.Remove(deletedItem);

                        // Lưu thay đổi -> chính thức bị xóa
                        int recordsAffected = db.SaveChanges();

                        if (recordsAffected > 0)
                        {
                            // Load lại dữ liệu dg
                            depotItems.Remove(selectedItem);
                            MessageBox.Show("Đã xóa thành công khỏi DB!", "Thành công");
                        }
                    }
                }
            }
        }

        // Cáo cáo dữ liệu trong dg cho tài admin
        private void btnReport_Click(object sender, RoutedEventArgs e)
        {

        }

        // Xem lịch sử chỉnh sửa dg
        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
