using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Duathlon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StarterWrapper _Starters = StarterWrapper.CreateNew();
        private CBFilter _Filter;
        private TimeKeeper _Time;
        private Visualization _Grid;
        private StarterEditer _Edit;
        private Print _Print;
        private OrderDefinition _Orderer;
        
        public MainWindow()
        {
            InitializeComponent();
            InitializeObjects();
        }

        public void InitializeObjects()
        {
            Statics.LoadSettings();
            GoogleImporter.SetGoogleStuff(googleImportGrid, hlGoogleLink);
            _Grid = new Visualization(dgOverview, ref _Starters, tabControl);
            _Time = new TimeKeeper(ref _Starters, _Grid, tiTime.Content as Grid);
            _Filter = new CBFilter(_Grid, cb1, cb2);
            _Edit = new StarterEditer(tiEdit.Content as Grid, Edit, Delete, ref _Starters, _Grid);
            _Print = new Print(ref _Starters, cbPrintCompetition, cbPrintFilter, tbPrintSearch, cmdPrint);
            _Orderer = new OrderDefinition(cbOrderFirstName, cbOrderLastName, cbOrderGender, cbOrderYoB, cbOrderTeamName, cbOrderCompetition, cbOrderClub, cbOrderEMail, cmdOrderApply);
            StarterIO.NameChanged += ChangeProgramTitle;

            SetYear(_Starters.CurrentYear);
            
            imgNewDoc.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\images\newDoc.png"));
            imgSave.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\images\save.png"));
            imgSaveAs.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\images\saveAs.png"));
            imgOpen.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\images\open.png"));

            tbCurrentYear.Text = _Starters.CurrentYear.ToString();
            tbCurrentYear.TextChanged += CurrentYearInput_Changed;

            tbMissingStartNumbers.Text = Statics.MissingStartNumbers.Count == 0
                ? String.Empty
                : Statics.MissingStartNumbers.Select(i => i.ToString()).Aggregate((s, a) => $"{s},{a}");
            tbMissingStartNumbers.TextChanged += MissingStartNumberInput_Changed;
        }

        public void CurrentYearInput_Changed(object sender, TextChangedEventArgs e)
        {
            int year;
            if (false == int.TryParse(tbCurrentYear.Text, out year))
            {
                MessageBox.Show("kein gültiges jahr");
                return;
            }
            _Starters.CurrentYear = year;
            SetYear(year, doSetOnTextBox: false);
            StarterIO.IsSaved = false;
        }

        public void ChangeProgramTitle(object sender, EventArgs e)
        {
            wMainWindow.Title = (string)sender ?? "Duathlon Manager";
            _Print.ProgramName = (string)sender;
        }

        public void MissingStartNumberInput_Changed(object sender, TextChangedEventArgs e)
        {
            Statics.MissingStartNumbers = tbMissingStartNumbers.Text
                .Split(',')
                .Select(s => 
                {
                    int result;
                    if (int.TryParse(s, out result))
                        return result;
                    return -1;
                })
                .Where(i => i != -1)
                .ToList();
        }

        private void cmdTest_Click(object sender, RoutedEventArgs e)
        {
            
        }
        
        private void cmdImportFromGoogle_Click(object sender, RoutedEventArgs e)
        {
            if (StarterIO.IsDataExisting && MessageBox.Show("Voranmeldung bereits eingelesen. Wenn Sie fortfahren werden alle bisher eingegebenen Daten gelöscht!", "Achtung!", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            _Starters.ReadBulkSignUp(GoogleImporter.GrabEntries());
            StarterIO.IsDataExisting = true;
            StarterIO.IsSaved= false;
            _Edit.UpdatecbNo();
            _Grid.Render(Competition.None);
            tabControl.SelectedItem = tiOverview;
        }

        private void cmdNewDoc_Click(object sender, RoutedEventArgs e)
        {
            StarterWrapper temp = StarterIO.NewDoc(_Starters);
            if (temp != null)
                StarterWrapper.CopyTo(_Starters, temp);
            SetYear(_Starters.CurrentYear);
            _Grid.Render(~Competition.None);
            _Edit.SetToDefault();
            _Filter.Reset();
        }

        private void cmdSave_Click(object sender, RoutedEventArgs e)
        {
            StarterIO.Save(_Starters);
        }

        private void cmdSaveAs_Click(object sender, RoutedEventArgs e)
        {
            StarterIO.SaveAs(_Starters);
        }

        private void cmdOpen_Click(object sender, RoutedEventArgs e)
        {
            StarterWrapper newData = StarterIO.Open(_Starters);
            if (newData == null)
                return;
            StarterWrapper.CopyTo(_Starters, newData);
            _Grid.Render(~Competition.None);
            SetYear(_Starters.CurrentYear);
            _Filter.Reset();
            _Edit.SetToDefault();
            tabControl.SelectedItem = tiOverview;
        }

        private void SetYear(int year, bool doSetOnTextBox = true)
        {
            _Filter.CurrentYear = year;
            _Print.CurrentYear = year;

            if (doSetOnTextBox)
                tbCurrentYear.Text = year.ToString();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Statics.SaveSettings();
            e.Cancel = StarterIO.CanWindowBeClosed(_Starters);
        }
    }
}