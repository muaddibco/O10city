﻿using System;
using O10.Core.Properties;

namespace O10.Core.Exceptions
{

    [Serializable]
    public class ConfigurationParameterValueConversionFailedException : Exception
    {
        public ConfigurationParameterValueConversionFailedException() { }
        public ConfigurationParameterValueConversionFailedException(string value, string parameterName, Type targetType, string propertyName, Type declaringType) : base(string.Format(Resources.ERR_APPLICATION_CONFIGURATION_CONVERSION_FAILED, value, parameterName, targetType.FullName, propertyName, declaringType.FullName)) { }
        public ConfigurationParameterValueConversionFailedException(string value, string parameterName, Type targetType, string propertyName, Type declaringType, Exception inner) : base(string.Format(Resources.ERR_APPLICATION_CONFIGURATION_CONVERSION_FAILED, value, parameterName, targetType.FullName, propertyName, declaringType.FullName), inner) { }
        protected ConfigurationParameterValueConversionFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
