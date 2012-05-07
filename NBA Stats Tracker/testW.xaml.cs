using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NBA_Stats_Tracker
{
    /// <summary>
    /// Interaction logic for testW.xaml
    /// </summary>
    public partial class testW : Window
    {
        public testW()
        {
            InitializeComponent();
        }

        public testW(DataSet ds):this()
        {
            dataGrid1.DataContext = ds.Tables[0].DefaultView;
        }
    }
}
