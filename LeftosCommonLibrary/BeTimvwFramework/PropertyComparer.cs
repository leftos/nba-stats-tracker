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

#region Using Directives

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

#endregion

namespace LeftosCommonLibrary.BeTimvwFramework
{
    /// <summary>
    ///     IComparer to compare two properties and thus sort a list.
    /// </summary>
    /// <typeparam name="T">Type of property.</typeparam>
    public class PropertyComparer<T> : IComparer<T>
    {
        private readonly IComparer _comparer;
        private PropertyDescriptor _propertyDescriptor;
        private int _reverse;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyComparer{T}"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="direction">The sorting direction.</param>
        public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
        {
            _propertyDescriptor = property;
            var comparerForPropertyType = typeof (Comparer<>).MakeGenericType(property.PropertyType);
            _comparer =
                (IComparer)
                comparerForPropertyType.InvokeMember("Default", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public, null,
                                                     null, null);
            setListSortDirection(direction);
        }

        #region IComparer<T> Members

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value Condition Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
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

        /// <summary>
        /// Sets the property descriptor and sorting direction.
        /// </summary>
        /// <param name="descriptor">The property descriptor.</param>
        /// <param name="direction">The sorting direction.</param>
        public void SetPropertyAndDirection(PropertyDescriptor descriptor, ListSortDirection direction)
        {
            setPropertyDescriptor(descriptor);
            setListSortDirection(direction);
        }
    }
}