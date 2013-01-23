#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
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

#region Using Directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

#endregion

namespace LeftosCommonLibrary
{
    /// <summary>
    ///     Implements event handlers that improve the behavior WPF controls.
    /// </summary>
    public static class GenericEventHandlers
    {
        private static bool _isTabPressed;

        public static bool OnExecutedPaste(object sender, ExecutedRoutedEventArgs args)
        {
            bool noErrors = true;

            var s = (DataGrid)sender;
            // parse the clipboard data
            List<string[]> rowData = CSV.ParseClipboardData();
            bool hasAddedNewRow = false;

            // call OnPastingCellClipboardContent for each cell
            int minRowIndex = Math.Max(s.Items.IndexOf(s.CurrentItem), 0);
            int maxRowIndex = s.Items.Count - 1;
            int minColumnDisplayIndex = (s.SelectionUnit != DataGridSelectionUnit.FullRow) ? s.Columns.IndexOf(s.CurrentColumn) : 0;
            int maxColumnDisplayIndex = s.Columns.Count - 1;

            int rowDataIndex = 0;
            for (int i = minRowIndex; i <= maxRowIndex && rowDataIndex < rowData.Count; i++, rowDataIndex++)
            {
                if (s.CanUserAddRows && i == maxRowIndex + 1)
                {
                    // add a new row to be pasted to
                    ICollectionView cv = CollectionViewSource.GetDefaultView(s.Items);
                    IEditableCollectionView iecv = cv as IEditableCollectionView;
                    if (iecv != null)
                    {
                        hasAddedNewRow = true;
                        iecv.AddNew();
                        if (rowDataIndex + 1 < rowData.Count)
                        {
                            // still has more items to paste, update the maxRowIndex
                            maxRowIndex = s.Items.Count - 1;
                        }
                    }
                }
                else if (i == maxRowIndex + 1)
                {
                    continue;
                }

                int columnDataIndex = 0;
                for (int j = minColumnDisplayIndex;
                     j <= maxColumnDisplayIndex && columnDataIndex < rowData[rowDataIndex].Length;
                     j++, columnDataIndex++)
                {
                    DataGridColumn column = s.ColumnFromDisplayIndex(j);
                    string propertyName;
                    try
                    {
                        propertyName = ((column as DataGridBoundColumn).Binding as Binding).Path.Path;
                    }
                    catch
                    {
                        propertyName = column.SortMemberPath;
                    }
                    object item = s.Items[i];
                    object value = rowData[rowDataIndex][columnDataIndex];
                    object[] index = null;
                    var originalPropertyName = propertyName;
                    if (propertyName.Contains("[") && propertyName.Contains("]"))
                    {
                        index = new object[1];
                        index[0] = Convert.ToInt32(propertyName.Split('[')[1].Split(']')[0]);
                        propertyName = propertyName.Split('[')[0];
                    }
                    PropertyInfo pi = item.GetType().GetProperty(propertyName);
                    //PropertyInfo opi = item.GetType().GetProperty(originalPropertyName);
                    Type pType = index != null ? pi.PropertyType.GetGenericArguments()[0] : pi.PropertyType;
                    if (pi != null)
                    {
                        object convertedValue = null;
                        try
                        {
                            convertedValue = Convert.ChangeType(value, pType); // Try to convert to proper type
                        }
                        catch // Could be enum, or completely improper data
                        {
                            if (pType.BaseType != null)
                            {
                                if (pType.BaseType == typeof(Enum))
                                {
                                    try
                                    {
                                        convertedValue = Enum.Parse(pType, value.ToString()); // Try to parse enum
                                    }
                                    catch (ArgumentException)
                                    {
                                        Tools.WriteToTrace(String.Format("\"{0}\" is not a valid member of \"{1}\"", value, pType.Name));
                                        noErrors = false;
                                        continue;
                                    }
                                }
                                else // What else could be there?
                                {
                                    Tools.WriteToTrace(String.Format("\"{0}\" couldn't be converted to {1}", value, pType.Name));
                                    noErrors = false;
                                    continue;
                                }
                            }
                            else
                            {
                                Tools.WriteToTrace(String.Format("\"{0}\" couldn't be converted to {1}", value, pType.Name));
                                noErrors = false;
                                continue;
                            }
                        }

                        try
                        {
                            if (index == null) // No index, so no list/observable to update
                            {
                                item.GetType().GetProperty(propertyName).SetValue(item, convertedValue, null);
                            }
                            else // We have an index, so try to actually update the list's item
                            {
                                var collection = pi.GetValue(item, null);
                                collection.GetType().GetProperty("Item") // Item is the normal name for an indexer
                                          .SetValue(collection, convertedValue, index);
                            }
                        }
                        catch
                        {
                            Tools.WriteToTrace(String.Format("Couldn't update parameter {0} with value \"{1}\"", propertyName, value));
                            noErrors = false;
                            continue;
                        }
                    }
                    //column.OnPastingCellClipboardContent(item, rowData[rowDataIndex][columnDataIndex]);
                }
            }
            return noErrors;
        }

        /// <summary>
        ///     When added to the RowEditEnding event of a WPF DataGrid, if the user presses Tab while on the last cell of a row, the focus
        ///     is switched to the first cell of the next row, instead of another control altogether.
        ///     <see cref="Any_PreviewKeyDown_CheckTab" /> and <see cref="Any_PreviewKeyUp_CheckTab" /> should also be added as event handlers to the control.
        /// </summary>
        /// <param name="sender">The WPF DataGrid (or compatible) control from which the event was raised..</param>
        /// <param name="e">
        ///     The <see cref="DataGridRowEditEndingEventArgs" /> instance containing the event data.
        /// </param>
        public static void WPFDataGrid_RowEditEnding_GoToNewRowOnTab(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (_isTabPressed && e.EditAction == DataGridEditAction.Commit)
            {
                var dataGrid = sender as DataGrid;

                Debug.Assert(dataGrid != null, "dataGrid != null");
                if (e.Row.Item == dataGrid.Items[dataGrid.Items.Count - 2])
                {
                    Window parentWindow = Window.GetWindow(dataGrid);
                    Debug.Assert(parentWindow != null, "parentWindow != null");
                    parentWindow.Dispatcher.BeginInvoke(new DispatcherOperationCallback(param =>
                                                                                        {
                                                                                            dataGrid.Focus();
                                                                                            dataGrid.SelectedIndex =
                                                                                                dataGrid.Items.IndexOf(
                                                                                                    CollectionView.NewItemPlaceholder);
                                                                                            dataGrid.CurrentCell =
                                                                                                new DataGridCellInfo(
                                                                                                    CollectionView.NewItemPlaceholder,
                                                                                                    dataGrid.Columns[0]);

                                                                                            //dataGrid.BeginEdit();
                                                                                            return null;
                                                                                        }), DispatcherPriority.Background,
                                                        new object[] {null});
                }
            }
        }

        /// <summary>
        ///     When added to the PreviewKeyDown event of any control, it checks whether the key pressed was Tab.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="KeyEventArgs" /> instance containing the event data.
        /// </param>
        public static void Any_PreviewKeyDown_CheckTab(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                _isTabPressed = true;
            }
        }

        /// <summary>
        ///     When added to the PreviewKeyUp event of any control, it checks whether the key pressed was Tab.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="KeyEventArgs" /> instance containing the event data.
        /// </param>
        public static void Any_PreviewKeyUp_CheckTab(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                _isTabPressed = false;
            }
        }

        /// <summary>
        ///     Handles the ShowToolTip event of any control, placing and showing a tooltip under the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.
        /// </param>
        public static void Any_ShowToolTip(object sender, DependencyPropertyChangedEventArgs e)
        {
            //Get tooltip from sender.
            var control = sender as Control;
            if (control != null)
            {
                var tt = (ToolTip) control.ToolTip;
                //Places the Tooltip under the control rather than at the mouse position
                tt.PlacementTarget = (UIElement) sender;
                tt.Placement = PlacementMode.Right;
                tt.PlacementRectangle = new Rect(0, control.Height, 0, 0);
                //Shows tooltip if KeyboardFocus is within.
                tt.IsOpen = control.IsKeyboardFocusWithin;
            }
        }
    }
}