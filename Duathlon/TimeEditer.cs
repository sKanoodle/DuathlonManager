using System;
using System.Windows;
using System.Windows.Controls;

namespace Duathlon
{
    public class TimeEditer
    {
        private TextBox _Number, _Hour, _Minute, _Second, _Millisecond;
        private Button _Save, _Cancel;
        private TimeCollector _Sender;
        private int _Index;

        /* UIElementCollection panel.Children
        **     [0] StackPanel
        **         [0] TextBox  startnumber
        **         [1] Border   space
        **         [2] TextBox  hour
        **         [3] TextBox  minute
        **         [4] TextBox  second
        **         [5] TextBox  millisecond
        **     [1] Border
        **     [2] StackPanel
        **         [1] Button   save
        **         [2] Button   cancel
        */
        public TimeEditer(StackPanel panel)
        {
            StackPanel temp = panel.Children[0] as StackPanel;
            _Number = temp.Children[0] as TextBox;
            _Hour = temp.Children[2] as TextBox;
            _Minute = temp.Children[3] as TextBox;
            _Second = temp.Children[4] as TextBox;
            _Millisecond = temp.Children[5] as TextBox;

            temp = panel.Children[2] as StackPanel;
            _Save = temp.Children[0] as Button;
            _Cancel = temp.Children[1] as Button;

            _Save.Click += Save_Click;
            _Cancel.Click += Cancel_Click;

            ChangeVisibility(Visibility.Hidden);
        }

        public void ChangeVisibility(Visibility visibility)
        {
            _Number.Visibility = visibility;
            _Hour.Visibility = visibility;
            _Minute.Visibility = visibility;
            _Second.Visibility = visibility;
            _Millisecond.Visibility = visibility;
            _Save.Visibility = visibility;
            _Cancel.Visibility = visibility;
        }

        public void Edit(TimeCollector sender, int index, TimeRecord record)
        {
            _Sender = sender;
            _Index = index;

            _Number.Text = record.StartNumber.ToString();
            _Hour.Text = record.Time.Hours.ToString("00");
            _Minute.Text = record.Time.Minutes.ToString("00");
            _Second.Text = record.Time.Seconds.ToString("00");
            _Millisecond.Text = record.Time.Milliseconds.ToString();

            ChangeVisibility(Visibility.Visible);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            int no, hour, minute, second, millisecond;
            TimeSpan time;
            
            if (int.TryParse(_Number.Text, out no) 
                && int.TryParse(_Hour.Text, out hour) 
                && int.TryParse(_Minute.Text, out minute) 
                && int.TryParse(_Second.Text, out second) 
                && int.TryParse(_Millisecond.Text, out millisecond))
                time = new TimeSpan(0, hour, minute, second, millisecond);
            else
            {
                MessageBox.Show("Eingabe kann nicht verarbeitet werden!");
                return;
            }

            _Sender.InsertEdited(_Index, new TimeRecord(no, time));
            
            ChangeVisibility(Visibility.Hidden);
            _Index = 0;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ChangeVisibility(Visibility.Hidden);
            _Index = 0;
        }
    }
}