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
                get
                {
                    return _handle;
                }
            }

            #endregion
        }

        #endregion
    }
}