using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WebsiteUnoTe.Models;
using WebsiteUnoTe.Services;

namespace WebsiteUnoTe.ViewModels
{
    public class InventoryViewModel : INotifyPropertyChanged
    {
        private readonly ProductServiceProxy _productService;
        private readonly ShoppingCartService _cartService;
        private Product _selectedProduct;
        private string _productName;
        private string _productPrice;
        private string _productQuantity;
        private string _quickAddQuantity = "1";
        private int _sortOption = 0;
        private bool _sortDescending = false;

        public ObservableCollection<Product> Products => _productService.Products;

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                if (_selectedProduct != null)
                {
                    ProductName = _selectedProduct.Name;
                    ProductPrice = _selectedProduct.Price.ToString();
                    ProductQuantity = _selectedProduct.Quantity.ToString();
                }
                OnPropertyChanged();
            }
        }

        public string ProductName
        {
            get => _productName;
            set
            {
                _productName = value;
                OnPropertyChanged();
            }
        }

        public string ProductPrice
        {
            get => _productPrice;
            set
            {
                _productPrice = value;
                OnPropertyChanged();
            }
        }

        public string ProductQuantity
        {
            get => _productQuantity;
            set
            {
                _productQuantity = value;
                OnPropertyChanged();
            }
        }

        public string QuickAddQuantity
        {
            get => _quickAddQuantity;
            set
            {
                _quickAddQuantity = value;
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

        public ICommand AddProductCommand { get; }
        public ICommand UpdateProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand GoToCartCommand { get; }
        public ICommand QuickAddToCartCommand { get; }
        public ICommand ConfigureTaxCommand { get; }

        public InventoryViewModel()
        {
            _productService = ProductServiceProxy.Current;
            _cartService = new ShoppingCartService();
            _selectedProduct = null;
            _productName = string.Empty;
            _productPrice = string.Empty;
            _productQuantity = string.Empty;

            AddProductCommand = new Command(AddProduct);
            UpdateProductCommand = new Command(UpdateProduct);
            DeleteProductCommand = new Command(DeleteProduct);
            GoToCartCommand = new Command(async () => await Shell.Current.GoToAsync("//cart"));
            QuickAddToCartCommand = new Command<Product>(QuickAddToCart);
            ConfigureTaxCommand = new Command(async () => await Shell.Current.GoToAsync("//settings"));
        }

        private void AddProduct()
        {
            if (string.IsNullOrWhiteSpace(ProductName))
                return;

            if (!decimal.TryParse(ProductPrice, out decimal price))
                return;

            if (!int.TryParse(ProductQuantity, out int quantity))
                return;

            var newProduct = new Product
            {
                Name = ProductName,
                Price = price,
                Quantity = quantity
            };

            _productService.AddOrUpdate(newProduct);

            ProductName = string.Empty;
            ProductPrice = string.Empty;
            ProductQuantity = string.Empty;

            Application.Current.Dispatcher.Dispatch(() => {
                RefreshProductList();
            });
        }

        private void UpdateProduct()
        {
            if (SelectedProduct == null)
                return;

            if (!decimal.TryParse(ProductPrice, out decimal price))
                return;

            if (!int.TryParse(ProductQuantity, out int quantity))
                return;

            SelectedProduct.Name = ProductName;
            SelectedProduct.Price = price;
            SelectedProduct.Quantity = quantity;

            _productService.AddOrUpdate(SelectedProduct);

            SelectedProduct = null;
            ProductName = string.Empty;
            ProductPrice = string.Empty;
            ProductQuantity = string.Empty;

            Application.Current.Dispatcher.Dispatch(() => {
                RefreshProductList();
            });
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null)
                return;

            _productService.Delete(SelectedProduct.Id);

            SelectedProduct = null;
            ProductName = string.Empty;
            ProductPrice = string.Empty;
            ProductQuantity = string.Empty;
        }

        private void RefreshProductList()
        {
            OnPropertyChanged(nameof(Products));
            OnPropertyChanged(nameof(SortedProducts));
        }

        private void QuickAddToCart(Product product)
        {
            if (product == null)
                return;

            int quantity = product.TempQuantity;

            if (quantity <= 0)
                quantity = 1;
            _cartService.AddToCart(product.Id, quantity);

            UpdateUIAfterCartChange();

            product.TempQuantity = 1;
        }


        private void UpdateUIAfterCartChange()
        {

            OnPropertyChanged(nameof(Products));
            OnPropertyChanged(nameof(SortedProducts));
            Application.Current.Dispatcher.Dispatch(() => {
                MessagingCenter.Send(this, "CartUpdated");
            });
        }


        public IEnumerable<Product> SortedProducts =>
            _productService.SortProducts(_sortOption, _sortDescending);

        private void ApplySorting()
        {
            OnPropertyChanged(nameof(SortedProducts));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}