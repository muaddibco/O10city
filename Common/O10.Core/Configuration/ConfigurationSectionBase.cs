using System;
using System.Reflection;
using System.ComponentModel;
using O10.Core.Exceptions;
using System.Collections.Generic;

namespace O10.Core.Configuration
{
	public class ConfigurationSectionBase : IConfigurationSection
    {
        private bool _isInitialized;
        private readonly object _sync = new object();

		public ConfigurationSectionBase(IAppConfig appConfig, string sectionName)
        {
            SectionName = sectionName;

            AppConfig = appConfig;
        }

		public string SectionName { get; }

        public IAppConfig AppConfig { get; }

        public void Initialize()
        {
            if (!_isInitialized)
            {
                lock (_sync)
                {
                    if (!_isInitialized)
                    {
                        _isInitialized = true;

                        foreach (PropertyInfo propertyInfo in GetPropertyInfos())
                        {
                            SetPropertyValue(propertyInfo);
                        }
                    }
                }
            }
        }

        private IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            Type type = GetType();

            while (!type.Equals(typeof(ConfigurationSectionBase)))
            {
                var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var propertyInfo in propertyInfos)
                {
                    yield return propertyInfo;
                }
                type = type.BaseType;
            }
        }

        private void SetPropertyValue(PropertyInfo propertyInfo)
        {
            string propertyName = propertyInfo.Name;

            bool isOptional = IsOptional(propertyInfo);
            bool isTokenized = IsTokenized(propertyInfo);

            string key = string.IsNullOrWhiteSpace(SectionName) ? propertyName : $"{SectionName}:{propertyName}";

            //string sValue = _appConfig.GetString(key.ToLower(), !_isOptional);
            string sValue = AppConfig.GetString(key, !isOptional);
            object value;
            if (propertyInfo.PropertyType.IsArray)
            {
                Array values;

                if (string.IsNullOrEmpty(sValue) && isOptional)
                {
                    values = Array.CreateInstance(propertyInfo.PropertyType.GetElementType(), 0);
                }
                else
                {
                    string[] arrValues = sValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    values = Array.CreateInstance(propertyInfo.PropertyType.GetElementType(), arrValues.Length);
                    object arrValue;
                    int index = 0;

                    foreach (string arrSValue in arrValues)
                    {
                        var val = arrSValue;
                        if(isTokenized && !string.IsNullOrEmpty(arrSValue))
                        {
                            val = AppConfig.ReplaceToken(arrSValue);
                        }

                        if (TryConvertSingleValue(propertyInfo.PropertyType.GetElementType(), val.Trim(), out arrValue, isOptional))
                        {
                            values.SetValue(arrValue, index++);
                        }
                        else
                        {
                            throw new ConfigurationParameterValueConversionFailedException(sValue, key, propertyInfo.PropertyType, propertyName, GetType());
                        }
                    }
                }

                value = values;
            }
            else
            {
                var val = sValue;
                if (isTokenized && !string.IsNullOrEmpty(sValue))
                {
                    val = AppConfig.ReplaceToken(sValue);
                }

                if (!TryConvertSingleValue(propertyInfo.PropertyType, val, out value, isOptional))
                {
                    throw new ConfigurationParameterValueConversionFailedException(sValue, key, propertyInfo.PropertyType, propertyName, GetType());
                }
            }

            propertyInfo.SetValue(this, value);
        }

        private static bool IsOptional(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo?.GetCustomAttributes(typeof(OptionalAttribute), true);
            bool isOptional = (attrs?.Length ?? 0) > 0;
            return isOptional;
        }

        private static bool IsTokenized(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo?.GetCustomAttributes(typeof(TokenizedAttribute), true);
            bool isTokenized = (attrs?.Length ?? 0) > 0;
            return isTokenized;
        }

        private bool TryConvertSingleValue(Type targetType, string sValue, out object value, bool isOptional)
        {
            value = null;
            TypeConverter tcFrom = TypeDescriptor.GetConverter(targetType);
            if (!tcFrom.CanConvertFrom(typeof(string)))
            {
                TypeConverter tcTo = TypeDescriptor.GetConverter(typeof(string));

                if (!tcTo.CanConvertTo(targetType))
                {
                    return false;
                }

                try
                {
                    value = tcTo.ConvertTo(sValue, targetType);
                }
                catch
                {
                    if (isOptional)
                    {
                        if (targetType.IsValueType)
                        {
                            value = Activator.CreateInstance(targetType);
                        }
                        else
                        {
                            value = null;
                        }

                        return true;
                    }

                    return false;
                }
            }
            else
            {
                try
                {
                    value = tcFrom.ConvertFromString(sValue);
                }
                catch
                {
                    if (isOptional)
                    {
                        if (targetType.IsValueType)
                        {
                            value = Activator.CreateInstance(targetType);
                        }
                        else
                        {
                            value = null;
                        }

                        return true;
                    }

                    return false;
                }
            }

            return true;
        }
    }
}
