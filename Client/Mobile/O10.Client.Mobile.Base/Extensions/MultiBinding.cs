using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using O10.Client.Mobile.Base.Converters;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace O10.Client.Mobile.Base.Extensions
{
    [ContentProperty(nameof(Bindings))]
    public class MultiBinding : IMarkupExtension<Binding>
    {
        private BindableObject _target;
        private readonly InternalValue _internalValue = new InternalValue();
        private readonly IList<BindableProperty> _properties = new List<BindableProperty>();

        public IList<Binding> Bindings { get; } = new List<Binding>();

        public string StringFormat { get; set; }

        public IMultiValueConverter Converter { get; set; }

        public object ConverterParameter { get; set; }

        public Binding ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(StringFormat) && Converter == null)
                throw new InvalidOperationException($"{nameof(MultiBinding)} requires a {nameof(Converter)} or {nameof(StringFormat)}");

            //Get the object that the markup extension is being applied to
            var provideValueTarget = (IProvideValueTarget)serviceProvider?.GetService(typeof(IProvideValueTarget));
            _target = provideValueTarget?.TargetObject as BindableObject;

            if (_target == null)
                return null;

            foreach (Binding b in Bindings)
            {
                var property = BindableProperty.Create($"Property-{Guid.NewGuid().ToString("N")}", typeof(object),
                    typeof(MultiBinding), default(object), propertyChanged: (_, o, n) => SetValue());
                _properties.Add(property);
                _target.SetBinding(property, b);
            }
            SetValue();

            var binding = new Binding
            {
                Path = nameof(InternalValue.Value),
                Converter = new MultiValueConverterWrapper(Converter, StringFormat),
                ConverterParameter = ConverterParameter,
                Source = _internalValue
            };

            return binding;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValue(serviceProvider);
        }

        private void SetValue()
        {
            if (_target == null)
                return;
            _internalValue.Value = _properties.Select(_target.GetValue).ToArray();
        }

        private sealed class InternalValue : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private object _value;
            public object Value
            {
                get { return _value; }
                set
                {
                    if (!Equals(_value, value))
                    {
                        _value = value;
                        OnPropertyChanged();
                    }
                }
            }

            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private sealed class MultiValueConverterWrapper : IValueConverter
        {
            private readonly IMultiValueConverter _multiValueConverter;
            private readonly string _stringFormat;

            public MultiValueConverterWrapper(IMultiValueConverter multiValueConverter, string stringFormat)
            {
                _multiValueConverter = multiValueConverter;
                _stringFormat = stringFormat;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (_multiValueConverter != null)
                {
                    value = _multiValueConverter.Convert(value as object[], targetType, parameter, culture);
                }
                if (!string.IsNullOrWhiteSpace(_stringFormat))
                {
                    var array = value as object[];
                    // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                    if (array != null)
                    {
                        value = string.Format(_stringFormat, array);
                    }
                    else
                    {
                        value = string.Format(_stringFormat, value);
                    }
                }
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
