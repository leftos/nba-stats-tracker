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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

#endregion

namespace NBA_Stats_Tracker.Helper.ListExtensions
{
    /// <summary>
    ///     Extensions for various kinds of .NET list constructs, including BindingList and ObservableCollection.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        ///     Sorts using the default IComparer of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bl">The binding list to be sorted.</param>
        public static void Sort<T>(this BindingList<T> bl)
        {
            sort(bl, null, null);
        }

        /// <summary>
        ///     Sorts using a custom IComparer of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bl">The binding list to be sorted.</param>
        /// <param name="pComparer">The comparer.</param>
        public static void Sort<T>(this BindingList<T> bl, IComparer<T> pComparer)
        {
            sort(bl, pComparer, null);
        }

        /// <summary>
        ///     Sorts using a custom Comparison of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bl">The binding list to be sorted.</param>
        /// <param name="pComparison">The comparison.</param>
        public static void Sort<T>(this BindingList<T> bl, Comparison<T> pComparison)
        {
            sort(bl, null, pComparison);
        }

        /// <summary>
        ///     Sorts using a custom IComparer and custom Comparison.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bl">The binding list to be sorted.</param>
        /// <param name="pComparer">The comparer.</param>
        /// <param name="pComparison">The comparison.</param>
        private static void sort<T>(this BindingList<T> bl, IComparer<T> pComparer, Comparison<T> pComparison)
        {
            //Extract items and sort separately
            List<T> sortList = bl.ToList();
            if (pComparison == null)
            {
                sortList.Sort(pComparer);
            } //if
            else
            {
                sortList.Sort(pComparison);
            } //else

            //Disable notifications, rebuild, and re-enable notifications
            bool oldRaise = bl.RaiseListChangedEvents;
            bl.RaiseListChangedEvents = false;
            try
            {
                bl.Clear();
                sortList.ForEach(bl.Add);
            }
            finally
            {
                bl.RaiseListChangedEvents = oldRaise;
                bl.ResetBindings();
            }
        }

        /// <summary>
        ///     Sorts the specified ObservableCollection using the default IComparer and Comparison.
        /// </summary>
        public static void Sort<T>(this ObservableCollection<T> oc)
        {
            sort(oc, null, null);
        }

        /// <summary>
        ///     Sorts the specified ObservableCollection using a custom IComparer.
        /// </summary>
        public static void Sort<T>(this ObservableCollection<T> oc, IComparer<T> pComparer)
        {
            sort(oc, pComparer, null);
        }

        /// <summary>
        ///     Sorts the specified ObservableCollection using a custom Comparison.
        /// </summary>
        public static void Sort<T>(this ObservableCollection<T> oc, Comparison<T> pComparison)
        {
            sort(oc, null, pComparison);
        }

        /// <summary>
        ///     Sorts the specified ObservableCollection using a custom IComparer and Comparison.
        /// </summary>
        private static void sort<T>(this ObservableCollection<T> oc, IComparer<T> pComparer, Comparison<T> pComparison)
        {
            //Extract items and sort separately
            List<T> sortList = oc.ToList();
            if (pComparison == null)
            {
                sortList.Sort(pComparer);
            } //if
            else
            {
                sortList.Sort(pComparison);
            } //else

            oc.Clear();
            sortList.ForEach(oc.Add);
        }

        /// <summary>
        ///     Implements a comparison between the values of two int-string KeyValuePairs.
        /// </summary>
        public static int KVPStringComparison(KeyValuePair<int, string> kvp1, KeyValuePair<int, string> kvp2)
        {
            return String.Compare(kvp1.Value, kvp2.Value);
        }
    }
}