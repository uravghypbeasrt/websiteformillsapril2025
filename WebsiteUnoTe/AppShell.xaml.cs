using Microsoft.Maui.Controls;

namespace WebsiteUnoTe
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("//inventory", typeof(Views.InventoryPage));
            Routing.RegisterRoute("//cart", typeof(Views.CartPage));
            Routing.RegisterRoute("//receipt", typeof(Views.ReceiptPage));
            Routing.RegisterRoute("//settings", typeof(Views.SettingsPage));
        }
    }
}