using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Models
{
    public class DepotHistoryItem : BaseViewModel
    {
        private string? _staffName;
        public string? StaffName
        {
            get => _staffName;
            set
            {
                if (_staffName != value)
                {
                    _staffName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _materialName;
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

        private decimal? _quantity;
        public decimal? Quantity
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

        //private decimal? _price;
        //public decimal? Price
        //{
        //    get => _price;
        //    set
        //    {
        //        if (_price != value)
        //        {
        //            _price = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        private DateTime? _date;
        public DateTime? Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _actionName;
        public string? ActionName
        {
            get => _actionName;
            set
            {
                if (_actionName != value)
                {
                    _actionName = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
