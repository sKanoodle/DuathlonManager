using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using static Duathlon.Statics;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using System.Diagnostics;

namespace Duathlon
{
    class Print
    {
        public int CurrentYear { get; set; } = DateTime.Now.Year;
        public string ProgramName;
        private ComboBox _PrintMode, _Filter;
        private TextBox _Search;
        private Button _Print;
        private StarterWrapper _Starters;
        private Printer _Printer;
        private string _TimeFormat = "hh':'mm':'ss";
        private Func<StarterWrapper, IEnumerable<string>> _CompletePrintList;

        public Print(ref StarterWrapper starters, ComboBox printMode, ComboBox filter, TextBox search, Button print)
        {
            GetDisplayText = s => $"{_Starters.GetIndex(s) + 1:000}-{(s.TeamName == String.Empty ? $"{s.Self.FirstName} {s.Self.LastName}" : $"{s.Self.LastName} {s.Partner.LastName}")}";

            _Starters = starters;
            _Printer = new Printer(ref _Starters);
            _PrintMode = printMode;
            _Filter = filter;
            _Search = search;
            _Print = print;

            _PrintMode.SelectionChanged += PrintMode_SelectionChanged;
            _Print.Click += Print_Click;
            _Search.TextChanged += Search_TextChanged;

            _PrintMode.ItemsSource = new[] { "Voranmeldung", "Ergebnisse", "Urkunde" };
            _PrintMode.SelectedIndex = 0;

            _CompletePrintList = (sw) => new[]
            {
                CompetitionLocalization(Competition.Mains, CurrentYear),
                CompetitionLocalization(Competition.Subs, CurrentYear),
                CompetitionLocalization(Competition.Children, CurrentYear),
            }
            .Concat(
                sw.Starters
                .Where(s => s.HasValue == true)
                .Select(GetDisplayText)
            );
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = _Search.Text;
            if (_PrintMode.SelectedIndex != 2)
                return;

            if (String.IsNullOrWhiteSpace(text))
                _Filter.ItemsSource = _CompletePrintList(_Starters);
            else
                _Filter.ItemsSource = _Starters.Starters
                    .Where(s => s.HasValue == true && s.Contains(text))
                    .Select(GetDisplayText);

            _Filter.SelectedIndex = 0;
        }

        private void PrintMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (_PrintMode.SelectedIndex)
            {
                case 0:
                    PrepareSignUpPrint();
                    break;
                case 1:
                    PrepareResultPrint();
                    break;
                case 2:
                    PrepareCertificatePrint();
                    break;
            }
        }

        private void PrepareSignUpPrint()
        {
            _Search.IsEnabled = false;
            _Search.Text = String.Empty;
            _Filter.ItemsSource = new[] { CompetitionLocalization(Competition.Mains, CurrentYear), CompetitionLocalization(Competition.Subs, CurrentYear), CompetitionLocalization(Competition.Children, CurrentYear) };
            _Filter.SelectedIndex = 0;
            PrintAction = () =>
                PrintSignUp(new[] { Competition.Mains, Competition.Subs, Competition.Children }[_Filter.SelectedIndex]);
        }

        private void PrepareResultPrint()
        {
            _Search.IsEnabled = false;
            _Search.Text = String.Empty;
            _Filter.ItemsSource = new[]
            {
                CompetitionLocalization(Competition.Mains, CurrentYear),
                CompetitionLocalization(Competition.Subs, CurrentYear),
                CompetitionLocalization(Competition.Children, CurrentYear),
                CompetitionLocalization(Competition.MainSingleMale, CurrentYear),
                CompetitionLocalization(Competition.MainSingleFemale, CurrentYear),
                CompetitionLocalization(Competition.MainRelayMale, CurrentYear),
                CompetitionLocalization(Competition.MainRelayFemale, CurrentYear),
                CompetitionLocalization(Competition.MainRelayMixed, CurrentYear),
                CompetitionLocalization(Competition.SubSingleMale, CurrentYear),
                CompetitionLocalization(Competition.SubSingleFemale, CurrentYear),
                CompetitionLocalization(Competition.SubRelay, CurrentYear),
                CompetitionLocalization(Competition.ChildSmallMale, CurrentYear),
                CompetitionLocalization(Competition.ChildSmallFemale, CurrentYear),
                CompetitionLocalization(Competition.ChildMediumMale, CurrentYear),
                CompetitionLocalization(Competition.ChildMediumFemale, CurrentYear),
                CompetitionLocalization(Competition.ChildBigMale, CurrentYear),
                CompetitionLocalization(Competition.ChildBigFemale, CurrentYear)
            };
            _Filter.SelectedIndex = 0;
            PrintAction = () =>
            {
                if (_Filter.SelectedIndex > 2)
                    PrintResults(new[]
                    {
                        Competition.None,
                        Competition.None,
                        Competition.None,
                        Competition.MainSingleMale,
                        Competition.MainSingleFemale,
                        Competition.MainRelayMale,
                        Competition.MainRelayFemale,
                        Competition.MainRelayMixed,
                        Competition.SubSingleMale,
                        Competition.SubSingleFemale,
                        Competition.SubRelay,
                        Competition.ChildSmallMale,
                        Competition.ChildSmallFemale,
                        Competition.ChildMediumMale,
                        Competition.ChildMediumFemale,
                        Competition.ChildBigMale,
                        Competition.ChildBigFemale
                    }[_Filter.SelectedIndex]);
                else if (_Filter.SelectedIndex == 0)
                {
                    PrintResults(Competition.Mains);
                }
                else if (_Filter.SelectedIndex == 1)
                {
                    PrintResults(Competition.Subs);
                }
                else if (_Filter.SelectedIndex == 2)
                {
                    PrintResults(Competition.Children);
                }
            };
        }

        private void PrepareCertificatePrint()
        {
            _Search.IsEnabled = true;
            _Search.Text = String.Empty;
            _Filter.ItemsSource = _CompletePrintList(_Starters);
            
            _Filter.SelectedIndex = 0;
            PrintAction = () =>
            {
                if (Int32.TryParse(_Filter.SelectedItem.ToString().Split('-')[0], out int startNumber))
                    PrintCertificate(_Starters[startNumber - 1]);
                else if (_Filter.SelectedIndex == 0)
                    PrintCertificate(_Starters.Starters.Where(s => Competition.Mains.HasFlag(s.Competition) && s.Place > 0 && s.Place < 4));
                else if (_Filter.SelectedIndex == 1)
                    PrintCertificate(_Starters.Starters.Where(s => Competition.Subs.HasFlag(s.Competition) && s.Place > 0 && s.Place < 4));
                else if (_Filter.SelectedIndex == 2)
                    PrintCertificate(_Starters.Starters.Where(s => Competition.Children.HasFlag(s.Competition) && s.Place > 0 && s.Place < 4));

                else
                    MessageBox.Show("Error while parsing starter to print");
            };
        }

        private Func<Starter, string> GetDisplayText;

        private Action PrintAction;

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            PrintAction();
        }

        private void PrintSignUp(Competition competitionFilter)
        {
            _Printer.Print(new PrintJob<Starter>(
                sa => sa.Where(s => competitionFilter.HasFlag(s.Competition)).ToArray(),
                $"Voranmeldung für {CompetitionLocalization(competitionFilter, CurrentYear)} {ProgramName}",
                new PrintColumn<Starter>("Nr", s => (_Starters.GetIndex(s) + 1).ToString(), doAlignRight: true),
                new PrintColumn<Starter>("Vorname", s => String.IsNullOrWhiteSpace(s.TeamName) ? s.Self.FirstName : s.Self.LastName),
                new PrintColumn<Starter>("Nachname", s => String.IsNullOrWhiteSpace(s.TeamName) ? s.Self.LastName : s.Partner.LastName),
                new PrintColumn<Starter>("m\\w", s => s.Self.IsMale == true ? "m" : "w"),
                new PrintColumn<Starter>("JG", s => s.Self.YoB.ToString()),
                new PrintColumn<Starter>("Verein", s => s.Self.Club)));
        }

        private void PrintResults(Competition competitionFilter)
        {
            Func<Competition, PrintJob<Starter>> getJob = c => new PrintJob<Starter>(
                sa => sa.Where(s => c == s.Competition && s.Place > 0).OrderBy(s => s.Place).ToArray(),
                $"Ergebnisse für {CompetitionLocalization(c, CurrentYear)} {ProgramName}",
                new PrintColumn<Starter>("Gesamtplatz", s => s.Place.ToString(), doAlignRight: true),
                new PrintColumn<Starter>("Nr", s => (_Starters.GetIndex(s) + 1).ToString(), doAlignRight: true),
                new PrintColumn<Starter>("Vorname", s => String.IsNullOrWhiteSpace(s.TeamName) ? s.Self.FirstName : s.Self.LastName),
                new PrintColumn<Starter>("Nachname", s => String.IsNullOrWhiteSpace(s.TeamName) ? s.Self.LastName : s.Partner.LastName),
                new PrintColumn<Starter>("m\\w", s => s.Self.IsMale ? "m" : "w"),
                new PrintColumn<Starter>("JG", s => s.Self.YoB.ToString()),
                new PrintColumn<Starter>("Verein", s => s.Self.Club),
                new PrintColumn<Starter>("SW Zeit", s => s.SwimTime.ToString(_TimeFormat)),
                new PrintColumn<Starter>("SW Pl", s => s.SwimPlace.ToString(), doAlignRight: true),
                new PrintColumn<Starter>("L Zeit", s => s.RunTime.ToString(_TimeFormat)),
                new PrintColumn<Starter>("L Pl", s => s.RunPlace.ToString(), doAlignRight: true),
                new PrintColumn<Starter>("Total", s => s.Time.ToString(_TimeFormat)));

            List<PrintJob<Starter>> printJobs = new List<PrintJob<Starter>>();

            foreach (Competition competition in Enum.GetValues(typeof(Competition)).Cast<Competition>().Where(c => c != Competition.None && (c & c - 1) == 0))
                if ((competitionFilter & competition) == competition)
                    printJobs.Add(getJob(competition));

            _Printer.MultiPrint(printJobs.ToArray());
        }

        private void PrintCertificate(IEnumerable<Starter> starters)
        {
            System.Windows.Forms.PrintDialog printDialog = new System.Windows.Forms.PrintDialog();
            if (printDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            foreach (Starter starter in starters)
            {
                PrintCertificate(starter, printDialog.PrinterSettings.PrinterName);
            }
        }

        private void PrintCertificate(Starter starter, string printer = null)
        {
            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "template.docx");
            string temporaryPath = Path.GetTempFileName();
            string documentPath = Path.ChangeExtension(temporaryPath, "docx");
            File.Copy(templatePath, documentPath);
            FindAndReplace(documentPath,
                new StringReplacement("Place", $"{starter.Place}. Platz"),
                new StringReplacement("Competition1", CompetitionLocalizationHead(starter.Competition)),
                new StringReplacement("Competition2", CompetitionLocalizationTail(starter.Competition, CurrentYear)),
                new StringReplacement("SFirstName", starter.Self.FirstName),
                new StringReplacement("SLastName", starter.Self.LastName),
                new StringReplacement("RFirstName", starter.Partner.FirstName),
                new StringReplacement("RLastName", starter.Partner.LastName),
                new StringReplacement("SwimTime", starter.SwimTime.ToString(_TimeFormat)),
                new StringReplacement("RunTime", starter.RunTime.ToString(_TimeFormat)),
                new StringReplacement("TotalTime", starter.Time.ToString(_TimeFormat))
            );
            int copyCount = String.IsNullOrWhiteSpace(starter.TeamName) ? 1 : 2;
            StartPrintingProcess(documentPath, printer, copyCount);
            File.Delete(documentPath);
        }

        private static void FindAndReplace(string docPath, params StringReplacement[] replacements)
        {
            using (WordprocessingDocument document = WordprocessingDocument.Open(docPath, true))
            {
                string docText = null;
                using (StreamReader reader = new StreamReader(document.MainDocumentPart.GetStream()))
                {
                    docText = reader.ReadToEnd();
                }

                foreach (StringReplacement replacement in replacements)
                    docText = docText.Replace(replacement.OldString, replacement.NewString);

                using (StreamWriter writer = new StreamWriter(document.MainDocumentPart.GetStream(FileMode.Create)))
                    writer.Write(docText);
            }
        }

        private static void StartPrintingProcess(string docPath, string printer, int copyCount = 1)
        {
            System.Windows.Forms.PrintDialog printDialog = new System.Windows.Forms.PrintDialog();
            if (printer == null)
                if (printDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
            ProcessStartInfo info = new ProcessStartInfo(docPath);
            info.Arguments = $"\"{printer ?? printDialog.PrinterSettings.PrinterName}\"";
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.Verb = "Printto";
            info.UseShellExecute = true;

            while (copyCount-- > 0)
            {
                Process process = Process.Start(info);
                process.WaitForExit();
            }
        }
    }

    internal class StringReplacement
    {
        public string OldString { get; set; }
        public string NewString { get; set; }

        public StringReplacement(string oldString, string newString)
        {
            OldString = oldString;
            NewString = newString;
        }
    }
}
