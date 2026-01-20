using CoffeeShop.Models;
using CoffeeShop.Service.DTOs;
using CoffeeShop.Service.Interfaces;
using CoffeeShop.View.Controls;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using static CoffeeShop.View.Controls.CustomMessageBox;

namespace CoffeeShop.ViewModels.AdminVM
{
    public partial class AdminDiscountViewModel : BaseViewModel
    {
        public ICommand RefreshPageCommand { get; set; }
        public ICommand InsertCouponCommand { get; set; }
        public ICommand UpdateCouponCommand { get; set; }
        public ICommand DeleteCouponCommand { get; set; }
        public ICommand ToggleStatusCommand { get; set; }

        public AdminDiscountViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            RefreshPageCommand = new RelayCommand<object>(p => _ = LoadData());
            InsertCouponCommand = new RelayCommand<object>(p => ExecuteInsert());
            UpdateCouponCommand = new RelayCommand<CouponDTO?>(ExecuteUpdate, (p) => p != null);
            DeleteCouponCommand = new RelayCommand<CouponDTO?>(ExecuteDelete, (p) => p != null);
            ToggleStatusCommand = new RelayCommand<object>(p => ExecuteToggleStatus());

            _ = LoadData();
        }

        public async Task LoadData()
        {
            // Load lại các giá trị lọc/tìm kiếm
            SelectedType = CouponTypes.First();
            SelectedPerformance = Performances.First();
            SelectedStatus = Statuses.First();
            SearchTerm = string.Empty;

            try
            {
                using (var db = new CoffeeShopContext())
                {
                    var dataFromDb = await db.Discounts.ToListAsync();

                    var result = dataFromDb.Select(d =>
                    {
                        var dto = new CouponDTO
                        {
                            DiscountId = d.DiscountId,
                            DiscountCode = d.DiscountCode,
                            DiscountName = d.DiscountName,
                            DiscountType = d.DiscountType == 0 ? "Tiền mặt" : "Phần trăm",
                            DiscountValue = d.DiscountValue,
                            MinimumOrderValue = d.MinimumOrderValue,
                            MaximumDiscountAmount = d.MaximumDiscountAmount ?? d.DiscountValue,
                            UsedCount = d.UsedCount,
                            IsActive = d.IsActive,
                            UseLimit = d.UseLimit,
                        };

                        if (dto.UseLimit > 0)
                        {
                            dto.UsagePercentage = ((double)dto.UsedCount / dto.UseLimit) * 100;
                        }
                        else
                        {
                            dto.UsagePercentage = 0;
                        }

                        return dto;
                    }).ToList();

                    // Cập nhật danh sách trên UI Thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AllCoupons = new ObservableCollection<CouponDTO>(result);
                        ApplyFilter();
                        RefreshStatistics();
                    });
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageButtons.OK, MessageType.Error);
            }
        }

        public void ApplyFilter()
        {
            var filtered = AllCoupons.AsEnumerable();

            if (SelectedType != "Tất cả") filtered = filtered.Where(x => x.DiscountType == SelectedType);
            if (SelectedStatus != "Tất cả") filtered = filtered.Where(x => x.DiscountStatus == SelectedStatus);
            if (!string.IsNullOrEmpty(SearchTerm))
                filtered = filtered.Where(x => x.DiscountCode.ToLower().Contains(SearchTerm.ToLower()));

            if (SelectedPerformance == "Tốt") filtered = filtered.Where(x => x.UsagePercentage >= 80);
            else if (SelectedPerformance == "Bình thường") filtered = filtered.Where(x => x.UsagePercentage >= 40 && x.UsagePercentage < 80);
            else if (SelectedPerformance == "Tệ") filtered = filtered.Where(x => x.UsagePercentage < 40);

            Coupons = new ObservableCollection<CouponDTO>(filtered);
            RefreshStatistics();
        }

        private void ExecuteInsert()
        {
            // Truyền null vì là thêm mới
            if (_dialogService.OpenInsertCouponWindow() == true)
            {
                _ = LoadData(); // Load lại bảng nếu người dùng ấn Apply (trả về true)
            }
        }

        private void ExecuteUpdate(CouponDTO? p)
        {
            if (SelectedCoupon == null) return;

            // Truyền SelectedCoupon vào để sửa
            if (_dialogService.OpenInsertCouponWindow(SelectedCoupon) == true)
            {
                _ = LoadData();
            }
        }

        private async void ExecuteDelete(CouponDTO? p)
        {
            if (SelectedCoupon == null)
            {
                CustomMessageBox.Show("Hãy chọn mã cần xóa!", "Thông báo", MessageButtons.OK, MessageType.Info);
                return;
            }

            var result = CustomMessageBox.Show($"Bạn có chắc muốn xóa mã {SelectedCoupon.DiscountCode} không?",
                                        "Xác nhận xóa", MessageButtons.YesNo, MessageType.Question);

            if (result == CustomMessageBox.MessageBoxResult.Yes)
            {
                using (var db = new CoffeeShopContext())
                {
                    var couponInDb = await db.Discounts.FindAsync(SelectedCoupon.DiscountId);
                    if (couponInDb != null)
                    {
                        db.Discounts.Remove(couponInDb);
                        await db.SaveChangesAsync();
                        await LoadData(); // Cập nhật lại danh sách và thống kê
                    }
                }
            }
        }

        private async void ExecuteToggleStatus()
        {
            if (SelectedCoupon == null) return;

            using (var db = new CoffeeShopContext())
            {
                var couponInDb = await db.Discounts.FindAsync(SelectedCoupon.DiscountId);
                if (couponInDb != null)
                {
                    // Đảo ngược trạng thái
                    couponInDb.IsActive = !couponInDb.IsActive;
                    await db.SaveChangesAsync();

                    await LoadData(); // Load lại để tính toán lại các con số thống kê
                }
            }
        }

        public void RefreshStatistics()
        {
            if (AllCoupons != null)
            {
                decimal total = AllCoupons.Sum(x => {
                    decimal count = (decimal)x.UsedCount;
                    decimal amount = x.MaximumDiscountAmount ?? x.DiscountValue;
                    return count * amount;
                });

                TotalDiscountAmount = total;
            }

            OnPropertyChanged(nameof(ActiveCouponsCount));
            OnPropertyChanged(nameof(BestPerformanceCoupon));
        }
    }
}
