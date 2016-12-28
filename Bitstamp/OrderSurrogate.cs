////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.Serialization;

    using static System.Globalization.CultureInfo;

    /// <summary>Represents a client for the Bitstamp API.</summary>
    public sealed partial class BitstampClient
    {
        private sealed class OrderSurrogate : IDataContractSurrogate
        {
            public Type GetDataContractType(Type type) => type == typeof(Order) ? typeof(string[]) : type;

            public object GetDeserializedObject(object obj, Type targetType)
            {
                var array = obj as string[];
                return array == null ? obj : new Order(Parse(array[0]), Parse(array[1]));
            }

            public object GetObjectToSerialize(object obj, Type targetType)
            {
                var bidAsk = obj as Order;
                return bidAsk == null ? obj : new string[] { ToString(bidAsk.Price), ToString(bidAsk.Amount) };
            }

            public object GetCustomDataToExport(Type clrType, Type dataContractType) => null;

            public object GetCustomDataToExport(MemberInfo memberInfo, Type dataContractType) => null;

            public void GetKnownCustomDataTypes(Collection<Type> customDataTypes)
            {
            }

            public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData) => null;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private static decimal Parse(string value) => decimal.Parse(value, NumberStyles.Float, InvariantCulture);

            private static string ToString(decimal value) => value.ToString(InvariantCulture);
        }
    }
}
