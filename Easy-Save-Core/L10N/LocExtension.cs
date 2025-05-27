using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using EasySaveCore.Models;

namespace CLEA.EasySaveCore.L10N
{
    [MarkupExtensionReturnType(typeof(string))]
    public class LocExtension : MarkupExtension, INotifyPropertyChanged
    {
        private string _key;

        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public string Value => L10N.Get().GetTranslation(Key);

        public LocExtension(string key)
        {
            _key = key;

            L10N.LanguageChanged -= OnLanguageChanged;
            L10N.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Value));
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding("Value")
            {
                Source = this,
                Mode = BindingMode.OneWay
            };

            return binding.ProvideValue(serviceProvider);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
    }
}
