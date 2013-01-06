namespace NBA_Stats_Tracker.Helper.Misc
{
    /// <summary>
    /// Implements a generic combo-box item with an IsEnabled property. 
    /// Used to create items in combo-boxes that can't be selected (e.g. group headers).
    /// </summary>
    public class ComboBoxItemWithIsEnabled
    {
        public ComboBoxItemWithIsEnabled(string item, bool isEnabled = true)
        {
            Item = item;
            IsEnabled = isEnabled;
        }

        public string Item { get; set; }
        public bool IsEnabled { get; set; }
    }
}