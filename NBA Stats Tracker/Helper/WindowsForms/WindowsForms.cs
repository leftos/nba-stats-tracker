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
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using IWin32Window = System.Windows.Forms.IWin32Window;

#endregion

namespace NBA_Stats_Tracker.Helper.WindowsForms
{
    /// <summary>
    ///     Implements legacy Windows Forms classes and methods in order to use dialogs present in that namespace.
    /// </summary>
    public static class WindowsForms
    {
        public static IWin32Window GetIWin32Window(this Visual visual)
        {
            var source = PresentationSource.FromVisual(visual) as HwndSource;
            if (source != null)
            {
                IWin32Window win = new OldWindow(source.Handle);
                return win;
            }
            throw (new Exception("GetIWin32Window failed for " + source));
        }

        #region Nested type: OldWindow

        private class OldWindow : IWin32Window
        {
            private readonly IntPtr _handle;

            public OldWindow(IntPtr handle)
            {
                _handle = handle;
            }

            #region IWin32Window Members

            IntPtr IWin32Window.Handle
            {
                get { return _handle; }
            }

            #endregion
        }

        #endregion
    }
}