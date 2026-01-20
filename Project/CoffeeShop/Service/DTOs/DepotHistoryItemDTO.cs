using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Service.DTOs
{
    public class DepotHistoryItemDTO : BaseDTO
    {
        private string? _staffName;
        public string? StaffName
        {
            get => _staffName;
            set => SetProperty(ref _staffName, value);
        }

        private int _materialId;
        public int MaterialId
        {
            get => _materialId;
            set => SetProperty(ref _materialId, value);
        }

        private string? _materialName;
        public string? MaterialName
        {
            get => _materialName;
            set => SetProperty(ref _materialName, value);
        }

        private decimal? _quantity;
        public decimal? Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        private DateTime? _date;
        public DateTime? Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        private string? _actionName;
        public string? ActionName
        {
            get => _actionName;
            set => SetProperty(ref _actionName, value);
        }
    }
}
