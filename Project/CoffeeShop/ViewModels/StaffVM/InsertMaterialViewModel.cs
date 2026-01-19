using CoffeeShop.Models;
using CoffeeShop.Service;
using CoffeeShop.Service.DTOs;
using CoffeeShop.View.Controls;
using DocumentFormat.OpenXml.Math;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class InsertMaterialViewModel : BaseViewModel
    {
        // Commands
        public ICommand SaveCommand { get; set; } = null!;
        public ICommand CloseWindowCommand { get; set; } = null!;

        #region Properties
        private string? _materialName;
        public string? MaterialName
        {
            get => _materialName;
            set
            {
                if (_materialName != value)
                {
                    _materialName = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal _quantity;
        public decimal Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> Units { get; set; } = new ObservableCollection<string>()
        {
            "Kg", "Lon", "Chai", "Hộp", "Hộp 1L", "Hũ"
        };

        private string _selectedUnit = string.Empty;
        public string SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (_selectedUnit != value)
                {
                    _selectedUnit = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal _threshold;
        public decimal Threshold
        {
            get => _threshold;
            set
            {
                if (_threshold != value)
                {
                    _threshold = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _note;
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

        private bool _isAddMode;
        public bool IsAddMode
        {
            get => _isAddMode;
            set { _isAddMode = value; OnPropertyChanged(); }
        }
        #endregion

        public string WindowTitle { get; private set; } 
        public string SaveButtonContent { get; private set; }

        private readonly ObservableCollection<DepotItemDTO> _depotItems;
        private DepotItemDTO? _itemToEdit; // Chỉ dùng cho chế độ sửa

        // Constructor Add
        public InsertMaterialViewModel(ObservableCollection<DepotItemDTO> itemsCollection)
        {
            _depotItems = itemsCollection;
            InitializeData();
            LoadCommands();

            // Setup cho chế độ Thêm mới
            this.WindowTitle = "Thêm vật tư mới";
            this.SaveButtonContent = "Thêm mới";
            IsAddMode = true;
        }

        // Constructor Update
        public InsertMaterialViewModel(DepotItemDTO selectedItem, ObservableCollection<DepotItemDTO> itemsCollection)
            : this(itemsCollection)
        {
            _itemToEdit = selectedItem;
            this.WindowTitle = $"Sửa vật tư: {selectedItem.MaterialName}";
            this.SaveButtonContent = "Cập nhật";

            this.MaterialName = selectedItem.MaterialName;
            this.Quantity = selectedItem.Quantity;
            this.SelectedUnit = selectedItem.Unit;
            this.Threshold = selectedItem.Threshold;
            this.Note = selectedItem.Note;
            IsAddMode = false;
        }

        private void InitializeData()
        {
            if (Units.Any())
            {
                SelectedUnit = Units.First();
            }
        }

        private bool CanExecuteSave(Window p)
        {
            // Check Condition
            if (string.IsNullOrWhiteSpace(MaterialName))
            {
                CustomMessageBox.Show("Tên vật tư không được để trống!", "Lỗi", CustomMessageBox.MessageButtons.OK, CustomMessageBox.MessageType.Error);
                return false;
            }
            else if (Quantity < 0)
            {
                CustomMessageBox.Show("Số lượng phải lớn hơn hoặc bằng 0!", "Lỗi", CustomMessageBox.MessageButtons.OK, CustomMessageBox.MessageType.Error);
                return false;
            }
            else if (Threshold < 0)
            {
                CustomMessageBox.Show("Ngưỡng cảnh báo phải lớn hơn hoặc bằng 0!", "Lỗi", CustomMessageBox.MessageButtons.OK, CustomMessageBox.MessageType.Error);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(SelectedUnit))
            {
                CustomMessageBox.Show("Chưa chọn đơn vị!", "Lỗi", CustomMessageBox.MessageButtons.OK, CustomMessageBox.MessageType.Error);
                return false;
            }
            return true;
        }

        private async void ExecuteSave(Window window)
        {
            if (!CanExecuteSave(window)) return;

            using (var db = new CoffeeShopContext())
            {
                if (_itemToEdit != null)
                {
                    // Update
                    Inventory? itemToUpdate = db.Inventories.Find(_itemToEdit.MaterialId);

                    if (itemToUpdate != null)
                    {
                        // Lấy actionType và staffId
                        int actionType; // 3 = Cập nhật
                        if (itemToUpdate.Quantity < Quantity) // Old < New
                        {
                            actionType = 3;
                        }
                        else actionType = 2;
                        int staffId = UserSession.Instance.StaffId; // ID nhân viên đang đăng nhập

                        // Bắt đầu Transaction
                        using (var transaction = db.Database.BeginTransaction()) // Transaction - All or nothing
                        {
                            try
                            {
                                // Ghi lại hành động vào lịch sử kho
                                InventoryHistory newHistory = new InventoryHistory
                                {
                                    MaterialId = itemToUpdate.MaterialId,
                                    ActionTypeId = actionType,
                                    Quantity = Math.Abs(itemToUpdate.Quantity - Quantity),
                                    Date = DateTime.Now,
                                    StaffId = staffId
                                };

                                // Cập nhật giá trị mới vào DB
                                itemToUpdate.MaterialName = MaterialName;
                                itemToUpdate.Quantity = Quantity;
                                itemToUpdate.Unit = SelectedUnit;
                                itemToUpdate.Threshold = Threshold;
                                itemToUpdate.Note = Note;

                                db.InventoryHistories.Add(newHistory);
                                await db.SaveChangesAsync(); // Lưu
                                transaction.Commit(); // Hoàn tất cả hai

                                // Cập nhật lại DG
                                _itemToEdit.MaterialName = MaterialName;
                                _itemToEdit.Quantity = Quantity;
                                _itemToEdit.Unit = SelectedUnit;
                                _itemToEdit.Threshold = Threshold;
                                _itemToEdit.Note = Note;
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback(); // Hủy bỏ cả hai
                                MessageBox.Show($"Lỗi: {ex.Message}");
                            }
                        }
                    }
                }
                else
                {
                    int actionType = 1; // 1 = Thêm mới
                    int staffId = UserSession.Instance.StaffId; // ID nhân viên đang đăng nhập
                    // Bắt đầu Transaction
                    using (var transaction = db.Database.BeginTransaction()) // Transaction - All or nothing
                    {
                        try
                        {
                            // Add
                            Inventory newItem = new Inventory
                            {
                                MaterialName = MaterialName,
                                Quantity = Quantity,
                                Unit = SelectedUnit,
                                Threshold = Threshold,
                                Note = Note
                            };

                            db.Inventories.Add(newItem);
                            db.SaveChanges(); // Thêm item vào csdl

                            // Ghi lại hành động vào lịch sử kho
                            InventoryHistory inventoryHistory = new InventoryHistory
                            {
                                MaterialId = newItem.MaterialId,
                                ActionTypeId = actionType,
                                Quantity = newItem.Quantity,
                                Date = DateTime.Now,
                                StaffId = staffId
                            };

                            db.InventoryHistories.Add(inventoryHistory);
                            await db.SaveChangesAsync(); // Lưu bản ghi lịch sử

                            transaction.Commit(); // Hoàn tất cả hai

                            // Ánh xạ ngược và thêm vào ObservableCollection
                            DepotItemDTO newDepotItem = new DepotItemDTO
                            {
                                MaterialId = newItem.MaterialId, // Lấy ID từ DB
                                MaterialName = newItem.MaterialName,
                                Quantity = newItem.Quantity,
                                Unit = newItem.Unit,
                                Threshold = newItem.Threshold,
                                Note = newItem.Note
                            };
                            // Cập nhật item vào dg
                            _depotItems.Add(newDepotItem);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback(); // Hủy bỏ cả hai
                            MessageBox.Show($"Lỗi: {ex.Message}");
                        }
                    }
                }
                // Đóng cửa sổ sau khi thao tác xong
                window.DialogResult = true;
                window?.Close();
            }
        }

        private void LoadCommands()
        {
            SaveCommand = new RelayCommand<Window>(ExecuteSave);
            CloseWindowCommand = new RelayCommand<Window>(p => { p?.Close(); });
        }
    }
}
