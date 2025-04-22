using System.Collections.ObjectModel;
using WebsiteUnoTe.Models;

namespace WebsiteUnoTe.Services
{
    public class ProductServiceProxy
    {
        private ProductServiceProxy()
        {
            Products = new ObservableCollection<Product>();
            // test
            AddOrUpdate(new Product { Name = "laptop ", Price = 999.99m, Quantity = 10 });
            AddOrUpdate(new Product { Name = "phone", Price = 699.99m, Quantity = 15 });
            AddOrUpdate(new Product { Name = "headpgones", Price = 149.99m, Quantity = 20 });
        }

        private int lastKey
        {
            get
            {
                if (!Products.Any())
                {
                    return 0;
                }
                return Products.Select(p => p.Id).Max();
            }
        }

        private static ProductServiceProxy _instance;
        private static readonly object _instanceLock = new object();

        public static ProductServiceProxy Current
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new ProductServiceProxy();
                    }
                }
                return _instance;
            }
        }

        public ObservableCollection<Product> Products { get; private set; }

        public Product AddOrUpdate(Product product)
        {
            Console.WriteLine($"ProductServiceProxy.AddOrUpdate called with product: {product.Name}");

            if (product.Id == 0)
            {
                product.Id = lastKey + 1;
                Console.WriteLine($"Assigned new ID: {product.Id}");
                Products.Add(product);
                Console.WriteLine($"Product added, new count: {Products.Count}");
            }
            else
            {
                var existing = Products.FirstOrDefault(p => p.Id == product.Id);
                if (existing != null)
                {
                    Console.WriteLine($"Updating existing product ID: {existing.Id}");
                    existing.Name = product.Name;
                    existing.Price = product.Price;
                    existing.Quantity = product.Quantity;
                }
                else
                {
                    Console.WriteLine($"Product with ID {product.Id} not found for update");
                }
            }
            return product;
        }

        public Product GetById(int id)
        {
            return Products.FirstOrDefault(p => p.Id == id);
        }

        public Product Delete(int id)
        {
            if (id == 0)
            {
                return null;
            }
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                Products.Remove(product);
            }
            return product;
        }

        public IEnumerable<Product> SortProducts(int sortOption, bool descending)
        {
            return sortOption switch
            {
                0 => descending ? Products.OrderByDescending(p => p.Id) : Products.OrderBy(p => p.Id),
                1 => descending ? Products.OrderByDescending(p => p.Name) : Products.OrderBy(p => p.Name),
                2 => descending ? Products.OrderByDescending(p => p.Price) : Products.OrderBy(p => p.Price),
                _ => Products
            };
        }
    }
}