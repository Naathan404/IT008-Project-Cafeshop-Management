using CoffeeShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using static CoffeeShop.ViewModels.StaffVM.StaffDepotViewModel;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class DepotHistoryViewModel : BaseViewModel
    {
        public ICommand ApplyFilterCommand { get; private set; } = null!;
        public ICommand ClearFilterCommand { get; private set; } = null!;
        public ICommand TogglePopupCommand { get; private set; } = null!;
        public ICommand ShowDetailDepotHistoryCommand { get; private set; } = null!;

        public class DepotHistoryItem : BaseViewModel
        {
            private string? _staffName;
            public string? StaffName
            {
                get => _staffName;
                set
                {
                    if (_staffName != value)
                    {
                        _staffName = value;
                        OnPropertyChanged();
                    }
                }
            }

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

            private decimal? _quantity;
            public decimal? Quantity
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

            //private decimal? _price;
            //public decimal? Price
            //{
            //    get => _price;
            //    set
            //    {
            //        if (_price != value)
            //        {
            //            _price = value;
            //            OnPropertyChanged();
            //        }
            //    }
            //}

            private DateTime? _date;
            public DateTime? Date
            {
                get => _date;
                set
                {
                    if (_date != value)
                    {
                        _date = value;
                        OnPropertyChanged();
                    }
                }
            }

            private string? _actionName;
            public string? ActionName
            {
                get => _actionName;
                set
                {
                    if (_actionName != value)
                    {
                        _actionName = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        private string _popupVisibleState = "Collapsed";
        public string PopupVisibleState
        {
            get => _popupVisibleState;
            set
            {
                if (_popupVisibleState != value)
                {
                    _popupVisibleState = value;
                    OnPropertyChanged();
                }
            }
        }

        // Data Collection
        public ObservableCollection<DepotHistoryItem> depotHistoryItems { get; set; } = new ObservableCollection<DepotHistoryItem>();
        public ObservableCollection<string> actionTypes { get; set; } = new ObservableCollection<string>()
        {
            "Tất cả", "Nhập", "Dùng", "Cập nhật", "Hủy"
        };
        public ObservableCollection<string> staffNames { get; set; } = new ObservableCollection<string>()
        {
            "Tất cả", "Nguyễn Chí Nguyên", "Nguyễn Ngọc Lan Anh", "Lê Thành Nghĩa", "ADMIN"
        };

        private DepotItem? _selectedItem;
        public DepotItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested(); // Cập nhật trạng thái CanExecute của các nút (Delete/Update)
                }
            }
        }

        // Thuộc tính binding với TextBox Search
        private string _searchTerm = string.Empty;
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (_searchTerm != value)
                {
                    _searchTerm = value;
                    OnPropertyChanged();
                    ApplyFilterCommand.Execute(null);
                }
            }
        }

        // Thuộc tính cho ô lọc số lượng Min/Max
        private decimal? _minQuantity;
        public decimal? MinQuantity
        {
            get => _minQuantity;
            set
            {
                if (_minQuantity != value)
                {
                    _minQuantity = value;
                    OnPropertyChanged();
                }
            }
        }
        private decimal? _maxQuantity;
        public decimal? MaxQuantity
        {
            get => _maxQuantity;
            set
            {
                if (_maxQuantity != value)
                {
                    _maxQuantity = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal? _minPrice;
        public decimal? MinPrice
        {
            get => _minPrice;
            set
            {
                if (_minPrice != value)
                {
                    _minPrice = value;
                    OnPropertyChanged();
                }
            }
        }
        private decimal? _maxPrice;
        public decimal? MaxPrice
        {
            get => _maxPrice;
            set
            {
                if (_maxPrice != value)
                {
                    _maxPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _selectedAction = "Tất cả";
        public string? SelectedAction
        {
            get => _selectedAction;
            set
            {
                if (_selectedAction != value)
                {
                    _selectedAction = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _selectedStaff = "Tất cả";
        public string? SelectedStaff
        {
            get => _selectedStaff;
            set
            {
                if (_selectedStaff != value)
                {
                    _selectedStaff = value;
                    OnPropertyChanged();
                }
            }

        }

        private DateTime? _startDateFilter;
        public DateTime? StartDateFilter
        {
            get => _startDateFilter;
            set
            {
                if (_startDateFilter != value)
                {
                    _startDateFilter = value;
                    OnPropertyChanged();
                    ApplyFilterCommand.Execute(null);
                }
            }
        }

        private DateTime? _endDateFilter;
        public DateTime? EndDateFilter
        {
            get => _endDateFilter;
            set
            {
                if (_endDateFilter != value)
                {
                    _endDateFilter = value;
                    OnPropertyChanged();
                    ApplyFilterCommand.Execute(null);
                }
            }
        }

        public DepotHistoryViewModel()
        {
            LoadCommands();
            LoadHistoryItem();
        }

        // Hàm lọc dữ liệu
        private void ExecuteApplyFilter(object? parameter)
        {
            depotHistoryItems.Clear();
            try
            {
                using (var db = new CoffeeShopContext())
                {
                    var query = db.InventoryHistories
                                    .Include(h => h.ActionType)
                                    .Include(h => h.Staff)
                                    .Include(h => h.Material)
                                    .AsQueryable();

                    // 1. LỌC THEO TÊN (Search Term)
                    if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        string searchLower = SearchTerm.ToLower();
                        query = query.Where(h => h != null && h.Material.MaterialName.ToLower().Contains(searchLower));
                    }

                    // 2. LỌC THEO SỐ LƯỢNG (Min/Max)
                    // Lọc theo Min (Chỉ lọc nếu MinValue > 0)
                    if (MinQuantity.HasValue && MinQuantity > 0)
                    {
                        // Kiểm tra Quantity trong DB >= MinValue
                        query = query.Where(h => h != null && h.Quantity >= MinQuantity);
                    }

                    // Lọc theo Max (Chỉ lọc nếu MaxValue > 0, hoặc MaxValue > MinValue)
                    if (MaxQuantity.HasValue && MaxQuantity > 0)
                    {
                        // Kiểm tra Quantity trong DB <= MaxValue
                        query = query.Where(h => h != null && h.Quantity <= MaxQuantity);
                    }

                    // 3. LỌC THEO GIÁ (Min/Max)

                    // Lọc theo Min (Chỉ lọc nếu MinValue > 0)
                    if (MinPrice.HasValue && MinPrice > 0)
                    {
                        // Kiểm tra Price trong DB >= MinValue
                        query = query.Where(h => h != null && h.InputPrice >= MinPrice);
                    }

                    // Lọc theo Max (Chỉ lọc nếu MaxValue > 0, hoặc MaxValue > MinValue)
                    if (MaxPrice.HasValue && MaxPrice > 0)
                    {
                        // Kiểm tra Price trong DB <= MaxValue
                        query = query.Where(h => h != null && h.InputPrice <= MaxPrice);
                    }

                    // 4. LỌC THEO HÀNH ĐỘNG
                    if (SelectedAction.ToLower() != "tất cả")
                    {
                        // Kiểm tra Unit trong DB khớp với SelectedUnit
                        query = query.Where(h => h != null && h.ActionType != null && h.ActionType.ActionName.ToLower() == SelectedAction.ToLower());
                    }

                    // 5. LỌC THEO NHÂN VIÊN
                    if (SelectedStaff.ToLower() != "tất cả")
                    {
                        // Kiểm tra Unit trong DB khớp với SelectedUnit
                        query = query.Where(h => h != null && h.Staff != null && h.Staff.StaffName.ToLower() == SelectedStaff.ToLower());
                    }

                    // 6. Lọc theo thời gian
                    if (StartDateFilter.HasValue)
                    {
                        //DateTime startDateTime = DateTime.Today.Add(StartDateFilter.Value.TimeOfDay);
                        query = query.Where(h => h.Date >= StartDateFilter.Value);
                    }
                    if (EndDateFilter.HasValue)
                    {
                        //DateTime endDateTime = DateTime.Today.Add(EndDateFilter.Value.TimeOfDay);
                        query = query.Where(h => h.Date < EndDateFilter.Value.AddDays(1));
                    }

                    // 4. THỰC THI TRUY VẤN VÀ CẬP NHẬT COLLECTION
                    var items = query.ToList();

                    foreach (var item in items)
                    {
                        // Ánh xạ an toàn
                        depotHistoryItems.Add(new DepotHistoryItem
                        {
                            MaterialName = item.Material.MaterialName ?? string.Empty,
                            Quantity = item.Quantity,
                            ActionName = item.ActionType.ActionName,
                            //Price = item.InputPrice,
                            Date = item.Date,
                            StaffName = item.Staff.StaffName ?? string.Empty
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Hiển thị lỗi nếu có vấn đề DB
                MessageBox.Show($"Lỗi lọc dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm bỏ lọc
        private void ExecuteClearFilter(object? parameter)
        {
            // Reset các thuộc tính input về giá trị mặc định
            SearchTerm = string.Empty;
            MinQuantity = null;
            MaxQuantity = null;
            MinPrice = null;
            MaxPrice = null;
            SelectedAction = "Tất cả";
            SelectedStaff = "Tất cả";
            StartDateFilter = null;
            EndDateFilter = null;

            // Chạy lại bộ lọc để hiển thị toàn bộ dữ liệu
            ExecuteApplyFilter(null);
        }

        private void ExecuteTogglePopup(object? parameter)
        {
            if (PopupVisibleState == "Collapsed")
            {
                PopupVisibleState = "Visible";
            }
            else
            {
                PopupVisibleState = "Collapsed";
            }
        }

        private void LoadHistoryItem()
        {
            depotHistoryItems.Clear();
            using (var db = new CoffeeShopContext())
            {
                var historyItems = db.InventoryHistories
                                    .Include(h => h.Staff)
                                    .Include(h => h.Material)
                                    .Include(h => h.ActionType)
                                    .ToList();
                foreach (var historyItem in historyItems)
                {
                    depotHistoryItems.Add(new DepotHistoryItem()
                    {
                        StaffName = historyItem.Staff.StaffName,
                        MaterialName = historyItem.Material.MaterialName,
                        Quantity = historyItem.Quantity,
                        //Price = historyItem.InputPrice,
                        Date = historyItem.Date,
                        ActionName = historyItem.ActionType.ActionName
                    });
                }
            }
        }

        private void LoadCommands()
        {
            ApplyFilterCommand = new RelayCommand<object>(ExecuteApplyFilter);
            ClearFilterCommand = new RelayCommand<object>(ExecuteClearFilter);
            TogglePopupCommand = new RelayCommand<object>(ExecuteTogglePopup);
        }
    }
}
