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
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace LeftosCommonLibrary
{
    public static class GenericExtensions
    {
        public static void TryChangeValue<T>(this T variable, Dictionary<string, string> dict, string key)
        {
            try
            {
                variable = (T) Convert.ChangeType(dict[key], typeof (T));
            }
            catch (InvalidCastException)
            {
                variable = default(T);
            }
            catch (KeyNotFoundException)
            {
            }
        }

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

        public static void TryChangeValue(this DataRow row, Dictionary<string, string> dict, string key, Type type, string splitCharacter)
        {
            try
            {
                string s = dict[key];
                string[] parts = s.Split(new[] {splitCharacter}, StringSplitOptions.None);
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

        public static T TrySetValue<T>(this T variable, Dictionary<string, string> dict, string key)
        {
            try
            {
                return (T) Convert.ChangeType(dict[key], typeof (T));
            }
            catch (InvalidCastException)
            {
                return default(T);
            }
            catch (KeyNotFoundException)
            {
                return variable;
            }
        }

        public static T TrySetValue<T>(this T variable, Dictionary<string, string> dict, string key, Type type)
        {
            try
            {
                object val = Convert.ChangeType(dict[key], type);
                return (T) Convert.ChangeType(val, typeof (T));
            }
            catch (InvalidCastException)
            {
                return default(T);
            }
            catch (KeyNotFoundException)
            {
                return variable;
            }
        }

        public static T TrySetValue<T>(this T variable, Dictionary<string, string> dict, string key, Type type, string splitCharacter)
        {
            try
            {
                string s = dict[key];
                string[] parts = s.Split(new[] {splitCharacter}, StringSplitOptions.None);
                foreach (string part in parts)
                {
                    Convert.ChangeType(part, type);
                }
                return (T) Convert.ChangeType(s, typeof (T));
            }
            catch (InvalidCastException)
            {
                return default(T);
            }
            catch (KeyNotFoundException)
            {
                return variable;
            }
        }

        public static T DeepClone<T>(this T original, params Object[] args)
        {
            return original.DeepClone(new Dictionary<object, object>(), args);
        }

        private static T DeepClone<T>(this T original, Dictionary<object, object> copies, params Object[] args)
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
                        t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance))
                    {
                        /* You can filter the fields here ( look for attributes and avoid
                            * unwanted fields ) */

                        Object fieldValue = field.GetValue(original);

                        // Check here if the instance should be cloned
                        Type ft = field.FieldType;

                        /* You can check here for ft.GetCustomAttributes(typeof(SerializableAttribute), false).Length != 0 to 
                            * avoid types which do not support serialization ( e.g. NetworkStreams ) */
                        if (fieldValue != null && !ft.IsValueType && ft != typeof (String))
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
                    result = (T) (Object) resultArray;
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

        public static T Clone<T>(this T RealObject)
        {
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, RealObject);
                objectStream.Seek(0, SeekOrigin.Begin);
                return (T) formatter.Deserialize(objectStream);
            }
        }
    }
}