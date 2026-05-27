using System.ComponentModel;

namespace PostIQ.Core.Database.Helpers
{
    public static class ConverterHelper
    {
        public static object ConvertStringToType(string value, Type targetType)
        {
            // Check if targetType is a nullable type
            bool isNullable = targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);

            // Get the underlying type if it's nullable
            Type nonNullableTargetType = isNullable ? Nullable.GetUnderlyingType(targetType) : targetType;

            // Handle null or empty values
            if (string.IsNullOrEmpty(value))
            {
                // Default to zero for numeric types
                if (IsNumericType(nonNullableTargetType))
                {
                    return Activator.CreateInstance(nonNullableTargetType);
                }

                // Return null for nullable types or default value for non-nullable types
                return isNullable ? null : Activator.CreateInstance(nonNullableTargetType);
            }

            // Use TypeDescriptor to perform the conversion
            var converter = TypeDescriptor.GetConverter(nonNullableTargetType);
            if (converter != null && converter.CanConvertFrom(typeof(string)))
            {
                return converter.ConvertFrom(value);
            }

            throw new InvalidOperationException($"Cannot convert from string to {targetType.Name}");
        }

        public static Array ConvertStringArrayToTypeArray(string[] values, Type targetType)
        {
            // Determine if the target type is nullable
            bool isNullable = targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
            Type nonNullableTargetType = isNullable ? Nullable.GetUnderlyingType(targetType) : targetType;

            // Create an array of the target type
            Array resultArray = Array.CreateInstance(targetType, values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                string value = values[i];
                object convertedValue = ConvertStringToType(value, nonNullableTargetType);
                resultArray.SetValue(convertedValue, i);
            }

            return resultArray;
        }


        // Helper method to check if a type is numeric
        private static bool IsNumericType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }
    }
}
