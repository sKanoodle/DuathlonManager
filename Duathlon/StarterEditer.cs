using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Duathlon
{
    public class StarterEditer
    {
        public List<string> cbCompetitionPickContent = new List<string> { "Hauptwettkampf", "Jedermann-Wettkampf", "Kinderwettkampf" };
        public List<string> cbCompetitionContent = new List<string> { "Hauptwettkampf (Einzel)", "Hauptwettkampf (Staffel-Schwimmer)", "Jedermann-Wettkampf (Einzel)", "Jedermann-Wettkampf (Staffel-Schwimmer)", "Kinderwettkampf" };
        public List<int> cbNoContent = new List<int>();
        public List<int> cbYoBContent = new List<int>();
        public List<int> cbRelayYoBContent = new List<int>();
        private ComboBox _NumberPicker, _YoB, _Competition, _RelayYoB;
        private TextBox _Surname, _Name, _Club, _EMail, _RelayTeamName, _RelaySurname, _RelayName, _RelayClub, _RelayEMail;
        private CheckBox _IsMale, _IsFemale, _RelayIsMale, _RelayIsFemale;
        private Button _Save, _Cancel;
        private Label _LblRelaySurname, _LblRelayName, _LblRelayIsMale, _LblRelayYoB, _LblRelayClub, _LblRelayEMail;
        private bool _RelayIsEnabled;

        private StarterWrapper _Starters;
        private Visualization _Grid;
        //public bool Male { get; set; }
        //public bool RelayMale { get; set; }

        /* UIElementCollection controls.Children
        **     [0] Label    startnumber
        **     [1] Combobox startnumber
        **     [2] Label    surname
        **     [3] Textbox  surname
        **     [4] Label    name
        **     [5] TextBox  name
        **     [6] Label    is male
        **     [7] Checkbox is male
        **     [8] Checkbox is female    
        **     [9] Label    year of birth
        **     [10]Combobox year of birth
        **     [11]Label    club
        **     [12]Textbox  club
        **     [13]Label    e-mail
        **     [14]Textbox  e-mail
        **     [15]Label    competition
        **     [16]Combobox competition
        **     [17]Label    team name
        **     [18]Textbox  team name
        **     [19]Label    relay surname
        **     [20]Textbox  relay surname
        **     [21]Label    relay name
        **     [22]Textbox  relay name
        **     [23]Label    relay is male
        **     [24]Checkbox relay is male
        **     [25]Checkbox relay is female
        **     [26]Label    relay year of birth
        **     [27]Combobox relay year of birth
        **     [28]Label    relay club
        **     [29]Textbox  relay club
        **     [30]Label    relay e-mail
        **     [31]Textbox  relay e-mail
        **     [32]Button   save
        **     [33]Button   cancel
        */
        public StarterEditer(Grid controls, MenuItem edit, MenuItem delete, ref StarterWrapper starters, Visualization grid)
        {
            _NumberPicker = controls.Children[1] as ComboBox;
            _Surname = controls.Children[3] as TextBox;
            _Name = controls.Children[5] as TextBox;
            _IsMale = controls.Children[7] as CheckBox;
            _IsFemale = controls.Children[8] as CheckBox;
            _YoB = controls.Children[10] as ComboBox;
            _Club = controls.Children[12] as TextBox;
            _EMail = controls.Children[14] as TextBox;
            _Competition = controls.Children[16] as ComboBox;
            _RelayTeamName = controls.Children[18] as TextBox;
            _RelaySurname = controls.Children[20] as TextBox;
            _RelayName = controls.Children[22] as TextBox;
            _RelayIsMale = controls.Children[24] as CheckBox;
            _RelayIsFemale = controls.Children[25] as CheckBox;
            _RelayYoB = controls.Children[27] as ComboBox;
            _RelayClub = controls.Children[29] as TextBox;
            _RelayEMail = controls.Children[31] as TextBox;
            _Save = controls.Children[32] as Button;
            _Cancel = controls.Children[33] as Button;

            _LblRelaySurname = controls.Children[19] as Label;
            _LblRelayName = controls.Children[21] as Label;
            _LblRelayIsMale = controls.Children[23] as Label;
            _LblRelayYoB = controls.Children[26] as Label;
            _LblRelayClub = controls.Children[28] as Label;
            _LblRelayEMail = controls.Children[30] as Label;

            _Save.Click += Save_Click;
            _Cancel.Click += Cancel_Click;
            _IsMale.Click += IsMale_Click;
            _IsFemale.Click += IsFemale_Click;
            _RelayIsMale.Click += RelayIsMale_Click;
            _RelayIsMale.Click += RelayIsFemale_Click;

            edit.Click += Edit_Click;
            delete.Click += Delete_Click;

            _RelayTeamName.TextChanged += RelayTeamname_TextChanged;

            _Starters = starters;
            _Grid = grid;
            for (int i = _Starters.YoBLowerBoundary; i <= _Starters.CurrentYear; i++)
            {
                cbYoBContent.Add(i);
                cbRelayYoBContent.Add(i);
            }
            _YoB.ItemsSource = cbYoBContent;
            _RelayYoB.ItemsSource = cbRelayYoBContent;
            _NumberPicker.ItemsSource = cbNoContent;
            _Competition.ItemsSource = cbCompetitionContent;
            UpdatecbNo();
            EnableRelay(false);

            //IsMale.DataContext = Male;
            //IsFemale.DataContext = !Male;
            //RelayIsMale.DataContext = RelayMale;
            //RelayIsFemale.DataContext = !RelayMale;
        }

        public void SetToDefault()
        {
            UpdatecbNo();
            _Surname.Text = null;
            _Name.Text = null;
            _IsMale.IsChecked = false;
            _IsFemale.IsChecked = false;
            _YoB.SelectedIndex = -1;
            _Club.Text = null;
            _EMail.Text = null;
            _Competition.SelectedIndex = -1;
            _RelayTeamName.Text = null;
            _RelaySurname.Text = null;
            _RelayName.Text = null;
            _RelayIsMale.IsChecked = false;
            _RelayIsFemale.IsChecked = false;
            _RelayYoB.SelectedIndex = -1;
            _RelayClub.Text = null;
            _RelayEMail.Text = null;
        }

        public void EnableRelay(bool isEnabled)
        {
            _LblRelaySurname.IsEnabled = isEnabled;
            _RelaySurname.IsEnabled = isEnabled;
            _LblRelayName.IsEnabled = isEnabled;
            _RelayName.IsEnabled = isEnabled;
            _LblRelayIsMale.IsEnabled = isEnabled;
            _RelayIsMale.IsEnabled = isEnabled;
            _RelayIsFemale.IsEnabled = isEnabled;
            _LblRelayYoB.IsEnabled = isEnabled;
            _RelayYoB.IsEnabled = isEnabled;
            _LblRelayClub.IsEnabled = isEnabled;
            _RelayClub.IsEnabled = isEnabled;
            _LblRelayEMail.IsEnabled = isEnabled;
            _RelayEMail.IsEnabled = isEnabled;
            _RelayIsEnabled = isEnabled;
        }

        public void UpdatecbNo()
        {
            cbNoContent.Clear();
            for (int i = 0; i < _Starters.Length; i++)
            {
                if (false == _Starters[i].HasValue && false == Statics.MissingStartNumbers.Contains(i + 1))
                    cbNoContent.Add(i + 1);
            }
            _NumberPicker.Items.Refresh();
        }
        
        private void IsMale_Click(object sender, RoutedEventArgs e)
        {
            _IsFemale.IsChecked = !_IsMale.IsChecked.Value;
        }

        private void IsFemale_Click(object sender, RoutedEventArgs e)
        {
            _IsMale.IsChecked = !_IsFemale.IsChecked.Value;
        }

        private void RelayIsMale_Click(object sender, RoutedEventArgs e)
        {
            _RelayIsFemale.IsChecked = !_RelayIsMale.IsChecked.Value;
        }

        private void RelayIsFemale_Click(object sender, RoutedEventArgs e)
        {
            _RelayIsMale.IsChecked = !_RelayIsFemale.IsChecked.Value;
        }
        
        private void RelayTeamname_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_RelayTeamName.Text == "" && _RelayIsEnabled)
                EnableRelay(false);
            if (_RelayTeamName.Text != "" && !_RelayIsEnabled)
                EnableRelay(true);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            int startNumber;
            try
            {
                startNumber = _Grid.DataGridRows[_Grid.SelectedIndex].Nr - 1;
            }
            catch (Exception)
            {
                MessageBox.Show("Es wurde nichts ausgewählt!");
                return;
            }

            _Grid.SelectTab(TabItems.Edit);

            cbNoContent.Clear();
            for (int i = 0; i < _Starters.Length; i++)
            {
                if (!_Starters[i].HasValue)
                    cbNoContent.Add(i + 1);
            }
            cbNoContent.Add(startNumber + 1);
            _NumberPicker.Items.Refresh();
            _NumberPicker.SelectedIndex = _NumberPicker.Items.Count - 1;

            Starter starter = _Starters[startNumber];

            _Surname.Text = starter.Self.FirstName;
            _Name.Text = starter.Self.LastName;
            _IsMale.IsChecked = starter.Self.IsMale;
            _IsFemale.IsChecked = !starter.Self.IsMale;
            _YoB.SelectedIndex = Math.Max(0, starter.Self.YoB - _Starters.YoBLowerBoundary);
            _Club.Text = starter.Self.Club;
            _EMail.Text = starter.Self.E_Mail;

            if (Competition.MainsSingle.HasFlag(starter.Competition)) _Competition.SelectedIndex = 0;
            if (Competition.MainsRelay.HasFlag(starter.Competition)) _Competition.SelectedIndex = 1;
            if (Competition.SubsSingle.HasFlag(starter.Competition)) _Competition.SelectedIndex = 2;
            if (Competition.SubRelay.HasFlag(starter.Competition)) _Competition.SelectedIndex = 3;
            if (Competition.Children.HasFlag(starter.Competition)) _Competition.SelectedIndex = 4;


            _RelayTeamName.Text = starter.TeamName;
            if (starter.TeamName != "")
            {
                _RelaySurname.Text = starter.Partner.FirstName;
                _RelayName.Text = starter.Partner.LastName;
                _RelayIsMale.IsChecked = starter.Partner.IsMale;
                _RelayIsFemale.IsChecked = !starter.Partner.IsMale;
                _RelayYoB.SelectedIndex = Math.Max(0, starter.Partner.YoB - _Starters.YoBLowerBoundary);
                _RelayClub.Text = starter.Partner.Club;
                _RelayEMail.Text = starter.Partner.E_Mail;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            int startNumber;

            try
            {
                startNumber = _Grid.DataGridRows[_Grid.SelectedIndex].Nr - 1;
            }
            catch (Exception)
            {
                MessageBox.Show("Es wurde nichts ausgewählt!");
                return;
            }

            if (MessageBox.Show("Wollen Sie den Starter [" + (startNumber + 1) + "] " + _Starters[startNumber].Self.FirstName + " " + _Starters[startNumber].Self.LastName + " wirklich löschen?", "Achtung!", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            _Starters[startNumber] = new Starter();
            _Grid.Render(Competition.None);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            int newIndex = (int)_NumberPicker.SelectedItem - 1;
            int oldIndex = (int)_NumberPicker.Items[_NumberPicker.Items.Count - 1] - 1;

            if (oldIndex != newIndex)
            {
                _Starters[newIndex].SwimTime = _Starters[oldIndex].SwimTime;
                _Starters[newIndex].SwimPlace = _Starters[oldIndex].SwimPlace;
                _Starters[newIndex].RunPlace = _Starters[oldIndex].RunPlace;
                _Starters[newIndex].Time = _Starters[oldIndex].Time;
                _Starters[newIndex].Place = _Starters[oldIndex].Place;
                _Starters[oldIndex] = new Starter();
            }

            _Starters[newIndex].HasValue = true;
            _Starters[newIndex].Self.FirstName = _Surname.Text;
            _Starters[newIndex].Self.LastName = _Name.Text;
            _Starters[newIndex].Self.IsMale = (bool)_IsMale.IsChecked;
            _Starters[newIndex].Self.YoB = (int)_YoB.SelectedItem;
            _Starters[newIndex].Self.Club = _Club.Text;
            _Starters[newIndex].Self.E_Mail = _EMail.Text;
            _Starters[newIndex].TeamName = _RelayTeamName.Text;
            if (!(_RelayTeamName.Text == ""))
            {
                _Starters[newIndex].Partner.FirstName = _RelaySurname.Text;
                _Starters[newIndex].Partner.LastName = _RelayName.Text;
                _Starters[newIndex].Partner.IsMale = (bool)_RelayIsMale.IsChecked;
                _Starters[newIndex].Partner.YoB = (int)_RelayYoB.SelectedItem;
                _Starters[newIndex].Partner.Club = _RelayClub.Text;
                _Starters[newIndex].Partner.E_Mail = _RelayEMail.Text;
            }

            bool doThrowAway;
            _Starters[newIndex].Competition = StarterWrapper.GetCompetition(
                out doThrowAway,
                competition: (string)_Competition.SelectedItem, 
                isMale: _Starters[newIndex].Self.IsMale,
                optionalAge: _Starters.CurrentYear - _Starters[newIndex].Self.YoB, 
                optionalRelayIsMale: _Starters[newIndex].Partner.IsMale);
            if (doThrowAway)
                MessageBox.Show("Konnte keine Wettkampfklasse zuweisen", "Achtung!");

            _Grid.Render(Competition.None);
            SetToDefault();
            _Grid.SelectTab(TabItems.Overview);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SetToDefault();
        }
    }
}