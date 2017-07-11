using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Duathlon
{
    public class TimeCollector
    {
        private TextBox _NumberIn;
        private Button _Add, _Edit, _Delete;
        private List<TimeRecord> _TimesSource = new List<TimeRecord>();
        private ListBox _Times;
        private Stopwatch _Watch;
        private TimeEditer _Editer;
        private TextBlock _Count;

        /* UIElementCollection panel.Children
        **     [0] StackPanel
        **         [0] TextBox  startnumber to add to the list
        **         [1] Border   space
        **         [2] Button   add
        **         [3] Border   space
        **         [4] Button   edit
        **         [5] Button   delete
        *          [6] Border   space
        *          [7] TextBlock  count of times
        **     [1] ListBox      time elements with start number and time
        */
        public TimeCollector(StackPanel panel, Stopwatch watch, TimeEditer editer)
        {
            _Watch = watch;
            _Editer = editer;

            StackPanel temp = panel.Children[0] as StackPanel;
            _NumberIn = temp.Children[0] as TextBox;
            _Add = temp.Children[2] as Button;
            _Edit = temp.Children[4] as Button;
            _Delete = temp.Children[5] as Button;
            _Count = temp.Children[7] as TextBlock;
            _Times = panel.Children[1] as ListBox;

            _NumberIn.KeyDown += NumberIn_KeyDown;
            _Add.Click += Add_Click;
            _Edit.Click += Edit_Click;
            _Delete.Click += Delete_Click;
        }

        public void Reset()
        {
            _TimesSource = new List<TimeRecord>();
            RefreshTimeList();
        }

        public void InsertEdited(int index, TimeRecord record)
        {
            _TimesSource[index] = record;
            RefreshTimeList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            int temp;
            int no = int.TryParse(_NumberIn.Text, out temp) ? temp : 0;
            
            _TimesSource.Add(new TimeRecord(no, _Watch.Elapsed));
            RefreshTimeList();
            _NumberIn.Text = String.Empty;
            _NumberIn.Focus();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            int index = _Times.SelectedIndex;
            if (index == -1)
                return;

            _Editer.Edit(this, index, _TimesSource[index]);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            int no = _Times.SelectedIndex;
            if (no == -1)
                return;

            if (MessageBox.Show(String.Format("Wollen Sie den Eintrag [{0}] {1} wirklich löschen?", _TimesSource[no].StartNumber, _TimesSource[no].Time.ToString(@"hh\:mm\:ss\.ff")), "Achtung!", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            _TimesSource.RemoveAt(no);
            RefreshTimeList();
        }

        private void NumberIn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Add_Click(sender, new RoutedEventArgs());
        }

        private void RefreshTimeList()
        {
            _Times.Items.Clear();
            foreach (TimeRecord record in _TimesSource)
            {
                _Times.Items.Add(String.Format("{0}\t{1:hh\\:mm\\:ss\\.ff}", record.StartNumber, record.Time));
            }
            _Count.Text = _TimesSource.Count.ToString();
        }

        public TimeRecord[] GetTimes()
        {
            return _TimesSource.ToArray();
        }
    }

    public class TimeRecord
    {
        public int StartNumber { get; set; }
        public TimeSpan Time { get; set; }

        public TimeRecord(int startNumber, TimeSpan time)
        {
            StartNumber = startNumber;
            Time = time;
        }
    }
}