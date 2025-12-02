using CoffeeShop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static CoffeeShop.View.Staff.Staff_Depot;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window
    {
        
        private DepotItem _itemToEdit = null; // Nếu đối tượng đã được thêm thì sẽ khác null
        ObservableCollection<DepotItem> depotItems;
        // Constructor 1: CHẾ ĐỘ THÊM MỚI
        public InputWindow(ObservableCollection<DepotItem> itemsCollection)
        {
            InitializeComponent();
            cbInputUnit.SelectedIndex = 0; // Gan gia tri mac dinh cho cb
            LoadInputUnit();
            depotItems = itemsCollection;
            this.Title = "Thêm vật tư mới";
        }
        
        // Constructor 2: CHẾ ĐỘ CẬP NHẬT
        public InputWindow(DepotItem selectedItem, ObservableCollection<DepotItem> itemsCollection) : this(itemsCollection) // Gọi lại constructor Thêm mới (ko cần ghi lại các dòng giống nhau)
        {
            // Gán item đang chọn cho _itemToEdit
            _itemToEdit = selectedItem;
            this.Title = $"Sửa vật tư: {selectedItem.MaterialName}";

            // Đổ dữ liệu cũ vào các Control
            txbName.Text = selectedItem.MaterialName;
            txbQuantity.Text = selectedItem.Quantity.ToString();
            // Chọn đúng đơn vị trong ComboBox
            cbInputUnit.SelectedItem = selectedItem.Unit;
            txbNote.Text = selectedItem.Note;

            // Đổi chữ trên nút để người dùng biết là họ đang Sửa
            btnAddItem.Content = "Cập nhật";
        }

        // Load các đơn vị của item vào combobox để phục vụ cho việc thêm mới item
        private void LoadInputUnit()
        {
            cbInputUnit.Items.Add("Kg");
            cbInputUnit.Items.Add("Lon");
            cbInputUnit.Items.Add("Chai");
            cbInputUnit.Items.Add("Hộp");
            cbInputUnit.Items.Add("Hộp 1L");
            cbInputUnit.Items.Add("Hũ");
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            // Load dữ liệu mới vào DB và cập nhật lên dg
            LoadInputData();
            // Reset lại ô nhập dữ liệu
            txbName.Clear();
            txbQuantity.Clear();
            cbInputUnit.SelectedIndex = 0;
            txbNote.Clear();
        }

        // Thêm dữ liệu mới vào db sau đó gọi hàm để Load lại dữ liệu cho datagrid
        private void LoadInputData()
        {
            string name = txbName.Text;
            string quantityText = txbQuantity.Text.Trim();
            string unit = cbInputUnit.Text;
            // Kiểm tra dữ liệu
            if (!decimal.TryParse(quantityText, out decimal quantityValue))
            {
                // Nếu chuyển đổi thất bại (chuỗi rỗng, chữ, dấu sai)
                MessageBox.Show("Vui lòng nhập Số Lượng hợp lệ (chỉ nhập số).", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (cbInputUnit.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn Đơn vị.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Tạo item để thêm vào db (chứa dữ liệu từ input)
            Inventory newItem = new Inventory
            {
                MaterialName = txbName.Text,
                Quantity = quantityValue,
                Unit = cbInputUnit.SelectedValue.ToString(),
                Note = txbNote.Text
            };

            // Thêm dữ liệu vào DB
            using (var db = new CoffeeShopContext())
            {
                if (_itemToEdit != null)
                {
                    // CHẾ ĐỘ UPDATE
                    Inventory itemToUpdate = db.Inventories.Find(_itemToEdit.MaterialId); // Dựa vào ID của item từ dg -> tìm kiếm item trong DB

                    if (itemToUpdate != null)
                    {
                        // Cập nhật giá trị mới vào DB
                        itemToUpdate.MaterialName = name;
                        itemToUpdate.Quantity = quantityValue;
                        itemToUpdate.Unit = unit;
                        itemToUpdate.Note = txbNote.Text;

                        db.SaveChanges(); // Lưu thay đổi trong DB

                        // Cập nhật lại dữ liệu cho dg
                        _itemToEdit.MaterialName = name;
                        _itemToEdit.Quantity = quantityValue;
                        _itemToEdit.Unit = unit;
                        _itemToEdit.Note = txbNote.Text;
                        // Vì DepotItem đã có INotifyPropertyChanged nên sẽ tự động set khi dữ liệu trong dg có sự thay đổi
                    }
                }
                else
                {
                    // CHẾ ĐỘ THÊM MỚI (ADD)
                    // Khai báo item mới chứa giá trị từ input
                    Inventory newEntity = new Inventory
                    {
                        MaterialName = name,
                        Quantity = quantityValue,
                        Unit = unit,
                        Note = txbNote.Text
                    };

                    db.Inventories.Add(newEntity);
                    db.SaveChanges();

                    // Ánh xạ ngược từ kiểu Inventory (có ID từ DB) sang DepotItem (Vì ko convert từ Inventory sang DepotItem được)
                    DepotItem newDepotItem = new DepotItem
                    {
                        MaterialId = newEntity.MaterialId, // ID từ DB
                        MaterialName = newEntity.MaterialName,
                        Quantity = newEntity.Quantity,
                        Unit = newEntity.Unit,
                        Note = newEntity.Note
                    };

                    // Thêm DepotItem vào ObservableCollection
                    depotItems.Add(newDepotItem);
                }
            }
            this.Close();
        }

        private void btnQuitInput_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
