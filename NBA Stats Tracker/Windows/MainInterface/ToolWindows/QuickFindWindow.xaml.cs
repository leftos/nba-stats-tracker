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

namespace NBA_Stats_Tracker.Windows.MainInterface.ToolWindows
{
    using System.ComponentModel;

    using LeftosCommonLibrary;

    using NBA_Stats_Tracker.Annotations;
    using NBA_Stats_Tracker.Data.Other;

    /// <summary>
    /// Interaction logic for QuickFindWindow.xaml
    /// </summary>
    public partial class QuickFindWindow : Window, INotifyPropertyChanged
    {
        public QuickFindWindow()
        {
            InitializeComponent();

            Height = Tools.GetRegistrySetting("QFWHeight", Height);
            Width = Tools.GetRegistrySetting("QFWWidth", Width);
            Top = Tools.GetRegistrySetting("QFWY", Top);
            Left = Tools.GetRegistrySetting("QFWX", Left);
        }

        public string Query
        {
            get { return _query; }
            set
            {
                _query = value;
                OnPropertyChanged("Query");
                if (String.IsNullOrWhiteSpace(_query))
                {
                    lstResults.ItemsSource = null;
                    return;
                }
                var parts = _query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var results = parts.Aggregate(
                    MainWindow.SearchCache,
                    (current, part) =>
                    current.Where(item => item.Description.ToLowerInvariant().Contains(part.ToLowerInvariant())).ToList());
                lstResults.ItemsSource = results;
                lstResults.SelectedIndex = 0;
            }
        }

        private string _query;
        private bool _userSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            txtQuery.Focus();

            _userSelected = false;
        }

        private void window_Closing(object sender, CancelEventArgs e)
        {
            Tools.SetRegistrySetting("QFWHeight", Height);
            Tools.SetRegistrySetting("QFWWidth", Width);
            Tools.SetRegistrySetting("QFWY", Top);
            Tools.SetRegistrySetting("QFWX", Left);

            DialogResult = _userSelected;
        }

        private void result_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstResults.SelectedItem == null)
            {
                return;
            }

            selectAndClose();
        }

        private void selectAndClose()
        {
            SelectedItem = (SearchItem) (lstResults.SelectedItem);
            _userSelected = true;

            Close();
        }

        public static SearchItem SelectedItem { get; private set; }

        private void lstResults_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && lstResults.SelectedItem != null)
            {
                e.Handled = true;
                selectAndClose();
            }
        }

        private void lstResults_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var l = (ListBox)sender;
            if (l.Items.Count == 0)
            {
                return;
            }
            if (l.SelectedIndex == -1)
            {
                l.SelectedIndex = 0;
            }
            var lbi = l.ItemContainerGenerator.ContainerFromIndex(l.SelectedIndex) as ListBoxItem;

            if (lbi != null)
            {
                lbi.Focus();
            }
        }

        private void txtQuery_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (lstResults.Items.Count == 0)
                {
                    return;
                }
                if (lstResults.SelectedIndex == -1)
                {
                    lstResults.SelectedIndex = 0;
                }
                selectAndClose();
            }
        }
    }
}