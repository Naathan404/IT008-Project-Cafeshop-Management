using CoffeeShop.Models;
using CoffeeShop.Service.DTOs;
using CoffeeShop.Service.Interfaces;
using CoffeeShop.View.General;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.AdminVM
{
    public partial class AdminHistoryViewModel : BaseViewModel
    {
        CultureInfo viVn = new CultureInfo("vn-VN");
        CancellationTokenSource _cts;
        IDialogService _dialogService;
        public ICommand RefreshPageCommand { get; set; }
        public ICommand ShowOrderDetailCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand ExportExcelCommand { get; set; }

        private DateTime? _fromDate;
        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                if (_fromDate != value)
                {
                    _fromDate = value;
                    OnPropertyChanged();

                    if (SelectedPeriod != "Tùy chọn")
                    {
                        _selectedPeriod = "Tùy chọn";
                        OnPropertyChanged(nameof(SelectedPeriod));
                    }
                    _ = LoadOrderHistory();
                }
            }
        }

        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                if (_toDate != value)
                {
                    _toDate = value;
                    OnPropertyChanged();

                    if (SelectedPeriod != "Tùy chọn")
                    {
                        _selectedPeriod = "Tùy chọn";
                        OnPropertyChanged(nameof(SelectedPeriod));
                    }

                    _ = LoadOrderHistory();
                }
            }
        }

        public ObservableCollection<string> PaymentMethods { get; } = new ObservableCollection<string>
        {
            "Tất cả",
            "Tiền mặt",
            "Chuyển khoản"
        };


        private string _selectedPaymentMethod = "";
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                _selectedPaymentMethod = value;
                OnPropertyChanged();
                _ = LoadOrderHistory();
            }
        }
        public ObservableCollection<string> Periods { get; } = new ObservableCollection<string>
        {
            "Hôm nay",
            "Tuần này",
            "Tháng này",
            "Tùy chọn"
        };

        private string _selectedPeriod = "Hôm nay";
        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                _selectedPeriod = value;
                OnPropertyChanged();
                _ = LoadOrderHistory();
            }
        }

        public Visibility OrderDetailVisibility
        {
            get => SelectedOrder != null ? Visibility.Visible : Visibility.Collapsed;
        }

        private ObservableCollection<OrderHistory> _orders = new ObservableCollection<OrderHistory>();
        public ObservableCollection<OrderHistory> Orders
        {
            get => _orders;
            set
            {
                _orders = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<OrderDetailDTO> _orderDetails = new ObservableCollection<OrderDetailDTO>();
        public ObservableCollection<OrderDetailDTO> OrderDetails
        {
            get => _orderDetails;
            set
            {
                _orderDetails = value;
                OnPropertyChanged();
            }
        }

        private OrderHistory? _selectedOrder = null;
        public OrderHistory? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(OrderDetailVisibility));

                _ = LoadOrderDetail();
            }
        }

        private string _customerNameFilter = "";
        public string CustomerNameFilter
        {
            get => _customerNameFilter;
            set
            {
                _customerNameFilter = value;
                OnPropertyChanged();
                _ = LoadOrderHistory();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        private string _totalRevenue = "0 đ";
        public string TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                _totalRevenue = value;
                OnPropertyChanged();
            }
        }

        private string _totalOrders = "0";
        public string TotalOrders
        {
            get => _totalOrders;
            set
            {
                _totalOrders = value;
                OnPropertyChanged();
            }
        }

        private string _totalCash = "0 đ";
        public string TotalCash
        {
            get => _totalCash;
            set
            {
                _totalCash = value;
                OnPropertyChanged();
            }
        }

        private string _totalBankTransfer = "0 đ";
        public string TotalBankTransfer
        {
            get => _totalBankTransfer;
            set
            {
                _totalBankTransfer = value;
                OnPropertyChanged();
            }
        }

        private string _totalDiscount = "0 đ";
        public string TotalDiscount
        {
            get => _totalDiscount;
            set
            {
                _totalDiscount = value;
                OnPropertyChanged();
            }
        }
    }
}
