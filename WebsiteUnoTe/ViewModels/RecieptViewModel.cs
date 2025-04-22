using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WebsiteUnoTe.Models;
using WebsiteUnoTe.Services;

namespace WebsiteUnoTe.ViewModels
{
    public class ReceiptViewModel : INotifyPropertyChanged
    {
        private readonly CartManager _cartManager;
        private readonly ShoppingCartService _cartService;
        private readonly AppSettings _appSettings;
        private ObservableCollection<Product> _receiptItems;

        public ObservableCollection<Product> ReceiptItems
        {
            get => _receiptItems;
            private set
            {
                _receiptItems = value;
                OnPropertyChanged();
            }
        }

        public decimal Subtotal => _cartManager.ActiveCart.GetTotal();
        public decimal Tax => _cartManager.ActiveCart.GetTax(_appSettings.TaxRate);
        public decimal Total => _cartManager.ActiveCart.GetTotalWithTax(_appSettings.TaxRate);

        public string TaxRateDisplay => $"{_appSettings.TaxRate:P0}";

        public ICommand FinishCommand { get; }
        public ICommand SaveForLaterCommand { get; }

        public ReceiptViewModel()
        {
            _cartManager = CartManager.Current;
            _cartService = new ShoppingCartService();
            _appSettings = AppSettings.Current;

            _receiptItems = new ObservableCollection<Product>();
            RefreshReceiptItems();

            FinishCommand = new Command(FinishShopping);
            SaveForLaterCommand = new Command(SaveCartForLater);

            MessagingCenter.Subscribe<CartViewModel>(this, "CartSwitched", (sender) =>
            {
                RefreshReceiptItems();
                OnPropertyChanged(nameof(Subtotal));
                OnPropertyChanged(nameof(Tax));
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(TaxRateDisplay));
            });


        }


        public void OnAppearing()
        {

            RefreshReceiptItems();

            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Tax));
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(TaxRateDisplay));
        }

        private void RefreshReceiptItems()
        {
            var items = new ObservableCollection<Product>();
            foreach (var item in _cartManager.ActiveCart.CartItems)
            {
                items.Add(new Product
                {
                    Id = item.Id,
                    Name = item.Name,
                    Price = item.Price,
                    Quantity = item.Quantity
                });
            }
            ReceiptItems = items;
        }

        private async void FinishShopping()
        {
            _cartService.ClearCart();

            MessagingCenter.Send(this, "DataChanged");

            await Shell.Current.GoToAsync("//inventory");
        }

        private async void SaveCartForLater()
        {
            var wishlistName = $"Saved Order {DateTime.Now:MM/dd/yyyy}";
            var wishlist = _cartService.CreateNewCart(wishlistName, true);

            foreach (var item in _cartManager.ActiveCart.CartItems.ToList())
            {
                _cartService.MoveItemToCart(item.Id, _cartManager.ActiveCart, wishlist);
            }

            MessagingCenter.Send(this, "DataChanged");

            await Shell.Current.GoToAsync("//inventory");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}