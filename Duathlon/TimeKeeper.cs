using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Linq;

namespace Duathlon
{
    class TimeKeeper
    {
        private ComboBox _CompetitionPicker;
        private Label _WatchDisplay;
        private DispatcherTimer _DispatcherTimer = new DispatcherTimer();
        private Stopwatch _Watch = new Stopwatch();
        private TimeCollector _SwimTimes, _TotalTimes;
        private StarterWrapper _Starters;
        private Visualization _Grid;
        private Competition Competition;

        /*  UIElementCollection in grid.Children
        **      [0] ComboBox competition picker
        **      [1] Label time since start
        **      [2] StackPanel buttons for the stopwatch
        **          [0] Button start
        **          [1] Button stop
        **          [2] Button reset
        **      [3] StackPanel time collector swimming
        **      [4] StackPanel time collector total
        **      [5] StackPanel time editer
        **          [4] Button save times to starter list
        */
        public TimeKeeper(ref StarterWrapper starters, Visualization visualization, Grid grid)
        {
            _Grid = visualization;
            _Starters = starters;
            _CompetitionPicker = grid.Children[0] as ComboBox;
            _WatchDisplay = grid.Children[1] as Label;
            TimeEditer editer = new TimeEditer(grid.Children[5] as StackPanel);
            _SwimTimes = new TimeCollector(grid.Children[3] as StackPanel, _Watch, editer);
            _TotalTimes = new TimeCollector(grid.Children[4] as StackPanel, _Watch, editer);

            _DispatcherTimer.Tick += dispatcherTimer_Tick;
            _DispatcherTimer.Interval = new TimeSpan(500000);

            ((grid.Children[5] as StackPanel).Children[4] as Button).Click += SaveTimes_Click;
            StackPanel buttons = grid.Children[2] as StackPanel;
            (buttons.Children[0] as Button).Click += Start_Click;
            (buttons.Children[1] as Button).Click += Stop_Click;
            (buttons.Children[2] as Button).Click += Reset_Click;

            _CompetitionPicker.ItemsSource = new[] 
            {
                Statics.CompetitionLocalization(Competition.Mains, 0),
                Statics.CompetitionLocalization(Competition.Subs, 0),
                Statics.CompetitionLocalization(Competition.Children, 0),
            };
            _CompetitionPicker.SelectionChanged += CompetitionPicker_SelectionChanged;
            _CompetitionPicker.SelectedIndex = 0;
        }

        private void CompetitionPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (_CompetitionPicker.SelectedIndex)
            {
                case 0:
                    Competition = Competition.Mains;
                    break;
                case 1:
                    Competition = Competition.Subs;
                    break;
                case 2:
                    Competition = Competition.Children;
                    break;
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            _WatchDisplay.Content = _Watch.Elapsed.ToString(@"hh\:mm\:ss\.ff");
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            _Watch.Start();
            _DispatcherTimer.Start();
            _CompetitionPicker.IsEnabled = false;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _Watch.Stop();
            _DispatcherTimer.Stop();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Wenn Sie fortfahren werden alle Eingaben gelöscht! Fortfahren?", "Achtung!", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            if (_DispatcherTimer.IsEnabled)
                _Watch.Restart();
            else
            {
                _Watch.Reset();
                _WatchDisplay.Content = _Watch.Elapsed.ToString(@"hh\:mm\:ss\.ff");
            }

            _SwimTimes.Reset();
            _TotalTimes.Reset();

            _CompetitionPicker.IsEnabled = true;
        }
        
        private void SaveTimes_Click(object sender, RoutedEventArgs e)
        {
            TimeRecord[] swimTimes = _SwimTimes.GetTimes();
            TimeRecord[] totalTimes = _TotalTimes.GetTimes();

            //check for wrong startnumbers
            foreach (TimeRecord record in swimTimes)
                if (false == IsStartnumberPossible(record))
                    return;

            foreach (TimeRecord record in totalTimes)
                if (false == IsStartnumberPossible(record))
                    return;

            //assign times
            foreach (TimeRecord record in swimTimes)
                _Starters[record.StartNumber - 1].SwimTime = record.Time;

            foreach (TimeRecord record in totalTimes)
                _Starters[record.StartNumber - 1].Time = record.Time;

            StarterIO.IsSaved = false;
            _Grid.CreateRankings();
            _Grid.Render(~Competition.None);
            _Grid.SelectTab(TabItems.Overview);
        }

        private bool IsStartnumberPossible(TimeRecord record)
        {
            if (record.StartNumber >= _Starters.Starters.Length)
            {
                MessageBox.Show($"{record.StartNumber} außerhalb des Startnummernbereichs. Es wurden keine Werte übernommen.");
                return false;
            }
            if (false == _Starters[record.StartNumber - 1].HasValue)
            {
                MessageBox.Show($"{record.StartNumber} ist nicht registriert. Es wurden keine Werte übernommen.");
                return false;
            }
            if (false == Competition.HasFlag(_Starters[record.StartNumber - 1].Competition))
            {
                MessageBox.Show($"{record.StartNumber} ist für einen anderen Wettkampf eingetragen. Es wurden keine Werte übernommen.");
                return false;
            }
            return true;
        }
    }
}