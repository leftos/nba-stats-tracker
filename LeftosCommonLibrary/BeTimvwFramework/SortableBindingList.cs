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

using System;
using System.Collections.Generic;
using System.ComponentModel;

#endregion

namespace LeftosCommonLibrary.BeTimvwFramework
{
    /// <summary>
    ///     An extension of the binding list that allows sorting.
    /// </summary>
    /// <typeparam name="T">The type of objects this list can contain.</typeparam>
    public class SortableBindingList<T> : BindingList<T>
    {
        private readonly Dictionary<Type, PropertyComparer<T>> _comparers;
        private bool _isSorted;
        private ListSortDirection _listSortDirection;
        private PropertyDescriptor _propertyDescriptor;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SortableBindingList{T}" /> class.
        /// </summary>
        public SortableBindingList()
            : base(new List<T>())
        {
            _comparers = new Dictionary<Type, PropertyComparer<T>>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SortableBindingList{T}" /> class containing a copy of the objects in the enumeration.
        /// </summary>
        /// <param name="enumeration">The enumeration to initialize the list with.</param>
        public SortableBindingList(IEnumerable<T> enumeration)
            : base(new List<T>(enumeration))
        {
            _comparers = new Dictionary<Type, PropertyComparer<T>>();
        }

        /// <summary>
        ///     Gets a value indicating whether this list supports sorting.
        /// </summary>
        /// <value>
        ///     <c>true</c> if it supports sorting; otherwise, <c>false</c>.
        /// </value>
        protected override bool SupportsSortingCore
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the list is sorted.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the list is sorted; otherwise, <c>false</c>.
        /// </value>
        protected override bool IsSortedCore
        {
            get
            {
                return _isSorted;
            }
        }

        /// <summary>
        ///     Gets the sorting property descriptor.
        /// </summary>
        /// <value>
        ///     The sorting property descriptor.
        /// </value>
        protected override PropertyDescriptor SortPropertyCore
        {
            get
            {
                return _propertyDescriptor;
            }
        }

        /// <summary>
        ///     Gets the sorting direction.
        /// </summary>
        /// <value>
        ///     The sorting direction.
        /// </value>
        protected override ListSortDirection SortDirectionCore
        {
            get
            {
                return _listSortDirection;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the list supports searching.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the list supports searching; otherwise, <c>false</c>.
        /// </value>
        protected override bool SupportsSearchingCore
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        ///     Applies the sort.
        /// </summary>
        /// <param name="property">The property descriptor.</param>
        /// <param name="direction">The sorting direction.</param>
        protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
        {
            var itemsList = (List<T>) Items;

            Type propertyType = property.PropertyType;
            PropertyComparer<T> comparer;
            if (!_comparers.TryGetValue(propertyType, out comparer))
            {
                comparer = new PropertyComparer<T>(property, direction);
                _comparers.Add(propertyType, comparer);
            }

            comparer.SetPropertyAndDirection(property, direction);
            itemsList.Sort(comparer);

            _propertyDescriptor = property;
            _listSortDirection = direction;
            _isSorted = true;

            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        /// <summary>
        ///     Removes the sort.
        /// </summary>
        protected override void RemoveSortCore()
        {
            _isSorted = false;
            _propertyDescriptor = base.SortPropertyCore;
            _listSortDirection = base.SortDirectionCore;

            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        /// <summary>
        ///     Finds an item.
        /// </summary>
        /// <param name="property">The property descriptor.</param>
        /// <param name="key">The key to find.</param>
        /// <returns></returns>
        protected override int FindCore(PropertyDescriptor property, object key)
        {
            int count = Count;
            for (int i = 0; i < count; ++i)
            {
                T element = this[i];
                object value = property.GetValue(element);
                if (value != null && value.Equals(key))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}