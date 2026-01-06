using System.Collections.ObjectModel;
using System.Windows.Media;

namespace CoffeeShop.Service.DTOs
{
    public class CouponDTO : BaseDTO
    {
        public int DiscountId { get; set; }
        public string DiscountCode { get; set; } = null!;
        public string DiscountName { get; set; } = null!;

        private string _discountType = "Tiền mặt";
        public string DiscountType
        {
            get => _discountType;
            set
            {
                if (_discountType == "Tiền mặt")
                {
                    DiscountValue = 0;
                }
                else if (_discountType == "Phần trăm")
                {
                    DiscountValue = 0;

                    MaximumDiscountAmount = DiscountValue;
                }
                _discountType = value;
                OnPropertyChanged();
                OnPropertyChanged("PercentInputVisibility");
                OnPropertyChanged(nameof(DiscountValue));
                OnPropertyChanged(nameof(MaximumDiscountAmount));
            }
        }

        private decimal _discountValue;
        public decimal DiscountValue
        {
            get => _discountValue;
            set
            {
                _discountValue = value;
                if (_discountType == "Tiền mặt")
                {
                    MaximumDiscountAmount = DiscountValue;
                }    
                OnPropertyChanged();
                OnPropertyChanged(nameof(MaximumDiscountAmount));
            }
        }
        public decimal? MinimumOrderValue { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }

        private int _usedCount;
        public int UsedCount
        {
            get => _usedCount;
            set
            {
                _usedCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UsageDetail));
                CalculateUsage();
            }
        }

        private int _useLimit = 100;
        public int UseLimit
        {
            get => _useLimit;
            set
            {
                _useLimit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UsageDetail));
                CalculateUsage();
            }
        }

        public string UsageDetail => $"{UsedCount} / {UseLimit}";

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive && (UsedCount < UseLimit);
            set { _isActive = value; OnPropertyChanged(); }
        }

        public string DiscountStatus => IsActive ? "Đang dùng" : "Ngưng dùng";
        private double _usagePercentage;
        public double UsagePercentage
        {
            get => _usagePercentage;
            set
            {
                _usagePercentage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UsagePercentage));
                OnPropertyChanged(nameof(PerformanceColor));
            }
        }

        public void CalculateUsage()
        {
            if (UseLimit > 0)
            {
                // Phải ép kiểu (double) ở đây nè m!
                UsagePercentage = ((double)UsedCount / UseLimit) * 100;
                if (UsagePercentage > 100) UsagePercentage = 100;
            }
            else
            {
                UsagePercentage = 0;
            }
        }

        // Màu sắc và hiệu ứng dòng (Mờ đi khi ko hoạt động)
        public Brush PerformanceColor
        {
            get
            {
                if (UsagePercentage >= 80) return Brushes.Green;    
                if (UsagePercentage >= 40) return Brushes.Orange; 
                return Brushes.Red;                              
            }
        }
        public double RowOpacity => IsActive ? 1.0 : 0.5;
        public string RowForeground => IsActive ? "#340D05" : "#888888";
    }
}
