using CoffeeShop.Service.Interfaces;
using CoffeeShop.Service.DTOs;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.AdminVM
{
    public partial class AdminDiscountViewModel : BaseViewModel
    {
        protected IDialogService _dialogService;

        // Trong file Properties
        private ObservableCollection<CouponDTO> _allCoupons = new();
        public ObservableCollection<CouponDTO> AllCoupons
        {
            get => _allCoupons;
            set { _allCoupons = value; OnPropertyChanged(); }
        }

        private ObservableCollection<CouponDTO> _coupons = new();
        public ObservableCollection<CouponDTO> Coupons
        {
            get => _coupons;
            set { _coupons = value; OnPropertyChanged(); }
        }

        private CouponDTO? _selectedCoupon;
        public CouponDTO? SelectedCoupon
        {
            get => _selectedCoupon;
            set { _selectedCoupon = value; OnPropertyChanged(); }
        }

        private string _selectedType = "Tất cả";
        public string SelectedType
        {
            get => _selectedType;
            set { _selectedType = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private string _selectedPerformance = "Tất cả";
        public string SelectedPerformance
        {
            get => _selectedPerformance;
            set { _selectedPerformance = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private string _selectedStatus = "Tất cả";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private string _searchTerm = "";
        public string SearchTerm
        {
            get => _searchTerm;
            set { _searchTerm = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public int ActiveCouponsCount => Coupons?.Count(x => x.IsActive) ?? 0;

        public string BestPerformanceCoupon => Coupons?
            .Where(x => x.IsActive) // Chỉ xét những thằng đang bật
            .OrderByDescending(x => x.UsagePercentage)
            .FirstOrDefault()?.DiscountCode ?? "N/A";

        private decimal? _totalDiscountAmount;
        public decimal? TotalDiscountAmount
        {
            get => _totalDiscountAmount;
            set { _totalDiscountAmount = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> CouponTypes { get; } = new() { "Tất cả", "Tiền mặt", "Phần trăm" };
        public ObservableCollection<string> Performances { get; } = new() { "Tất cả", "Tốt", "Bình thường", "Tệ" };
        public ObservableCollection<string> Statuses { get; } = new() { "Tất cả", "Đang dùng", "Ngưng dùng" };
    }
}
