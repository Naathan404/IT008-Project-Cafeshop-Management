using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Service.DTOs
{
    public class DepotItemDTO : BaseDTO
    {
        private int _materialId;
        private string? _materialName;
        private decimal _quantity;
        private string? _unit;
        private string? _note;
        public int MaterialId { get; set; }
        public string? MaterialName
        {
            get => _materialName;
            set =>SetProperty(ref _materialName, value);
        }

        public decimal Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        public string? Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        public string? Note
        {
            get => _note;
            set => SetProperty(ref _note, value);
        }
    }
}
