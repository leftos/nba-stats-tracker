#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Uses Object DeepCloning Code Example by Felix K.
// Source: http://stackoverflow.com/a/8026574/427338
//
// Uses Object DeepCloning Code Example by Rahul Dantkale of Indigo Architects
// Source: http://www.codeproject.com/Articles/23983/Object-Cloning-at-its-simplest
//
// Implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou,
// Computer Engineering & Informatics Department, University of Patras, Greece.
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace LeftosCommonLibrary
{

    /// <summary>
    /// Implements generic extension methods.
    /// </summary>
    public static class GenericExtensions
    {
        /// <summary>
        /// Tries to the change the value of a variable using the value of a specified dictionary entry.
        /// </summary>
        /// <typeparam name="T">The type of the variable.</typeparam>
        /// <param name="variable">The variable.</param>
        /// <param name="dict">The dict.</param>
        /// <param name="key">The key.</param>
        public static void TryChangeValue<T>(this T variable, Dictionary<string, string> dict, string key)
        {
            try
            {
                variable = (T)Convert.ChangeType(dict[key], typeof(T));
            }
            catch (InvalidCastException)
            {
                Trace.WriteLine(string.Format("{2}: InvalidCastException for key {0} with value {1}", key, dict[key], DateTime.Now));
            }
            catch (FormatException)
            {
                Trace.WriteLine(string.Format("{2}: FormatException for key {0} with value {1}", key, dict[key], DateTime.Now));
            }
            catch (KeyNotFoundException)
            {
            }
        }

        /// <summary>
        /// Tries to the change the value of specific DataRow entry by using the corresponding value of a dictionary entry.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="dict">The dictionary containing the value we're trying to set.</param>
        /// <param name="key">The key representing both the DataRow column as well as the Dictionary key.</param>
        public static void TryChangeValue(this DataRow row, Dictionary<string, string> dict, string key)
        {
            try
            {
                row[key] = dict[key];
            }
            catch (KeyNotFoundException)
            {
            }
        }

        /// <summary>
        /// Tries to the change the value of specific DataRow entry by using the corresponding value of a dictionary entry, after converting it to the specified <see cref="type"/>.
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
            }
            catch (KeyNotFoundException)
            {
            }
        }

        /// <summary>
        /// Tries to the change the value of specific DataRow entry by using the corresponding value of a dictionary entry.
        /// Operation will succeed only if all parts of the dictionary entry, after the latter is split at each <see cref="splitCharacter" />, can be converted into the specified <see cref="type"/>.
        /// </summary>
        /// <param name="row">The DataRow containing the entry of which we want to try and change the value.</param>
        /// <param name="dict">The dictionary containing the value we're trying to set.</param>
        /// <param name="key">The key representing both the DataRow column as well as the Dictionary key.</param>
        /// <param name="type">The type to attempt to convert all parts of the dictionary entry to.</param>
        /// <param name="splitCharacter">The character used to split the dictionary entry at.</param>
        public static void TryChangeValue(this DataRow row, Dictionary<string, string> dict, string key, Type type, string splitCharacter)
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

        /// <summary>
        /// Tries to the set the value of a variable using a user-specified dictionary entry.
        /// </summary>
        /// <typeparam name="T">The type of the variable.</typeparam>
        /// <param name="variable">The variable.</param>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The dictionary key.</param>
        /// <returns>The value that the variable should be set to if the operation succeeds. 
        /// If the cast is invalid, it returns the default value of the type. 
        /// If the key isn't found, it returns the original value of the variable.</returns>
        public static T TrySetValue<T>(this T variable, Dictionary<string, string> dict, string key, bool onErrorRemain = false)
        {
            try
            {
                if (typeof(T).BaseType != null)
                {
                    if (typeof(T).BaseType == typeof(Enum))
                    {
                        return (T)Enum.Parse(typeof(T), dict[key]);
                    }
                }
                var ret = (T)Convert.ChangeType(dict[key], typeof(T));
                return ret;
            }
            catch (OverflowException)
            {
                Trace.WriteLine(string.Format("{2}: OverflowException for key {0} with value '{1}'", key, dict[key], DateTime.Now));
                if (onErrorRemain) return variable;
                else return default(T);
            }
            catch (InvalidCastException)
            {
                Trace.WriteLine(string.Format("{2}: InvalidCastException for key {0} with value '{1}'", key, dict[key], DateTime.Now));
                if (onErrorRemain) return variable;
                else return default(T);
            }
            catch (FormatException)
            {
                Trace.WriteLine(string.Format("{2}: FormatException for key {0} with value '{1}'", key, dict[key], DateTime.Now));
                if (onErrorRemain) return variable;
                else return default(T);
            }
            catch (ArgumentException)
            {
                Trace.WriteLine(string.Format("{2}: ArgumentException for key {0} with value '{1}'", key, dict[key], DateTime.Now));
                if (onErrorRemain) return variable;
                else return default(T);
            }
            catch (KeyNotFoundException)
            {
                return variable;
            }
        }

        /// <summary>
        /// Tries to the set the value of a variable using a user-specified dictionary entry, after converting it to the specified <see cref="type"/>.
        /// </summary>
        /// <typeparam name="T">The type of the variable.</typeparam>
        /// <param name="variable">The variable.</param>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The dictionary key.</param>
        /// <param name="type">The type.</param>
        /// <returns>The value that the variable should be set to if the operation succeeds. 
        /// If the cast is invalid, it returns the default value of the type. 
        /// If the key isn't found, it returns the original value of the variable.</returns>
        public static T TrySetValue<T>(this T variable, Dictionary<string, string> dict, string key, Type type, bool onErrorRemain = false)
        {
            try
            {
                object val = Convert.ChangeType(dict[key], type);
                var ret = (T)Convert.ChangeType(val, typeof(T));
                return ret;
            }
            catch (OverflowException)
            {
                Trace.WriteLine(string.Format("{2}: OverflowException for key {0} with value '{1}'", key, dict[key], DateTime.Now));
                if (onErrorRemain) return variable;
                else return default(T);
            }
            catch (InvalidCastException)
            {
                Trace.WriteLine(string.Format("{2}: InvalidCastException for key {0} with value '{1}'", key, dict[key], DateTime.Now));
                if (onErrorRemain) return variable;
                else return default(T);
            }
            catch (FormatException)
            {
                Trace.WriteLine(string.Format("{2}: FormatException for key {0} with value '{1}'", key, dict[key], DateTime.Now));
                if (onErrorRemain) return variable;
                else return default(T);
            }
            catch (ArgumentException)
            {
                Trace.WriteLine(string.Format("{2}: ArgumentException for key {0} with value '{1}'", key, dict[key], DateTime.Now));
                if (onErrorRemain) return variable;
                else return default(T);
            }
            catch (KeyNotFoundException)
            {
                return variable;
            }
        }

        /// <summary>
        /// Tries to the set the value of a variable by using the corresponding value of a dictionary entry.
        /// Operation will succeed only if all parts of the dictionary entry, after the latter is split at each <see cref="splitCharacter" />, can be converted into the specified <see cref="type" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable">The variable.</param>
        /// <param name="dict">The dictionary containing the value we're trying to set.</param>
        /// <param name="key">The key representing both the DataRow column as well as the Dictionary key.</param>
        /// <param name="type">The type to attempt to convert all parts of the dictionary entry to.</param>
        /// <param name="splitCharacter">The character used to split the dictionary entry at.</param>
        /// <returns>The value that the variable should be set to if the operation succeeds. 
        /// If the cast is invalid, it returns the default value of the type. 
        /// If the key isn't found, it returns the original value of the variable.</returns>
        public static T TrySetValue<T>(this T variable, Dictionary<string, string> dict, string key, Type type, string splitCharacter, bool onErrorRemain = false)
        {
            try
            {
                string s = dict[key];
                string[] parts = s.Split(new[] { splitCharacter }, StringSplitOptions.None);
                foreach (string part in parts)
                {
                    Convert.ChangeType(part, type);
                }
                var ret = (T)Convert.ChangeType(s, typeof(T));
                return ret;
            }
            catch (OverflowException)
            {
                Trace.WriteLine(string.Format("{2}: OverflowException for key {0} with value '{1}'", key, dict[key], DateTime.Now));
                if (onErrorRemain) return variable;
                else return default(T);
            }
            catch (InvalidCastException)
            {
                Trace.WriteLine(string.Format("{2}: InvalidCastException for key {0} with value '{1}'", key, dict[key], DateTime.Now));
                if (onErrorRemain) return variable;
                else return default(T);
            }
            catch (FormatException)
            {
                Trace.WriteLine(string.Format("{2}: FormatException for key {0} with value '{1}'", key, dict[key], DateTime.Now));
                if (onErrorRemain) return variable;
                else return default(T);
            }
            catch (ArgumentException)
            {
                Trace.WriteLine(string.Format("{2}: ArgumentException for key {0} with value '{1}'", key, dict[key], DateTime.Now));
                if (onErrorRemain) return variable;
                else return default(T);
            }
            catch (KeyNotFoundException)
            {
                return variable;
            }
        }

        /// <summary>
        /// Creates a deep-cloned copy of an object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="original">The original object.</param>
        /// <param name="args">The arguments to pass to the constructor of the new object.</param>
        /// <returns>The copy of the object.</returns>
        public static T DeepClone<T>(this T original, params Object[] args)
        {
            return original.DeepClone(new Dictionary<object, object>(), args);
        }

        /// <summary>
        /// Creates a deep-cloned copy of an object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="original">The original object.</param>
        /// <param name="copies">A dictionary containing the copies of the object.</param>
        /// <param name="args">The arguments to pass to the constructor of the new object.</param>
        /// <returns>
        /// The copy of the object.
        /// </returns>
        private static T DeepClone<T>(this T original, Dictionary<object, object> copies, params Object[] args)
        {
            T result;
            Type t = original.GetType();

            Object tmpResult;
            // Check if the object already has been copied
            if (copies.TryGetValue(original, out tmpResult))
            {
                return (T)tmpResult;
            }
            else
            {
                if (!t.IsArray)
                {
                    /* Create new instance, at this point you pass parameters to
                        * the constructor if the constructor if there is no default constructor
                        * or you change it to Activator.CreateInstance<T>() if there is always
                        * a default constructor */
                    result = (T)Activator.CreateInstance(t, args);
                    copies.Add(original, result);

                    // Maybe you need here some more BindingFlags
                    foreach (FieldInfo field in
                        t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance))
                    {
                        /* You can filter the fields here ( look for attributes and avoid
                            * unwanted fields ) */

                        Object fieldValue = field.GetValue(original);

                        // Check here if the instance should be cloned
                        Type ft = field.FieldType;

                        /* You can check here for ft.GetCustomAttributes(typeof(SerializableAttribute), false).Length != 0 to 
                            * avoid types which do not support serialization ( e.g. NetworkStreams ) */
                        if (fieldValue != null && !ft.IsValueType && ft != typeof(String))
                        {
                            fieldValue = fieldValue.DeepClone(copies);
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
                    var originalArray = (Array)(Object)original;
                    var resultArray = (Array)originalArray.Clone();
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

                        Int32 p = lengths.Length - 1;

                        /* Now we need to iterate though each of the ranks
                            * we need to keep it generic to support all array ranks */
                        while (Increment(indicies, lengths, p))
                        {
                            Object value = resultArray.GetValue(indicies);
                            if (value != null)
                                resultArray.SetValue(value.DeepClone(copies), indicies);
                        }
                    }
                    result = (T)(Object)resultArray;
                }
                return result;
            }
        }

        private static Boolean Increment(Int32[] indicies, Int32[] lengths, Int32 p)
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
                    if (Increment(indicies, lengths, p - 1))
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

        /// <summary>
        /// Creates a deep-cloned copy of an object using serialization.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="RealObject">The original object.</param>
        /// <returns>The deep-cloned copy of the object.</returns>
        public static T Clone<T>(this T RealObject)
        {
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, RealObject);
                objectStream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(objectStream);
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}