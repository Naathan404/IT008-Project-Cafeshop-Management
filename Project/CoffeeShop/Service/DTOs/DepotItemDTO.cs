using System;

namespace CoffeeShop.Service.DTOs
{
    public class DepotItemDTO : BaseDTO
    {
        private int _materialId;
        private string? _materialName;
        private decimal _quantity;
        private string? _unit;
        private string? _note;
        private decimal _threshold;

        public int MaterialId
        {
            get => _materialId;
            set => SetProperty(ref _materialId, value);
        }

        public string MaterialName
        {
            get => _materialName ?? string.Empty;
            set => SetProperty(ref _materialName, value);
        }

        public decimal Quantity
        {
            get => _quantity;
            set
            {
                SetProperty(ref _quantity, value);
                OnPropertyChanged(nameof(StatusBackground));
            }
        }

        public string Unit
        {
            get => _unit ?? string.Empty;
            set => SetProperty(ref _unit, value);
        }

        public string Note
        {
            get => _note ?? string.Empty;
            set => SetProperty(ref _note, value);
        }

        public decimal Threshold
        {
            get => _threshold;
            set => SetProperty(ref _threshold, value);
        }

        public string StatusBackground
        {
            get
            {
                if (Quantity <= 0) return "#FF9999";
                if (Quantity <= Threshold) return "#FFE066"; 
                return "Transparent";
            }
        }
    }
}