using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WebsiteUnoTe.Models;
using WebsiteUnoTe.Services;

namespace WebsiteUnoTe.ViewModels
{
    public class CartViewModel : INotifyPropertyChanged
    {
        private readonly CartManager _cartManager;
        private readonly ProductServiceProxy _inventory;
        private readonly ShoppingCartService _cartService;
        private readonly AppSettings _appSettings;

        private Product _selectedProduct;
        private Product _selectedCartItem;
        private ShoppingCart _selectedTargetCart;
        private string _quantity;
        private string _newCartName = "New Wishlist";
        private int _sortOption = 0;
        private bool _sortDescending = false;

        public ObservableCollection<Product> InventoryProducts => _inventory.Products;
        public ObservableCollection<Product> CartItems => _cartManager.ActiveCart.CartItems;
        public ObservableCollection<ShoppingCart> Carts => _cartManager.Carts;

        public IEnumerable<Product> SortedCartItems =>
            _cartManager.SortCartItems(_cartManager.ActiveCart, _sortOption, _sortDescending);

        public IEnumerable<ShoppingCart> OtherCarts =>
            _cartManager.Carts.Where(c => c != _cartManager.ActiveCart);

        public bool HasSelectedCartItem => SelectedCartItem != null;

        public bool CanCheckout => !IsActiveCartWishlist && CartItems.Any();

        public bool CanDeleteCart => _cartManager.Carts.Count > 1 && _cartManager.ActiveCart != null;

        public string TaxRateFormatted => $"Tax ({_appSettings.TaxRate:P0}): $";

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
            }
        }

        public Product SelectedCartItem
        {
            get => _selectedCartItem;
            set
            {
                _selectedCartItem = value;
                if (_selectedCartItem != null)
                {
                    Quantity = _selectedCartItem.Quantity.ToString();
                }
                OnPropertyChanged();
            }
        }

        public ShoppingCart SelectedTargetCart
        {
            get => _selectedTargetCart;
            set
            {
                _selectedTargetCart = value;
                OnPropertyChanged();
            }
        }

        public ShoppingCart ActiveCart
        {
            get => _cartManager.ActiveCart;
            set
            {
                _cartManager.ActiveCart = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CartItems));
                OnPropertyChanged(nameof(SortedCartItems));
                OnPropertyChanged(nameof(Subtotal));
                OnPropertyChanged(nameof(Tax));
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(IsActiveCartWishlist));
            }
        }

        public string Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }

        public string NewCartName
        {
            get => _newCartName;
            set
            {
                _newCartName = value;
                OnPropertyChanged();
            }
        }

        public int SortOption
        {
            get => _sortOption;
            set
            {
                _sortOption = value;
                OnPropertyChanged();
                ApplySorting();
            }
        }

        public bool SortDescending
        {
            get => _sortDescending;
            set
            {
                _sortDescending = value;
                OnPropertyChanged();
                ApplySorting();
            }
        }

        public bool IsActiveCartWishlist => _cartManager.ActiveCart.IsWishlist;

        public decimal Subtotal => _cartService.GetCartSubtotal();
        public decimal Tax => _cartService.GetCartTax();
        public decimal Total => _cartService.GetCartTotal();

        public ICommand AddToCartCommand { get; }
        public ICommand UpdateCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand GoToInventoryCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand SwitchCartCommand { get; }
        public ICommand CreateNewCartCommand { get; }
        public ICommand DeleteCartCommand { get; }
        public ICommand MoveToCartCommand { get; }

        public CartViewModel()
        {
            _cartManager = CartManager.Current;
            _inventory = ProductServiceProxy.Current;
            _cartService = new ShoppingCartService();
            _appSettings = AppSettings.Current;

            _selectedProduct = null;
            _selectedCartItem = null;
            _selectedTargetCart = null;
            _quantity = "1";

            AddToCartCommand = new Command(AddToCart);
            UpdateCartCommand = new Command(UpdateCart);
            RemoveFromCartCommand = new Command(RemoveFromCart);
            GoToInventoryCommand = new Command(async () => await Shell.Current.GoToAsync("//inventory"));
            CheckoutCommand = new Command(async () =>
            {
                MessagingCenter.Send(this, "CartSwitched");
                await Shell.Current.GoToAsync("//receipt");
            });

            SwitchCartCommand = new Command<ShoppingCart>(SwitchCart);
            CreateNewCartCommand = new Command(CreateNewCart);
            DeleteCartCommand = new Command<ShoppingCart>(DeleteCart);
            MoveToCartCommand = new Command<ShoppingCart>(MoveItemToSelectedCart);

            MessagingCenter.Subscribe<InventoryViewModel>(this, "CartUpdated", (sender) => {
                UpdateAllBindingsOnMainThread();
            });

            MessagingCenter.Subscribe<ShoppingCartService>(this, "DataChanged", (sender) => {
                UpdateAllBindingsOnMainThread();
            });

            MessagingCenter.Subscribe<ReceiptViewModel>(this, "DataChanged", (sender) => {
                UpdateAllBindingsOnMainThread();
            });

            MessagingCenter.Subscribe<AppSettings>(this, "TaxRateChanged", (sender) => {
                Application.Current.Dispatcher.Dispatch(() => {
                    OnPropertyChanged(nameof(Subtotal));
                    OnPropertyChanged(nameof(Tax));
                    OnPropertyChanged(nameof(Total));
                    OnPropertyChanged(nameof(TaxRateFormatted));
                });
            });

        }

        private void AddToCart()
        {
            if (SelectedProduct == null || !int.TryParse(Quantity, out int quantity) || quantity <= 0)
                return;

            _cartService.AddToCart(SelectedProduct.Id, quantity);

            UpdateAllBindingsOnMainThread();

            SelectedProduct = null;
            Quantity = "1";
        }

        private void UpdateCart()
        {
            if (SelectedCartItem == null || !int.TryParse(Quantity, out int quantity))
                return;

            _cartService.UpdateCartItem(SelectedCartItem.Id, quantity);

            UpdateAllBindingsOnMainThread();

            SelectedCartItem = null;
            Quantity = "1";
        }

        private void RemoveFromCart()
        {
            if (SelectedCartItem == null)
                return;

            _cartService.RemoveFromCart(SelectedCartItem.Id);

            UpdateAllBindingsOnMainThread();

            SelectedCartItem = null;
            Quantity = "1";
        }

        private void SwitchCart(ShoppingCart cart)
        {
            if (cart != null)
            {
                ActiveCart = cart;
                ApplySorting();
                MessagingCenter.Send(this, "CartSwitched");
            }
        }

        private void CreateNewCart()
        {
            if (string.IsNullOrWhiteSpace(NewCartName))
                return;

            var newCart = _cartService.CreateNewCart(NewCartName, true);
            ActiveCart = newCart;
            NewCartName = "New Wishlist";
        }

        private void DeleteCart(ShoppingCart cart)
        {
            if (cart != null && Carts.Count > 1)
            {
                _cartService.DeleteCart(cart.Id);
                OnPropertyChanged(nameof(Carts));
            }
        }

        private void MoveItemToSelectedCart(ShoppingCart targetCart)
        {
            if (SelectedCartItem != null && targetCart != null && targetCart != ActiveCart)
            {
                _cartService.MoveItemToCart(SelectedCartItem.Id, ActiveCart, targetCart);

                UpdateAllBindingsOnMainThread();

                SelectedCartItem = null;
                Quantity = "1";
            }
        }

        public void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<InventoryViewModel>(this, "CartUpdated");
            MessagingCenter.Unsubscribe<ShoppingCartService>(this, "DataChanged");
            MessagingCenter.Unsubscribe<ReceiptViewModel>(this, "DataChanged");
            MessagingCenter.Unsubscribe<AppSettings>(this, "TaxRateChanged");
        }

        private void ApplySorting()
        {
            OnPropertyChanged(nameof(SortedCartItems));
        }

        private void UpdateAllBindingsOnMainThread()
        {
            Application.Current.Dispatcher.Dispatch(() => {
                OnPropertyChanged(nameof(CartItems));
                OnPropertyChanged(nameof(SortedCartItems));
                OnPropertyChanged(nameof(Subtotal));
                OnPropertyChanged(nameof(Tax));
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(TaxRateFormatted));
                OnPropertyChanged(nameof(InventoryProducts));
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}