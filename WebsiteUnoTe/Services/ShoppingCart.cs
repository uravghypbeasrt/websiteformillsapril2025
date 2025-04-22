using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WebsiteUnoTe.Models;

namespace WebsiteUnoTe.Services
{
    public class ShoppingCart : INotifyPropertyChanged
    {
        private int _id;
        private string _name = string.Empty;
        private bool _isWishlist;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsWishlist
        {
            get => _isWishlist;
            set
            {
                if (_isWishlist != value)
                {
                    _isWishlist = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Product> CartItems { get; private set; } = new ObservableCollection<Product>();

        public ShoppingCart()
        {
        }

        public ShoppingCart(int id, string name, bool isWishlist = false)
        {
            Id = id;
            Name = name;
            IsWishlist = isWishlist;
            CartItems = new ObservableCollection<Product>();
        }

        public void AddToCart(Product product, int quantity)
        {
            var cartItem = CartItems.FirstOrDefault(p => p.Id == product.Id);
            if (cartItem == null)
            {
                CartItems.Add(new Product
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity
                });
            }
            else
            {
                cartItem.Quantity += quantity;
            }
        }

        public void UpdateCart(int productId, int quantity)
        {
            var cartItem = CartItems.FirstOrDefault(p => p.Id == productId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
            }
        }

        public void RemoveFromCart(Product product)
        {
            var cartItem = CartItems.FirstOrDefault(p => p.Id == product.Id);
            if (cartItem != null)
            {
                CartItems.Remove(cartItem);
            }
        }

        public decimal GetTotal()
        {
            return CartItems.Sum(p => p.Price * p.Quantity);
        }

        public decimal GetTax(decimal taxRate)
        {
            return GetTotal() * taxRate;
        }

        public decimal GetTotalWithTax(decimal taxRate)
        {
            return GetTotal() + GetTax(taxRate);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{Name} ({CartItems.Count} items, ${GetTotal():F2})";
        }
    }
}