using CoffeeShop.Models;
using CoffeeShop.Service.DTOs;
using CoffeeShop.View.General;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.AdminVM
{
    public partial class AdminDiscountViewModel : BaseViewModel
    {
        public ICommand RefreshPageCommand { get; set; } = null!;
        public ICommand InsertCouponCommand { get; set; } = null!;
        public ICommand UpdateCouponCommand { get; set; } = null!;
        public ICommand DeleteCouponCommand { get; set; } = null!;

        public AdminDiscountViewModel()
        {
            RefreshPageCommand = new RelayCommand<object>(
                async (p) =>
                {
                    await LoadCouponList();
                },
                (p) => true
            );



            _ = LoadCouponList();
        }

        private async Task LoadCouponList()
        {
        }

        private void LoadCommands()
        {

        }

        public class Discount
        {
            public int DiscountId { get; set; }

            public string DiscountCode { get; set; } = null!;

            public string DiscountName { get; set; } = null!;

            public int DiscountType { get; set; }

            public decimal DiscountValue { get; set; }

            public decimal? MinimumOrderValue { get; set; }

            public decimal? MaximumDiscountAmount { get; set; }

            public bool IsActive { get; set; }

            public int UsedCount { get; set; }
            public int UseLimit { get; set; }
        }
    }
}
