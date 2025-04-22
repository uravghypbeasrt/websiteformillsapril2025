using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WebsiteUnoTe.Services;

namespace WebsiteUnoTe.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly AppSettings _appSettings;
        private string _taxRateText;

        public string TaxRateText
        {
            get => _taxRateText;
            set
            {
                _taxRateText = value;
                OnPropertyChanged();
            }
        }

        public string TaxRateDisplay => $"{_appSettings.TaxRate:P0}";

        public ICommand SaveSettingsCommand { get; }
        public ICommand GoBackCommand { get; }

        public SettingsViewModel()
        {
            _appSettings = AppSettings.Current;
            _taxRateText = (_appSettings.TaxRate * 100).ToString("F2");

            SaveSettingsCommand = new Command(SaveSettings);
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync("//inventory"));
        }

        private void SaveSettings()
        {
            if (decimal.TryParse(_taxRateText, out decimal taxRate))
            {
                _appSettings.TaxRate = taxRate / 100m;
                OnPropertyChanged(nameof(TaxRateDisplay));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}