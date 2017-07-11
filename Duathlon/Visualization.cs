using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Duathlon
{
    public class Visualization
    {
        private StarterWrapper _Starters;
        private DataGrid _Display;
        public int SelectedIndex { get { return _Display.SelectedIndex; } }
        private Competition _CurrentDataGridScope;
        public MyRow[] DataGridRows { get; private set; }
        private TabControl _TC;

        public Visualization(DataGrid grid, ref StarterWrapper starters, TabControl tc)
        {
            _TC = tc;
            _Display = grid;
            _Starters = starters;
        }

        public void SelectTab(TabItems item)
        {
            _TC.SelectedIndex = (int)item;
        }

        public void Render(Competition competitions)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(_Display.ItemsSource);
            if (view != null && view.SortDescriptions != null)
            {
                view.SortDescriptions.Clear();
                foreach (var column in _Display.Columns)
                {
                    column.SortDirection = null;
                }
            }

            if (competitions == Competition.None)
                competitions = _CurrentDataGridScope;
            else
                _CurrentDataGridScope = competitions;

            DataGridRows = _Starters.Starters
                .Where(s => s.HasValue && competitions.HasFlag(s.Competition))
                .Select(s => new MyRow(s, _Starters.GetIndex(s) + 1))
                .ToArray();
            _Display.ItemsSource = DataGridRows;
        }

        public void CreateRankings()
        {
            foreach (Competition competition in Enum.GetValues(typeof(Competition)))
                if (competition != Competition.None && (competition & (competition - 1)) == 0)
                    CreateRankingsForCompetition(competition);
        }

        public void CreateRankingsForCompetition(Competition competition)
        {
            CreateRankingSwim(competition);
            CreateRankingRun(competition);
            CreateRankingOverall(competition);
        }

        private void CreateRankingSwim(Competition competition)
        {
            CreateRanking(competition: competition, 
                getTime: startNumber => _Starters[startNumber].SwimTime, 
                assignPlace: (startNumber, place) => _Starters[startNumber].SwimPlace = place);
        }

        private void CreateRankingRun(Competition competition)
        {
            CreateRanking(competition: competition, 
                getTime: startNumber => _Starters[startNumber].RunTime, 
                assignPlace: (startNumber, place) => _Starters[startNumber].RunPlace = place);
        }

        private void CreateRankingOverall(Competition competition)
        {
            CreateRanking(competition: competition, 
                getTime: startNumber => _Starters[startNumber].Time, 
                assignPlace: (startNumber, place) => _Starters[startNumber].Place = place);
        }

        private void CreateRanking(Competition competition, Func<int, TimeSpan> getTime, Action<int, int> assignPlace)
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < _Starters.Length; i++)
            {
                if (!_Starters[i].HasValue)
                    continue;
                if (_Starters[i].Competition == competition && getTime(i).Ticks != 0)
                    indexes.Add(i);
            }
            InsertionSort(ref indexes, (x, y) => getTime(x).CompareTo(getTime(y)));
            for (int i = 0; i < indexes.Count; i++)
                assignPlace(indexes[i], i + 1);
        }

        private void InsertionSort(ref List<int> input, Func<int, int, int> comparer)
        {
            for (int i = 0; i < input.Count - 1; i++)
            {
                int j = i + 1;

                while (j > 0)
                {
                    if (comparer(input[j - 1], input[j]) > 0)
                    {
                        int temp = input[j - 1];
                        input[j - 1] = input[j];
                        input[j] = temp;

                    }
                    j--;
                }
            }
        }

    }
}
