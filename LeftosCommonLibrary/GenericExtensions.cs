#region Copyright Notice

//    Copyright 2011-2013 Eleftherios Aslanoglou
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

namespace LeftosCommonLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>Implements generic extension methods.</summary>
    public static class GenericExtensions
    {
        private static readonly Random Rng = new Random();

        /// <summary>
        ///     Tries to the change the value of specific DataRow entry by using the corresponding value of a dictionary entry, after converting
        ///     it to the specified <c>type</c>.
        /// </summary>
        /// <param name="row">The DataRow containing the entry of which we want to try and change the value</param>
        /// <param name="dict">The dictionary containing the value we're trying to set.</param>
        /// <param name="key">The key representing both the DataRow column as well as the Dictionary key.</param>
        /// <param name="type">The type to convert to.</param>
        public static void TryChangeValue(this DataRow row, Dictionary<string, string> dict, string key, Type type)
        {
            try
            {
                object val = Convert.ChangeType(dict[key], type);
                row[key] = val.ToString();
            }
            catch (FormatException)
            {
                Tools.WriteToTrace(string.Format("FormatException for key {0} with value {1}", key, dict[key]));
            }
            catch (KeyNotFoundException)
            {
                Tools.WriteToTrace(string.Format("KeyNotFoundException for key {0}", key));
            }
        }

        /// <summary>
        ///     Tries to the change the value of specific DataRow entry by using the corresponding value of a dictionary entry. Operation will
        ///     succeed only if all parts of the dictionary entry, after the latter is split at each
        ///     <c>splitCharacter</c>, can be converted into the specified <c>type</c>.
        /// </summary>
        /// <param name="row">The DataRow containing the entry of which we want to try and change the value.</param>
        /// <param name="dict">The dictionary containing the value we're trying to set.</param>
        /// <param name="key">The key representing both the DataRow column as well as the Dictionary key.</param>
        /// <param name="type">The type to attempt to convert all parts of the dictionary entry to.</param>
        /// <param name="splitCharacter">The character used to split the dictionary entry at.</param>
        public static void TryChangeValue(
            this DataRow row, Dictionary<string, string> dict, string key, Type type, string splitCharacter)
        {
            try
            {
                string s = dict[key];
                string[] parts = s.Split(new[] { splitCharacter }, StringSplitOptions.None);
                foreach (string part in parts)
                {
                    Convert.ChangeType(part, type);
                }
                row[key] = s;
            }
            catch (FormatException)
            {
            }
            catch (KeyNotFoundException)
            {
            }
        }

        /// <summary>Tries to the set the value of a variable using a user-specified dictionary entry.</summary>
        /// <typeparam name="T">The type of the variable.</typeparam>
        /// <param name="variable">The variable.</param>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The dictionary key.</param>
        /// <param name="onErrorRemain">
        ///     If <c>true</c>, if the method is unsuccessful, the value returned is the previous one; otherwise, the default value for the type
        ///     is returned.
        /// </param>
        /// <returns>
        ///     The value that the variable should be set to if the operation succeeds. If the cast is invalid, it returns the default value
        ///     of the type. If the key isn't found, it returns the original value of the variable.
        /// </returns>
        public static T TrySetValue<T>(this T variable, Dictionary<string, string> dict, string key, bool onErrorRemain = false)
        {
            try
            {
                if (typeof(T).BaseType != null)
                {
                    if (typeof(T).BaseType == typeof(Enum))
                    {
                        return (T) Enum.Parse(typeof(T), dict[key]);
                    }
                }
                var ret = (T) Convert.ChangeType(dict[key], typeof(T));
                return ret;
            }
            catch (OverflowException)
            {
                Tools.WriteToTrace(string.Format("OverflowException for key {0} with value '{1}'", key, dict[key]));
                return onErrorRemain ? variable : default(T);
            }
            catch (InvalidCastException)
            {
                Tools.WriteToTrace(string.Format("InvalidCastException for key {0} with value '{1}'", key, dict[key]));
                return onErrorRemain ? variable : default(T);
            }
            catch (FormatException)
            {
                Tools.WriteToTrace(string.Format("FormatException for key {0} with value '{1}'", key, dict[key]));
                return onErrorRemain ? variable : default(T);
            }
            catch (ArgumentException)
            {
                Tools.WriteToTrace(string.Format("ArgumentException for key {0} with value '{1}'", key, dict[key]));
                return onErrorRemain ? variable : default(T);
            }
            catch (KeyNotFoundException)
            {
                return variable;
            }
        }

        /// <summary>
        ///     Tries to the set the value of a variable using a user-specified dictionary entry, after converting it to the specified
        ///     <c>type</c>.
        /// </summary>
        /// <typeparam name="T">The type of the variable.</typeparam>
        /// <param name="variable">The variable.</param>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The dictionary key.</param>
        /// <param name="type">The type.</param>
        /// <param name="onErrorRemain">
        ///     If <c>true</c>, if the method is unsuccessful, the value returned is the previous one; otherwise, the default value for the type
        ///     is returned.
        /// </param>
        /// <returns>
        ///     The value that the variable should be set to if the operation succeeds. If the cast is invalid, it returns the default value
        ///     of the type. If the key isn't found, it returns the original value of the variable.
        /// </returns>
        public static T TrySetValue<T>(
            this T variable, Dictionary<string, string> dict, string key, Type type, bool onErrorRemain = false)
        {
            try
            {
                object val = Convert.ChangeType(dict[key], type);
                var ret = (T) Convert.ChangeType(val, typeof(T));
                return ret;
            }
            catch (OverflowException)
            {
                Tools.WriteToTrace(string.Format("OverflowException for key {0} with value '{1}'", key, dict[key]));
                if (onErrorRemain)
                {
                    return variable;
                }
                else
                {
                    return default(T);
                }
            }
            catch (InvalidCastException)
            {
                Tools.WriteToTrace(string.Format("InvalidCastException for key {0} with value '{1}'", key, dict[key]));
                if (onErrorRemain)
                {
                    return variable;
                }
                else
                {
                    return default(T);
                }
            }
            catch (FormatException)
            {
                Tools.WriteToTrace(string.Format("FormatException for key {0} with value '{1}'", key, dict[key]));
                if (onErrorRemain)
                {
                    return variable;
                }
                else
                {
                    return default(T);
                }
            }
            catch (ArgumentException)
            {
                Tools.WriteToTrace(string.Format("ArgumentException for key {0} with value '{1}'", key, dict[key]));
                if (onErrorRemain)
                {
                    return variable;
                }
                else
                {
                    return default(T);
                }
            }
            catch (KeyNotFoundException)
            {
                return variable;
            }
        }

        /// <summary>
        ///     Tries to the set the value of a variable by using the corresponding value of a dictionary entry. Operation will succeed only if
        ///     all parts of the dictionary entry, after the latter is split at each
        ///     <c>splitCharacter</c>, can be converted into the specified <c>type</c>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable">The variable.</param>
        /// <param name="dict">The dictionary containing the value we're trying to set.</param>
        /// <param name="key">The key representing both the DataRow column as well as the Dictionary key.</param>
        /// <param name="type">The type to attempt to convert all parts of the dictionary entry to.</param>
        /// <param name="splitCharacter">The character used to split the dictionary entry at.</param>
        /// <param name="onErrorRemain">
        ///     If <c>true</c>, if the method is unsuccessful, the value returned is the previous one; otherwise, the default value for the type
        ///     is returned.
        /// </param>
        /// <returns>
        ///     The value that the variable should be set to if the operation succeeds. If the cast is invalid, it returns the default value
        ///     of the type. If the key isn't found, it returns the original value of the variable.
        /// </returns>
        public static T TrySetValue<T>(
            this T variable, Dictionary<string, string> dict, string key, Type type, string splitCharacter, bool onErrorRemain = false)
        {
            try
            {
                string s = dict[key];
                string[] parts = s.Split(new[] { splitCharacter }, StringSplitOptions.None);
                foreach (string part in parts)
                {
                    Convert.ChangeType(part, type);
                }
                var ret = (T) Convert.ChangeType(s, typeof(T));
                return ret;
            }
            catch (OverflowException)
            {
                Tools.WriteToTrace(string.Format("OverflowException for key {0} with value '{1}'", key, dict[key]));
                if (onErrorRemain)
                {
                    return variable;
                }
                else
                {
                    return default(T);
                }
            }
            catch (InvalidCastException)
            {
                Tools.WriteToTrace(string.Format("InvalidCastException for key {0} with value '{1}'", key, dict[key]));
                if (onErrorRemain)
                {
                    return variable;
                }
                else
                {
                    return default(T);
                }
            }
            catch (FormatException)
            {
                Tools.WriteToTrace(string.Format("FormatException for key {0} with value '{1}'", key, dict[key]));
                if (onErrorRemain)
                {
                    return variable;
                }
                else
                {
                    return default(T);
                }
            }
            catch (ArgumentException)
            {
                Tools.WriteToTrace(string.Format("ArgumentException for key {0} with value '{1}'", key, dict[key]));
                if (onErrorRemain)
                {
                    return variable;
                }
                else
                {
                    return default(T);
                }
            }
            catch (KeyNotFoundException)
            {
                return variable;
            }
        }

        /// <summary>Creates a deep-cloned copy of an object.</summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="original">The original object.</param>
        /// <param name="args">The arguments to pass to the constructor of the new object.</param>
        /// <returns>The copy of the object.</returns>
        public static T DeepClone<T>(this T original, params Object[] args)
        {
            return original.deepClone(new Dictionary<object, object>(), args);
        }

        /// <summary>Creates a deep-cloned copy of an object.</summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="original">The original object.</param>
        /// <param name="copies">A dictionary containing the copies of the object.</param>
        /// <param name="args">The arguments to pass to the constructor of the new object.</param>
        /// <returns>The copy of the object.</returns>
        private static T deepClone<T>(this T original, Dictionary<object, object> copies, params Object[] args)
        {
            T result;
            Type t = original.GetType();

            Object tmpResult;
            // Check if the object already has been copied
            if (copies.TryGetValue(original, out tmpResult))
            {
                return (T) tmpResult;
            }
            else
            {
                if (!t.IsArray)
                {
                    /* Create new instance, at this point you pass parameters to
                        * the constructor if the constructor if there is no default constructor
                        * or you change it to Activator.CreateInstance<T>() if there is always
                        * a default constructor */
                    result = (T) Activator.CreateInstance(t, args);
                    copies.Add(original, result);

                    // Maybe you need here some more BindingFlags
                    foreach (FieldInfo field in
                        t.GetFields(
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance))
                    {
                        /* You can filter the fields here ( look for attributes and avoid
                            * unwanted fields ) */

                        object fieldValue = field.GetValue(original);

                        // Check here if the instance should be cloned
                        Type ft = field.FieldType;

                        /* You can check here for ft.GetCustomAttributes(typeof(SerializableAttribute), false).Length != 0 to 
                            * avoid types which do not support serialization ( e.g. NetworkStreams ) */
                        if (fieldValue != null && !ft.IsValueType && ft != typeof(String))
                        {
                            fieldValue = fieldValue.deepClone(copies);
                            /* Does not support parameters for subobjects nativly, but you can provide them when using
                                * a delegate to create the objects instead of the Activator. Delegates should not work here
                                * they need some more love */
                        }

                        field.SetValue(result, fieldValue);
                    }
                }
                else
                {
                    // Handle arrays here
                    var originalArray = (Array) (Object) original;
                    var resultArray = (Array) originalArray.Clone();
                    copies.Add(original, resultArray);

                    // If the type is not a value type we need to copy each of the elements
                    if (!t.GetElementType().IsValueType)
                    {
                        var lengths = new Int32[t.GetArrayRank()];
                        var indicies = new Int32[lengths.Length];
                        // Get lengths from original array
                        for (int i = 0; i < lengths.Length; i++)
                        {
                            lengths[i] = resultArray.GetLength(i);
                        }

                        int p = lengths.Length - 1;

                        /* Now we need to iterate though each of the ranks
                            * we need to keep it generic to support all array ranks */
                        while (increment(indicies, lengths, p))
                        {
                            object value = resultArray.GetValue(indicies);
                            if (value != null)
                            {
                                resultArray.SetValue(value.deepClone(copies), indicies);
                            }
                        }
                    }
                    result = (T) (Object) resultArray;
                }
                return result;
            }
        }

        private static Boolean increment(Int32[] indicies, Int32[] lengths, Int32 p)
        {
            if (p > -1)
            {
                indicies[p]++;
                if (indicies[p] < lengths[p])
                {
                    return true;
                }
                else
                {
                    if (increment(indicies, lengths, p - 1))
                    {
                        indicies[p] = 0;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        //
        // Created by Rahul D. of Indigo Architects.
        // Used under the Common Development and Distribution License (CDDL-1.0) license.
        //
        /// <summary>Creates a deep-cloned copy of an object using serialization.</summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="realObject">The original object.</param>
        /// <returns>The deep-cloned copy of the object.</returns>
        public static T Clone<T>(this T realObject)
        {
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, realObject);
                objectStream.Seek(0, SeekOrigin.Begin);
                return (T) formatter.Deserialize(objectStream);
            }
        }

        /// <summary>Randomizes the list.</summary>
        /// <typeparam name="T">The type of the list's items.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>Converts the object to an Int32.</summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="o">The source object.</param>
        /// <returns>The converted Int32.</returns>
        public static Int32 ToInt32<T>(this T o)
        {
            return Convert.ToInt32(o);
        }

        /// <summary>Converts the object to a Double.</summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="o">The soutce object.</param>
        /// <returns>The converted Double.</returns>
        public static Double ToDouble<T>(this T o)
        {
            return Convert.ToDouble(o);
        }

        /// <summary>Gets the first item from a list and then removes it, similar to the Stack/Queue Pop() function.</summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="list">The source list.</param>
        /// <returns>The first object of the list.</returns>
        /// <exception cref="System.InvalidOperationException">List is empty.</exception>
        public static T Pop<T>(this List<T> list)
        {
            if (list.Count == 0)
            {
                throw new InvalidOperationException("List is empty.");
            }

            T item = list[0];
            list.RemoveAt(0);
            return item;
        }
    }
}