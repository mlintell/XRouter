﻿using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace ObjectConfigurator.ItemTypes
{
    [DataContract]
    class BasicItemType : ItemType
    {
        internal static readonly Type[] BasicClrTypes = new[] { 
            typeof(SByte), typeof(Int16), typeof(Int32), typeof(Int64), 
            typeof(Byte), typeof(UInt16), typeof(UInt32), typeof(UInt64),
            typeof(Single), typeof(Double),
            typeof(Decimal), typeof(Boolean), typeof(String)
        };

        public BasicItemType(Type clrType)
            : base(clrType)
        {
        }

        public override void WriteToXElement(XElement target, object value)
        {
            string content = ValueToString(value);
            if (content != null) {
                target.SetValue(content);
            }
        }

        public override object ReadFromXElement(XElement source)
        {
            if (source.Value == null) {
                return null;
            }
            object result = ParseValue(source.Value);
            return result;
        }

        public override void WriteDefaultValueToXElement(XElement target)
        {
            object defaultValue = CreateInstance();
            WriteToXElement(target, defaultValue);
        }

        public string ValueToString(object value)
        {
            if (value == null) {
                return null;
            }

            Type clrType = GetClrType();
            string result;
            if (clrType == typeof(string)) {
                result = (string)value;
            } else if (value is IFormattable) {
                result = ((IFormattable)value).ToString(null, CultureInfo.InvariantCulture);
            } else {
                result = value.ToString();
            }
            return result;
        }

        public object ParseValue(string str)
        {
            Type clrType = GetClrType();
            object result;
            if (clrType == typeof(string)) {
                result = str;
            } else if (clrType == typeof(bool)) {
                MethodInfo parseMethod = clrType.GetMethod("Parse", new Type[] { typeof(string) });
                result = parseMethod.Invoke(null, new object[] { str });
            } else {
                MethodInfo parseMethod = clrType.GetMethod("Parse", new Type[] { typeof(string), typeof(CultureInfo) });
                result = parseMethod.Invoke(null, new object[] { str, CultureInfo.InvariantCulture });
            }
            return result;
        }
    }
}
