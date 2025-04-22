namespace WebsiteUnoTe.Views
{
    public partial class ReceiptPage : ContentPage
    {
        public ReceiptPage()
        {
            InitializeComponent();

            BindingContext = new WebsiteUnoTe.ViewModels.ReceiptViewModel();
        }
    }

}