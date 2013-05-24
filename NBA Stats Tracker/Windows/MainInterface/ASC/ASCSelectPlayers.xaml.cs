using System;
using System.Collections.Generic;
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

namespace NBA_Stats_Tracker.Windows.MainInterface.ASC
{
    /// <summary>
    /// Interaction logic for ASCSelectPlayers.xaml
    /// </summary>
    public partial class ASCSelectPlayers : Window
    {
        private List<KeyValuePair<int, string>> _list;
        private List<int> _ids;

        public ASCSelectPlayers()
        {
            InitializeComponent();
        }

        public ASCSelectPlayers(IEnumerable<KeyValuePair<int, string>> list, IEnumerable<int> ids) : this()
        {
            _list = list.ToList();
            _ids = ids.ToList();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPlayers.SelectedIndex == -1)
            {
                return;
            }
            if (lstSelectedPlayers.Items.Contains(cmbPlayers.SelectedItem))
            {
                return;
            }
            lstSelectedPlayers.Items.Add(cmbPlayers.SelectedItem);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstSelectedPlayers.SelectedIndex == -1)
            {
                return;
            }
            var list = lstSelectedPlayers.SelectedItems.Cast<KeyValuePair<int, string>>().ToList();
            foreach (var item in list)
            {
                lstSelectedPlayers.Items.Remove(item);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (lstSelectedPlayers.Items.Count > 10)
            {
                MessageBox.Show(
                    "You can't have more than 10 players on the floor at the same time. You currently have "
                    + lstSelectedPlayers.Items.Count + ".");
                return;
            }
            AdvancedStatCalculatorWindow.PlayersOnTheFloor = lstSelectedPlayers.Items.Cast<KeyValuePair<int, string>>().Select(item => item.Key).ToList();
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbPlayers.ItemsSource = _list;
            foreach (var id in _ids)
            {
                try
                {
                    lstSelectedPlayers.Items.Add(_list.Single(item => item.Key == id));
                }
                catch
                {
                    Console.WriteLine("ASCSelectPlayers: Entry with ID " + id + " not found in list.");
                }
            }
        }
    }
}