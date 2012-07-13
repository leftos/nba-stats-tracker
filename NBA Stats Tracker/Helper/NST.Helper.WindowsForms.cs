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

using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace NBA_Stats_Tracker.Helper
{
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