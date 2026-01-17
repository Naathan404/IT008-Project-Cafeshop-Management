using CoffeeShop.Models;
using CoffeeShop.Service;
using CoffeeShop.ViewModels.AdminVM;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static CoffeeShop.ViewModels.StaffVM.StaffOrderViewModel;

namespace CoffeeShop.ViewModels.StaffVM
{
    public partial class StaffTableViewModel : NotificationBase
    {
        #region Properties
        // Danh sách gốc chứa tất cả các bàn lấy từ Database
        private List<VMTable> _allTables = new List<VMTable>();

        // Danh sách hiển thị trên UI sau khi đã lọc
        private ObservableCollection<VMTable> _filteredTables = new ObservableCollection<VMTable>();
        public ObservableCollection<VMTable> FilteredTables
        {
            get => _filteredTables;
            set
            {
                _filteredTables = value;
                OnPropertyChanged();
            }
        }

        // Tab được chọn (0: Trống, 1: Đang phục vụ, -1 hoặc null: Tất cả)
        private TabItem? _selectedTabItem;
        public TabItem? SelectedTabItem
        {
            get => _selectedTabItem;
            set
            {
                _selectedTabItem = value;
                OnPropertyChanged();

                // Lấy TableStatus từ Tag của TabItem
                if (value?.Tag is string tag && int.TryParse(tag, out int status))
                {
                    CurrentTableStatus = status;
                }
                else
                {
                    CurrentTableStatus = -1; // -1 đại diện cho tab "Tất cả"
                }
            }
        }

        private int _currentTableStatus = -1;
        public int CurrentTableStatus
        {
            get => _currentTableStatus;
            set
            {
                _currentTableStatus = value;
                OnPropertyChanged();
                FilterTables(); // Mỗi khi đổi status thì lọc lại danh sách
            }
        }

        private VMTable? _selectedTable;
        public VMTable? SelectedTable
        {
            get => _selectedTable;
            set { _selectedTable = value; OnPropertyChanged(); }
        }

        private string _searchTableKeyWords;
        public string SearchTableKeyWords
        {
            get => _searchTableKeyWords;
            set
            {
                _searchTableKeyWords = value;
                OnPropertyChanged();
                FilterTables(); // Gọi hàm lọc mỗi khi người dùng nhập chữ
            }
        }

        #endregion

        #region Commands
        public ICommand ChangeStatusToServingCommand { get; set; }
        public ICommand ChangeStatusToEmptyCommand { get; set; }
        public ICommand LoadTablesCommand { get; set; }
        #endregion

        public StaffTableViewModel()
        {
            EventAggregator.Instance.Subscribe<OrderCompletedMessage>(async (msg) =>
            {
                LoadData();
            });
            ChangeStatusToServingCommand = new RelayCommand<VMTable>(
                p => UpdateTableStatus(p, 1), // Execute
                p => p != null                // CanExecute
            );

            ChangeStatusToEmptyCommand = new RelayCommand<VMTable>(
                p => UpdateTableStatus(p, 0),
                p => p != null
            );
            LoadData();
        }

        #region Methods
        public void LoadData()
        {
            using (var context = new CoffeeShopContext())
            {
                var dbTables = context.CafeTables.Where(t => t.IsDeleted == false).ToList();

                _allTables = dbTables.Select(t => new VMTable
                {
                    TableId = t.TableId,
                    TableName = t.TableName,
                    TableStatus = t.TableStatus, // 0: Trống, 1: Phục vụ
                    Note = t.Note
                }).ToList();

                FilterTables();
            }
        }

        // Hàm cập nhật trạng thái xuống Database
        private void UpdateTableStatus(VMTable table, int newStatus)
        {
            using (var context = new CoffeeShopContext())
            {
                var dbTable = context.CafeTables.Find(table.TableId);
                if (dbTable != null)
                {
                    dbTable.TableStatus = newStatus;
                    context.SaveChanges();

                    // Cập nhật lại trong danh sách local để UI thay đổi ngay lập tức
                    table.TableStatus = newStatus;

                    // Nếu tab hiện tại không phải "Tất cả", phải xóa bàn đó khỏi view hiện tại
                    FilterTables();
                }
            }
        }
        // Hàm lọc bàn (Kết hợp cả Lọc theo Tab và Lọc theo Tên)
        private void FilterTables()
        {
            if (_allTables == null) return;

            var filtered = _allTables.AsEnumerable();

            // Lọc theo Tab (Trạng thái)
            if (SelectedTabItem != null && SelectedTabItem.Tag.ToString() != "-1")
            {
                int status = int.Parse(SelectedTabItem.Tag.ToString());
                filtered = filtered.Where(t => t.TableStatus == status);
            }

            // Lọc theo Tên (Tìm kiếm)
            if (!string.IsNullOrEmpty(SearchTableKeyWords))
            {
                filtered = filtered.Where(t => t.TableName.ToLower().Contains(SearchTableKeyWords.ToLower()));
            }

            FilteredTables = new ObservableCollection<VMTable>(filtered);
        }
        #endregion

        #region Helper Class
        public class VMTable : NotificationBase
        {
            private int _tableId;
            private string _tableName = string.Empty;
            private int _tableStatus;
            private string? _note;

            public int TableId { get => _tableId; set { _tableId = value; OnPropertyChanged(); } }
            public string TableName { get => _tableName; set { _tableName = value ?? string.Empty; OnPropertyChanged(); } }
            public int TableStatus { get => _tableStatus; set { _tableStatus = value; OnPropertyChanged(); } }
            public string? Note { get => _note; set { _note = value; OnPropertyChanged(); } }
        }

        public class NotificationBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
