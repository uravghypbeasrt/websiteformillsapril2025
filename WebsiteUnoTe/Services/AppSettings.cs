using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Storage;

namespace WebsiteUnoTe.Services
{
    public class AppSettings : INotifyPropertyChanged
    {
        private static AppSettings _instance;
        private static readonly object _instanceLock = new object();
        private decimal _taxRate = 0.07m;

        public decimal TaxRate
        {
            get => _taxRate;
            set
            {
                if (_taxRate != value && value >= 0 && value <= 1)
                {
                    _taxRate = value;
                    OnPropertyChanged();
                    SaveSettings();
                    MessagingCenter.Send(this, "TaxRateChanged");
                }
            }
        }

        private AppSettings()
        {
            LoadSettings();
        }

        public static AppSettings Current
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new AppSettings();
                    }
                }
                return _instance;
            }
        }

        private void LoadSettings()
        {
            var taxRateString = Preferences.Get("TaxRate", "0.07");
            if (decimal.TryParse(taxRateString, out var taxRate))
            {
                _taxRate = taxRate;
            }
            else
            {
                _taxRate = 0.07m;
            }
        }

        private void SaveSettings()
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