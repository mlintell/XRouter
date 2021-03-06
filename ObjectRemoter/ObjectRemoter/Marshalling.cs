﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Globalization;

namespace ObjectRemoter
{
    /// <summary>
    /// Internal support for marshalling objects (ie. serialization).
    /// </summary>
    internal class Marshalling
    {
        private static readonly string ArrayElementSeparator = "[5g#v&4k_x]";

        /// <summary>
        /// Converts an object of a given formal type to string
        /// representation which can be unmarshalled (converted back to
        /// object) on a target computer.
        /// </summary>
        /// <remarks>
        /// It supports objects of following (informal) categories/types:
        /// primitive, string, IRemotelyCloneable, IRemotelyReferable,
        /// delegate, System.Void, [Serializable] and array. A null object also
        /// can be marshalled.
        /// </remarks>
        /// <param name="obj">An object to be marshalled. Can be null.
        /// </param>
        /// <param name="type">Formal type of the object (only this type will
        /// be available on a target computer). Must not be null.</param>
        /// <returns>Marshalled string representation of given object.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException">If the object type is not
        /// supported for marshalling.</exception>
        public static string Marshal(object obj, Type type)
        {
            // Parameter 'type' must not be null by contract.
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (type == typeof(object))
            {
                type = DetermineFormalTypeFromObject(obj);
            }

            Func<Type, string, string> composeResultFromFormalTypeAndData = delegate(Type formalType, string data)
            {
                return formalType.Assembly.FullName + "!" + formalType.FullName + ":" + data;
            };

            if (type.IsPrimitive)
            {
                string result;
                if (obj is IFormattable)
                {
                    result = ((IFormattable)obj).ToString(null, CultureInfo.InvariantCulture);
                }
                else
                {
                    result = obj.ToString();
                }
                return composeResultFromFormalTypeAndData(type, result);
            }

            if ((type.FullName == "System.Void") || (obj == null))
            {
                return composeResultFromFormalTypeAndData(Type.GetType("System.Void"), string.Empty);
            }

            if (type == typeof(string))
            {
                return composeResultFromFormalTypeAndData(typeof(string), (string)obj);
            }

            if (typeof(IRemotelyCloneable).IsAssignableFrom(type))
            {
                var clonable = (IRemotelyCloneable)obj;
                string result = clonable.SerializeClone();
                return composeResultFromFormalTypeAndData(type, result);
            }

            if (typeof(IRemotelyReferable).IsAssignableFrom(type))
            {
                if (!type.IsInterface)
                {
                    throw new ArgumentException("The formal type of the remotely referable object must be an interface.");
                }

                var remotelyReferable = (IRemotelyReferable)obj;
                var address = ObjectServer.PublishObject(remotelyReferable);
                string result = address.Serialize();
                return composeResultFromFormalTypeAndData(type, result);
            }

            if (typeof(Delegate).IsAssignableFrom(type))
            {
                var target = (Delegate)obj;
                var invocable = new Invocable(target);
                var address = ObjectServer.PublishObject(invocable);
                string result = address.Serialize();
                return composeResultFromFormalTypeAndData(type, result);
            }

            if (type.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(ms, obj);
                    byte[] bytes = ms.GetBuffer();
                    int length = (int)ms.Length;
                    string result = length.ToString() + ":" + Convert.ToBase64String(bytes, 0, length);
                    return composeResultFromFormalTypeAndData(type, result);
                }
            }

            if (type.IsArray)
            {
                Array array = (Array)obj;
                if (array.Rank == 1)
                {
                    Type elementType = type.GetElementType();
                    StringBuilder result = new StringBuilder();
                    for (int i = 0; i < array.Length; i++)
                    {
                        object element = array.GetValue(i);
                        Type elementFormalType = DetermineFormalTypeFromObject(element);

                        result.Append(elementFormalType.ToString());
                        result.Append(ArrayElementSeparator);
                        result.Append(Marshal(element, elementType));
                        if (i < array.Length - 1)
                        {
                            result.Append(ArrayElementSeparator);
                        }
                    }

                    return composeResultFromFormalTypeAndData(type, result.ToString());
                }
            }

            throw new ArgumentException("Cannot marshal given type.");
        }

        // TODO:
        // - return value type of Unmarshal() could be specified in a type parameter:
        //   static T Unmarshal<T>(string marshalled, Type type)
        // - This TODO should be removed because caller does not know type T at compile type and so it would be still always just Object

        /// <summary>
        /// Creates an object of a specified type from its marshalled string
        /// representation.
        /// </summary>
        /// <param name="marshalled">Marshalled object represented as a string.
        /// Must not be null.
        /// </param>
        /// <param name="requestedType">Type of the object to be unmarshalled.
        /// Must not be null.</param>
        /// <returns>The unmarshalled object.</returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException">If the object type is not
        /// supported for unmarshalling. See the Marshal method for supported
        /// object types and categories.
        /// </exception>
        /// <see cref="Marshal(object,Type)"/>
        public static object Unmarshal(string marshalled, Type requestedType)
        {
            if (marshalled == null)
            {
                throw new ArgumentNullException("marshalled");
            }

            if (requestedType == null)
            {
                throw new ArgumentNullException("requestedType");
            }

            // working type - can be either set the requested type or to the
            // type stored in the marshalled representation
            Type type = requestedType;

            #region Extract typeAndAssemblyFullName from parameter marshalled
            int formalTypeEndPos = marshalled.IndexOf(':');
            if (formalTypeEndPos < 0)
            {
                throw new ArgumentException("Cannot find formal type specification.", "marshalled");
            }
            string typeAndAssemblyFullName = marshalled.Substring(0, formalTypeEndPos);
            if (formalTypeEndPos + 1 < marshalled.Length)
            {
                marshalled = marshalled.Substring(formalTypeEndPos + 1);
            }
            else
            {
                marshalled = string.Empty;
            }
            #endregion

            // type stored in the marshalled representation
            Type storedType = null;
            if (type is object)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                storedType = TypeExtensions.GetType(assemblies, typeAndAssemblyFullName);
                type = storedType;
            }

            if (type.IsPrimitive)
            {
                // TODO: will this overload Parse(string) work for all primitives?
                // See ticket #29.
                object result;
                if (type == typeof(bool))
                {
                    MethodInfo parseMethod = type.GetMethod("Parse", new Type[] { typeof(string) });
                    result = parseMethod.Invoke(null, new object[] { marshalled });
                }
                else
                {
                    MethodInfo parseMethod = type.GetMethod("Parse", new Type[] { typeof(string), typeof(CultureInfo) });
                    result = parseMethod.Invoke(null, new object[] { marshalled, CultureInfo.InvariantCulture });
                }
                return result;
            }

            if ((type.FullName == "System.Void") || ((type == typeof(object)) && (marshalled.Length == 0)))
            {
                return null;
            }

            if (type == typeof(string))
            {
                return marshalled;
            }

            if (typeof(IRemotelyCloneable).IsAssignableFrom(type))
            {
                // TODO:
                // If one of the types (stored or requested) is not abstract
                // and the other is, the non-abstract type could be
                // instantiated instead of throwing an exception.
                // The behavior should be analyzed better.
                // It is not straighforward if an exception should be
                // thrown or the type should be automatically corrected
                // (ie. taken from the saved real type in unmarshalling).
                if (type.IsAbstract)
                {
                    throw new ArgumentException(
                        string.Format("Cannot instantiate an abstract type: {0}", type));
                }
                var result = (IRemotelyCloneable)FormatterServices.GetUninitializedObject(type);
                result.DeserializeClone(marshalled);
                return result;
            }

            if (typeof(IRemotelyReferable).IsAssignableFrom(type))
            {
                var address = RemoteObjectAddress.Deserialize(marshalled);
                var result = RemoteObjectProxyProvider.GetProxy<IRemotelyReferable>(address, type);
                return result;
            }

            if (typeof(Delegate).IsAssignableFrom(type))
            {
                var address = RemoteObjectAddress.Deserialize(marshalled);
                IInvocable invocable = RemoteObjectProxyProvider.GetProxy<IInvocable>(address);

                Delegate result = CreateDynamicDelegate(type,
                    delegate(object[] arguments)
                    {
                        object res = invocable.Invoke(arguments);
                        return res;
                    });
                return result;
            }

            if (type.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0)
            {
                int colonPos = marshalled.IndexOf(':');
                if (colonPos < 0)
                {
                    throw new ArgumentException("Missing object contents length.", "marshalled");
                }
                int length = int.Parse(marshalled.Substring(0, colonPos));
                string base64 = marshalled.Substring(colonPos + 1);
                if (string.IsNullOrEmpty(base64))
                {
                    throw new ArgumentException("Missing object contents.", "marshalled");
                }
                byte[] bytes;
                try
                {
                    bytes = Convert.FromBase64String(base64);
                }
                catch (FormatException ex)
                {
                    throw new ArgumentException("Bad object contents.", "marshalled", ex);
                }
                if (length != bytes.Length)
                {
                    throw new ArgumentException("Bad object contents length.", "marshalled");
                }
                try
                {
                    using (MemoryStream ms = new MemoryStream(bytes, 0, length))
                    {
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        object result = binaryFormatter.Deserialize(ms);
                        return result;
                    }
                }
                catch (SerializationException ex)
                {
                    throw new ArgumentException("Bad object contents.", "marshalled", ex);
                }
                catch (System.Security.SecurityException ex)
                {
                    // TODO: no test coverage
                    throw new ArgumentException("Bad object contents.", "marshalled", ex);
                }
            }

            if (type.IsArray)
            {
                string[] marshalledElements = marshalled.Split(new[] { ArrayElementSeparator }, StringSplitOptions.None);
                object[] result = new object[marshalledElements.Length / 2];
                for (int i = 0; i < result.Length; i++)
                {
                    Type formalType = Type.GetType(marshalledElements[(i * 2) + 0]);
                    string marshalledElement = marshalledElements[(i * 2) + 1];
                    result[i] = Unmarshal(marshalledElement, formalType);
                }

                return result;
            }

            throw new ArgumentException("Cannot unmarshal given type.");
        }

        private static Type DetermineFormalTypeFromObject(object obj)
        {
            Type result = typeof(object);
            if (obj != null)
            {
                Type realType = obj.GetType();
                if (typeof(IRemotelyReferable).IsAssignableFrom(realType))
                {
                    result = typeof(IRemotelyReferable);
                }
                else if (typeof(IRemotelyCloneable).IsAssignableFrom(realType))
                {
                    result = typeof(IRemotelyCloneable);
                }
                else if (realType.IsPrimitive || (realType == typeof(string)))
                {
                    result = realType;
                }
                else if (typeof(Delegate).IsAssignableFrom(realType))
                {
                    result = realType;
                }
            }

            return result;
        }

        #region Reflection support

        internal static ConcurrentDictionary<int, Func<object[], object>> delegatesBodies = new ConcurrentDictionary<int, Func<object[], object>>();

        private static Delegate CreateDynamicDelegate(Type delegateType, Func<object[], object> body)
        {
            int delegateTargetID = delegatesBodies.Count + 1;
            while (!delegatesBodies.TryAdd(delegateTargetID, body))
            {
                // TODO: no test coverage
                delegateTargetID++;
            }

            MethodInfo signature = delegateType.GetMethod("Invoke");
            Type[] parameterTypes = signature.GetParameters().Select(p => p.ParameterType).ToArray();
            DynamicMethod dynamicMethod = new DynamicMethod("DynamicMethod", signature.ReturnType, parameterTypes, DynamicMethod.GetCurrentMethod().Module);

            ILGenerator il = dynamicMethod.GetILGenerator();

            // Push delegate body:
            //   delegatesBodies.GetOrAdd(delegateTargetID, null)
            FieldInfo delegatesField = typeof(Marshalling).GetField("delegatesBodies", BindingFlags.NonPublic | BindingFlags.Static);
            il.Emit(OpCodes.Ldsfld, delegatesField);
            il.Emit(OpCodes.Ldc_I4, delegateTargetID);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Callvirt, typeof(ConcurrentDictionary<int, Func<object[], object>>).GetMethod("GetOrAdd", new[] { typeof(int), typeof(Func<object[], object>) }));

            // Push arguments
            il.Emit(OpCodes.Ldc_I4, parameterTypes.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldarg, i);
                if (parameterTypes[i].IsValueType)
                {
                    // TODO: no test coverage
                    il.Emit(OpCodes.Box, parameterTypes[i]);
                }
                il.Emit(OpCodes.Stelem_Ref);
            }

            // Call body
            il.Emit(OpCodes.Callvirt, typeof(Func<object[], object>).GetMethod("Invoke"));

            if (signature.ReturnType.FullName == "System.Void")
            {
                il.Emit(OpCodes.Pop);
            }

            il.Emit(OpCodes.Ret);

            Delegate result = dynamicMethod.CreateDelegate(delegateType);
            return result;
        }

        #endregion

        private class Invocable : IInvocable
        {
            private Delegate target;

            public Invocable(Delegate target)
            {
                this.target = target;
            }

            public object Invoke(object[] arguments)
            {
                object result = target.DynamicInvoke(arguments);
                return result;
            }
        }
    }
}
