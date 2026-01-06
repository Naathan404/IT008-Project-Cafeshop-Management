using CoffeeShop.Models;
using CoffeeShop.Service.DTOs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.AdminVM
{
    public class InsertCouponViewModel : BaseViewModel
    {
        // --- Properties ---

        private string _title = "THÊM MÃ GIẢM GIÁ";
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        public bool IsEditMode { get; set; }
        public ObservableCollection<string> CouponTypes { get; } = new() { "Tiền mặt", "Phần trăm" };

        // Thuộc tính để khóa ô nhập Mã khi Sửa
        public bool IsCodeEnabled => !IsEditMode;

        // Logic ẩn hiện Binding trực tiếp từ DiscountType
        public Visibility PercentInputVisibility => (Coupon?.DiscountType == "Phần trăm") ? Visibility.Visible : Visibility.Collapsed;

        private CouponDTO _coupon;
        public CouponDTO Coupon
        {
            get => _coupon;
            set
            {
                _coupon = value;
                OnPropertyChanged();
                RefreshVisibility();
            }
        }

        public void RefreshVisibility()
        {
            OnPropertyChanged(nameof(PercentInputVisibility));
        }

        // --- Commands ---
        public ICommand ApplyCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public InsertCouponViewModel(CouponDTO? itemToEdit = null)
        {
            if (itemToEdit != null)
            {
                // CHẾ ĐỘ CẬP NHẬT
                IsEditMode = true;
                Title = "CẬP NHẬT MÃ GIẢM GIÁ";
                // Clone dữ liệu để tránh sửa trực tiếp lên DataGrid khi chưa nhấn Apply
                Coupon = new CouponDTO
                {
                    DiscountId = itemToEdit.DiscountId,
                    DiscountCode = itemToEdit.DiscountCode,
                    DiscountName = itemToEdit.DiscountName,
                    DiscountType = itemToEdit.DiscountType,
                    DiscountValue = itemToEdit.DiscountValue,
                    MinimumOrderValue = itemToEdit.MinimumOrderValue,
                    MaximumDiscountAmount = itemToEdit.MaximumDiscountAmount,
                    UseLimit = itemToEdit.UseLimit,
                    UsedCount = itemToEdit.UsedCount,
                    IsActive = itemToEdit.IsActive
                };
            }
            else
            {
                // CHẾ ĐỘ THÊM MỚI
                IsEditMode = false;
                Coupon = new CouponDTO { DiscountType = "Tiền mặt", IsActive = true, UseLimit = 100 };
            }


            ApplyCommand = new RelayCommand<Window>(async w => {
                if (string.IsNullOrWhiteSpace(Coupon.DiscountCode))
                {
                    MessageBox.Show("Mã giảm giá không được để trống!");
                    return;
                }

                try
                {
                    using (var db = new CoffeeShopContext())
                    {
                        if (IsEditMode)
                        {
                            // --- CHẾ ĐỘ CẬP NHẬT ---
                            var discountInDb = await db.Discounts.FindAsync(Coupon.DiscountId);
                            // --- TRONG CHẾ ĐỘ CẬP NHẬT ---
                            if (discountInDb != null)
                            {
                                discountInDb.DiscountCode = Coupon.DiscountCode;
                                discountInDb.DiscountName = Coupon.DiscountName;
                                discountInDb.DiscountType = Coupon.DiscountType == "Tiền mặt" ? 0 : 1;
                                discountInDb.DiscountValue = Coupon.DiscountValue;
                                discountInDb.MinimumOrderValue = Coupon.MinimumOrderValue;
                                discountInDb.MaximumDiscountAmount = Coupon.MaximumDiscountAmount ?? Coupon.DiscountValue;
                                discountInDb.IsActive = Coupon.IsActive;

                                discountInDb.UseLimit = Coupon.UseLimit;
                            }
                        }
                        else
                        {
                            // --- CHẾ ĐỘ THÊM MỚI ---
                            var newDiscount = new Discount
                            {
                                DiscountCode = Coupon.DiscountCode,
                                DiscountName = Coupon.DiscountName,
                                DiscountType = Coupon.DiscountType == "Tiền mặt" ? 0 : 1,
                                DiscountValue = Coupon.DiscountValue,
                                MinimumOrderValue = Coupon.MinimumOrderValue,
                                // Nếu là tiền mặt thì MaxDiscount = DiscountValue
                                MaximumDiscountAmount = Coupon.MaximumDiscountAmount ?? Coupon.DiscountValue,
                                UsedCount = 0,
                                UseLimit = Coupon.UseLimit,
                                IsActive = true
                            };
                            db.Discounts.Add(newDiscount);
                        }

                        await db.SaveChangesAsync();

                        if (w != null)
                        {
                            w.DialogResult = true;
                            w.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message);
                }
            });

            CancelCommand = new RelayCommand<Window>(w => w?.Close());

            Coupon.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Coupon.DiscountType))
                {
                    OnPropertyChanged(nameof(PercentInputVisibility));
                }
            };
        }
    }
}
