using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WebsiteUnoTe.Models;

namespace WebsiteUnoTe.Services
{
    public class CartManager : INotifyPropertyChanged
    {
        private static CartManager _instance;
        private static readonly object _instanceLock = new object();
        private int _nextCartId = 1;
        private decimal _taxRate = 0.07m; // tax rate en florida

        private ShoppingCart _activeCart;
        public ShoppingCart ActiveCart
        {
            get => _activeCart;
            set
            {
                if (_activeCart != value)
                {
                    _activeCart = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal TaxRate
        {
            get => _taxRate;
            set
            {
                if (_taxRate != value && value >= 0 && value <= 1)
                {
                    _taxRate = value;
                    OnPropertyChanged();
                    SaveTaxRate();
                }
            }
        }

        public ObservableCollection<ShoppingCart> Carts { get; private set; }

        private CartManager()
        {
            Carts = new ObservableCollection<ShoppingCart>();
            LoadTaxRate();

            var defaultCart = CreateNewCart("My Cart");
            CreateNewCart("Wishlist", true);

            ActiveCart = defaultCart;
        }

        public static CartManager Current
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new CartManager();
                    }
                }
                return _instance;
            }
        }

        public ShoppingCart CreateNewCart(string name, bool isWishlist = false)
        {
            var cart = new ShoppingCart(_nextCartId++, name, isWishlist);
            Carts.Add(cart);
            return cart;
        }

        public bool DeleteCart(int cartId)
        {
            var cart = Carts.FirstOrDefault(c => c.Id == cartId);
            if (cart != null)
            {
                foreach (var item in cart.CartItems.ToList())
                {
                    var product = ProductServiceProxy.Current.GetById(item.Id);
                    if (product != null)
                    {
                        product.Quantity += item.Quantity;
                    }
                    cart.RemoveFromCart(item);
                }

                Carts.Remove(cart);

                if (ActiveCart == cart && Carts.Count > 0)
                {
                    ActiveCart = Carts[0];
                }

                return true;
            }
            return false;
        }

        public IEnumerable<Product> SortCartItems(ShoppingCart cart, int sortOption, bool descending)
        {
            return sortOption switch
            {
                0 => descending ? cart.CartItems.OrderByDescending(p => p.Id) : cart.CartItems.OrderBy(p => p.Id),
                1 => descending ? cart.CartItems.OrderByDescending(p => p.Name) : cart.CartItems.OrderBy(p => p.Name),
                2 => descending ? cart.CartItems.OrderByDescending(p => p.Price) : cart.CartItems.OrderBy(p => p.Price),
                _ => cart.CartItems
            };
        }

        private void LoadTaxRate()
        {
            _taxRate = Convert.ToDecimal(Preferences.Get("TaxRate", "0.07"));
        }

        private void SaveTaxRate()
        {
            Preferences.Set("TaxRate", _taxRate.ToString());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}