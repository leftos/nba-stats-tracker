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

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

#endregion

namespace LeftosCommonLibrary
{
    public static class GenericEventHandlers
    {
        private static bool _isTabPressed;

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
                                                                                            }), DispatcherPriority.Background, new object[] {null});
                }
            }
        }

        public static void Any_PreviewKeyDown_CheckTab(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                _isTabPressed = true;
            }
        }

        public static void Any_PreviewKeyUp_CheckTab(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                _isTabPressed = false;
            }
        }

        public static void Any_ShowToolTip(object sender, DependencyPropertyChangedEventArgs e)
        {
            //Get tooltip from sender.
            var control = sender as Control;
            if (control != null)
            {
                ToolTip tt = (ToolTip)control.ToolTip;
                //Places the Tooltip under the control rather than at the mouse position
                tt.PlacementTarget = (UIElement)sender;
                tt.Placement = PlacementMode.Right;
                tt.PlacementRectangle = new Rect(0, control.Height, 0, 0);
                //Shows tooltip if KeyboardFocus is within.
                tt.IsOpen = control.IsKeyboardFocusWithin;
            }
        }
    }
}