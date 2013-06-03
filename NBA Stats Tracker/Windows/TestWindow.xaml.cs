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

namespace NBA_Stats_Tracker.Windows
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Windows.Media.Imaging;

    #endregion

    /// <summary>Used to debug and try out new features before rolling them out to release.</summary>
    public partial class TestWindow
    {
        public TestWindow()
        {
            InitializeComponent();

            var rng = new Random();

            var nameList = new List<string> { "Default", "Red", "Grey", "Blue" };

            var list = new List<Image> { Properties.Resources.Default_001 };
            for (var i = 2; i <= 15; i++)
            {
                list.Add(
                    (Image) Properties.Resources.ResourceManager.GetObject(nameList[rng.Next(4)] + "_0" + String.Format("{0:00}", i)));
            }

            Image canvas = new Bitmap(478, 397);
            var frame = new Rectangle(0, 0, 478, 397);
            var g = Graphics.FromImage(canvas);

            list.ForEach(o => g.DrawImage(o, frame, frame, GraphicsUnit.Pixel));

            var tempFile = App.AppTempPath + "\\temp.png";
            File.Delete(tempFile);
            canvas.Save(tempFile);

            var bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.CacheOption = BitmapCacheOption.OnLoad;
            bmi.UriSource = new Uri(tempFile, UriKind.Absolute);
            bmi.EndInit();

            imageControl.Source = bmi;

            g.Dispose();
            canvas.Dispose();
        }
    }
}