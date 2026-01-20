using CoffeeShop.ViewModels.AdminVM;
using System.ComponentModel;
using System.Windows;

namespace CoffeeShop.View.Admin
{
    public partial class ItemEditWindow : Window
    {
        public ItemEditWindow()
        {
            InitializeComponent();
            DataContext = new ItemEditViewModel();
            SubscribeToViewModel();
        }

        public ItemEditWindow(int itemId)
        {
            InitializeComponent();
            DataContext = new ItemEditViewModel(itemId);
            SubscribeToViewModel();
        }

        private void SubscribeToViewModel()
        {
            if (DataContext is ItemEditViewModel vm)
            {
                vm.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemEditViewModel.DialogResult))
            {
                if (DataContext is ItemEditViewModel vm && vm.DialogResult.HasValue)
                {
                    this.DialogResult = vm.DialogResult;
                    this.Close();
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Unsubscribe để tránh memory leak
            if (DataContext is ItemEditViewModel vm)
            {
                vm.PropertyChanged -= ViewModel_PropertyChanged;
            }
            base.OnClosing(e);
        }
    }
}