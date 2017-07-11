using System;
using System.Collections.Generic;
using System.Windows.Controls;
using static Duathlon.Statics;

namespace Duathlon
{
    public class CBFilter
    {
        public string[] Cb1Content { get; set; } = new[] { "Alle Wettkämpfe", "Hauptwettkampf", "Jedermann-Wettkampf", "Kinderwettkampf" };
        public string[] Cb2Content { get; set; } = new[] { "Alle Teilnehmergruppen" };
        private ComboBox _Cb1, _Cb2;
        private Visualization _Grid;
        public int CurrentYear { get; set; } = DateTime.Now.Year;

        public CBFilter(Visualization grid, ComboBox cb1, ComboBox cb2)
        {
            _Grid = grid;
            _Cb1 = cb1;
            _Cb2 = cb2;

            _Cb1.SelectionChanged += Cb1_SelectionChanged;
            _Cb2.SelectionChanged += Cb2_SelectionChanged;

            _Cb1.ItemsSource = Cb1Content;
            _Cb2.ItemsSource = Cb2Content;

            Reset();
        }

        public void Cb1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_Cb1.SelectedIndex == 0)
            {
                Cb2Content = new[] { "Alle Teilnehmergruppen" };
                _Grid.Render(~Competition.None);
            }
            if (_Cb1.SelectedIndex == 1)
            {
                Cb2Content = new[]
                {
                    "Alle Teilnehmergruppen",
                    "Einzelwettkampf",
                    "männlich - Einzel",
                    "weiblich - Einzel",
                    "Staffelwettkampf",
                    "männlich - Staffel",
                    "weiblich - Staffel",
                    "mixed - Staffel"
                };
                _Grid.Render(Competition.Mains);
            }
            if (_Cb1.SelectedIndex == 2)
            {
                Cb2Content = new[]
                {
                    "Alle Teilnehmergruppen",
                    "Einzelwettkampf",
                    "männlich - Einzel",
                    "weiblich - Einzel",
                    "Staffelwettkampf"
                };
                _Grid.Render(Competition.Subs);
            }
            if (_Cb1.SelectedIndex == 3)
            {
                Cb2Content = new[]
                {
                    "Alle Teilnehmergruppen",
                    "männlich",
                    "weiblich",
                    $"{(CurrentYear-10).ToString()}/{(CurrentYear-9).ToString()} männlich",
                    $"{(CurrentYear-10).ToString()}/{(CurrentYear-9).ToString()} weiblich",
                    $"{(CurrentYear-12).ToString()}/{(CurrentYear-11).ToString()} männlich",
                    $"{(CurrentYear-12).ToString()}/{(CurrentYear-11).ToString()} weiblich",
                    $"{(CurrentYear-13).ToString()} männlich",
                    $"{(CurrentYear-13).ToString()} weiblich"
                };
                _Grid.Render(Competition.Children);
            }
            _Cb2.ItemsSource = Cb2Content;
            _Cb2.SelectedIndex = 0;
        }

        public void Cb2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_Cb1.SelectedIndex == 1)
                switch (_Cb2.SelectedIndex)
                {
                    case 0: { _Grid.Render(Competition.Mains); return; }
                    case 1: { _Grid.Render(Competition.MainsSingle); return; }
                    case 2: { _Grid.Render(Competition.MainSingleMale); return; }
                    case 3: { _Grid.Render(Competition.MainSingleFemale); return; }
                    case 4: { _Grid.Render(Competition.MainsRelay); return; }
                    case 5: { _Grid.Render(Competition.MainRelayMale); return; }
                    case 6: { _Grid.Render(Competition.MainRelayFemale); return; }
                    case 7: { _Grid.Render(Competition.MainRelayMixed); return; }
                }
            if (_Cb1.SelectedIndex == 2)
                switch (_Cb2.SelectedIndex)
                {
                    case 0: { _Grid.Render(Competition.Subs); return; }
                    case 1: { _Grid.Render(Competition.SubsSingle); return; }
                    case 2: { _Grid.Render(Competition.SubSingleMale); return; }
                    case 3: { _Grid.Render(Competition.SubSingleFemale); return; }
                    case 4: { _Grid.Render(Competition.SubRelay); return; }
                }
            if (_Cb1.SelectedIndex == 3)
                switch (_Cb2.SelectedIndex)
                {
                    case 0: { _Grid.Render(Competition.Children); return; }
                    case 1: { _Grid.Render(Competition.ChildrenMale); return; }
                    case 2: { _Grid.Render(Competition.ChildrenFemale); return; }
                    case 3: { _Grid.Render(Competition.ChildSmallMale); return; }
                    case 4: { _Grid.Render(Competition.ChildSmallFemale); return; }
                    case 5: { _Grid.Render(Competition.ChildMediumMale); return; }
                    case 6: { _Grid.Render(Competition.ChildMediumFemale); return; }
                    case 7: { _Grid.Render(Competition.ChildBigMale); return; }
                    case 8: { _Grid.Render(Competition.ChildBigFemale); return; }
                }
        }

        public void Reset()
        {
            _Cb1.SelectedIndex = 0;
        }
    }
}