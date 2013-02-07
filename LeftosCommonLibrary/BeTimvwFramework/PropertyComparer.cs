#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2013
// 
// Initial development until v1.0 done as part of the implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

#endregion

namespace LeftosCommonLibrary.BeTimvwFramework
{
    public class PropertyComparer<T> : IComparer<T>
    {
        private readonly IComparer _comparer;
        private PropertyDescriptor _propertyDescriptor;
        private int _reverse;

        public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
        {
            _propertyDescriptor = property;
            Type comparerForPropertyType = typeof (Comparer<>).MakeGenericType(property.PropertyType);
            _comparer =
                (IComparer)
                comparerForPropertyType.InvokeMember("Default", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public, null,
                                                     null, null);
            setListSortDirection(direction);
        }

        #region IComparer<T> Members

        public int Compare(T x, T y)
        {
            return _reverse*_comparer.Compare(_propertyDescriptor.GetValue(x), _propertyDescriptor.GetValue(y));
        }

        #endregion

        private void setPropertyDescriptor(PropertyDescriptor descriptor)
        {
            _propertyDescriptor = descriptor;
        }

        private void setListSortDirection(ListSortDirection direction)
        {
            _reverse = direction == ListSortDirection.Ascending ? 1 : -1;
        }

        public void SetPropertyAndDirection(PropertyDescriptor descriptor, ListSortDirection direction)
        {
            setPropertyDescriptor(descriptor);
            setListSortDirection(direction);
        }
    }
}