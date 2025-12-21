using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Service.DTOs
{
    // Class DepotItem
    public class DepotItemDTO : BaseViewModel
    {
        // Backing field (nơi lưu giá trị thật sự) 
        private int _materialId;
        private string? _materialName;
        private decimal _quantity;
        private string? _unit;
        private string? _note;

        // --- CÁC PROPERTY - Khi có sự thay đổi mới gián giá trị mới cho backing field ---
        public int MaterialId { get; set; } /// Ko có sự thay đổi ID nên ko cần định nghĩa
        // 1. MaterialName
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

        // 2. Quantity
        public decimal Quantity
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

        // 3. Unit
        public string? Unit
        {
            get => _unit;
            set
            {
                if (_unit != value)
                {
                    _unit = value;
                    OnPropertyChanged();
                }
            }
        }

        // 4. Note
        public string? Note
        {
            get => _note;
            set
            {
                if (_note != value)
                {
                    _note = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
