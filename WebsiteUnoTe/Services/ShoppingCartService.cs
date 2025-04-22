using WebsiteUnoTe.Models;

namespace WebsiteUnoTe.Services
{
    public class ShoppingCartService
    {
        private readonly ProductServiceProxy _inventory;
        private readonly CartManager _cartManager;

        public ShoppingCartService()
        {
            _inventory = ProductServiceProxy.Current;
            _cartManager = CartManager.Current;
        }

        public ShoppingCartService(ProductServiceProxy inventory, CartManager cartManager)
        {
            _inventory = inventory;
            _cartManager = cartManager;
        }

        public void AddToCart(int productId, int quantity, ShoppingCart cart = null)
        {
            //pls work
            cart ??= _cartManager.ActiveCart;

            var product = _inventory.GetById(productId);
            if (product != null && product.Quantity >= quantity && quantity > 0)
            {
                cart.AddToCart(product, quantity);

                if (!cart.IsWishlist)
                {
                    product.Quantity -= quantity;
                    _inventory.AddOrUpdate(product);
                }
                NotifyProductChanges();
            }
        }

        private void NotifyProductChanges()
        {
            MessagingCenter.Send(this, "DataChanged");
        }

        public void UpdateCartItem(int productId, int quantity, ShoppingCart cart = null)
        {
            cart ??= _cartManager.ActiveCart;

            var cartItem = cart.CartItems.FirstOrDefault(p => p.Id == productId);
            var inventoryItem = _inventory.GetById(productId);

            if (cartItem != null && inventoryItem != null)
            {
                if (!cart.IsWishlist)
                {
                    int quantityDifference = quantity - cartItem.Quantity;
                    if (quantityDifference > 0 && quantityDifference > inventoryItem.Quantity)
                        return;

                    inventoryItem.Quantity += cartItem.Quantity;
                }

                if (quantity <= 0)
                {
                    cart.RemoveFromCart(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                    if (!cart.IsWishlist)
                    {
                        inventoryItem.Quantity -= quantity;
                    }
                }
            }
        }

        public void RemoveFromCart(int productId, ShoppingCart cart = null)
        {
            cart ??= _cartManager.ActiveCart;

            var cartItem = cart.CartItems.FirstOrDefault(p => p.Id == productId);
            var inventoryItem = _inventory.GetById(productId);

            if (cartItem != null && inventoryItem != null)
            {
                if (!cart.IsWishlist)
                {
                    inventoryItem.Quantity += cartItem.Quantity;
                }
                cart.RemoveFromCart(cartItem);
            }
        }

        public void MoveItemToCart(int productId, ShoppingCart sourceCart, ShoppingCart targetCart)
        {
            var cartItem = sourceCart.CartItems.FirstOrDefault(p => p.Id == productId);
            if (cartItem != null)
            {
                var inventoryItem = _inventory.GetById(productId);
                if (inventoryItem != null)
                {
                    int quantityToMove = cartItem.Quantity;
                    if (sourceCart.IsWishlist && !targetCart.IsWishlist)
                    {
                        quantityToMove = Math.Min(quantityToMove, inventoryItem.Quantity);
                        if (quantityToMove <= 0)
                            return;

                        inventoryItem.Quantity -= quantityToMove;
                    }
                    else if (!sourceCart.IsWishlist && targetCart.IsWishlist)
                    {
                        inventoryItem.Quantity += quantityToMove;
                    }

                    targetCart.AddToCart(new Product
                    {
                        Id = cartItem.Id,
                        Name = cartItem.Name,
                        Price = cartItem.Price,
                        Quantity = quantityToMove
                    }, quantityToMove);

                    sourceCart.RemoveFromCart(cartItem);
                }
            }
        }

        public void SetActiveCart(int cartId)
        {
            var cart = _cartManager.Carts.FirstOrDefault(c => c.Id == cartId);
            if (cart != null)
            {
                _cartManager.ActiveCart = cart;
            }
        }

        public ShoppingCart CreateNewCart(string name, bool isWishlist = false)
        {
            return _cartManager.CreateNewCart(name, isWishlist);
        }

        public bool DeleteCart(int cartId)
        {
            return _cartManager.DeleteCart(cartId);
        }

        public void SetTaxRate(decimal taxRate)
        {
            _cartManager.TaxRate = taxRate;
        }

        public decimal GetCartSubtotal(ShoppingCart cart = null)
        {
            cart ??= _cartManager.ActiveCart;
            return cart.GetTotal();
        }

        public decimal GetCartTax(ShoppingCart cart = null)
        {
            cart ??= _cartManager.ActiveCart;
            var rate = AppSettings.Current.TaxRate;
            return cart.GetTotal() * rate;
        }
        public decimal GetCartTotal(ShoppingCart cart = null)
        {
            cart ??= _cartManager.ActiveCart;
            return cart.GetTotalWithTax(_cartManager.TaxRate);
        }

        public void ClearCart(ShoppingCart cart = null)
        {
            cart ??= _cartManager.ActiveCart;

            if (!cart.IsWishlist)
            {
                foreach (var item in cart.CartItems.ToList())
                {
                    RemoveFromCart(item.Id, cart);
                }
            }
            else
            {
                cart.CartItems.Clear();
            }
        }
    }
}