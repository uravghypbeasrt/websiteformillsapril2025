using System.Globalization;
//redunbdant doesnt work
namespace WebsiteUnoTe.Converters
{
    public class ItemTotalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal price && parameter is string quantityStr)
            {
                if (int.TryParse(quantityStr, out int quantity))
                {
                    return price * quantity;
                }
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}