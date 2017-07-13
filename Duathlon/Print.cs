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
            GetStarterDisplayText = s => $"{_Starters.GetIndex(s) + 1:000}-{(s.TeamName == String.Empty ? $"{s.Self.FirstName} {s.Self.LastName}" : $"{s.Self.LastName} {s.Partner.LastName}")}";

            _Starters = starters;
            _Printer = new Printer(ref _Starters);
            _PrintMode = printMode;
            _Filter = filter;
            _Search = search;
            _Print = print;

            _PrintMode.SelectionChanged += PrintMode_SelectionChanged;
            _Print.Click += Print_Click;
            _Search.TextChanged += Search_TextChanged;

            _PrintMode.ItemsSource = new[] { "Online-Anmeldung", "Voranmeldung", "Ergebnisse", "Urkunde" };
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
                .Select(GetStarterDisplayText)
            );
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = _Search.Text;
            if (_PrintMode.SelectedIndex != 3)
                return;

            if (String.IsNullOrWhiteSpace(text))
                _Filter.ItemsSource = _CompletePrintList(_Starters);
            else
                _Filter.ItemsSource = _Starters.Starters
                    .Where(s => s.HasValue == true && s.Contains(text))
                    .Select(GetStarterDisplayText);

            _Filter.SelectedIndex = 0;
        }

        private void PrintMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (_PrintMode.SelectedIndex)
            {
                case 0:
                    PrepareRegistrationPrint();
                    break;
                case 1:
                    PrepareSignUpPrint();
                    break;
                case 2:
                    PrepareResultPrint();
                    break;
                case 3:
                    PrepareCertificatePrint();
                    break;
            }
        }

        private IEnumerable<string> LocalizeCompetitions(IEnumerable<Competition> competitions)
        {
            return competitions.Select(c => CompetitionLocalization(c, CurrentYear));
        }

        private void PrepareRegistrationPrint()
        {
            Competition[] items = { Competition.MainsSingle, Competition.MainsRelay, Competition.SubsSingle, Competition.SubRelay, Competition.Children };
            _Search.IsEnabled = false;
            _Search.Text = String.Empty;
            _Filter.ItemsSource = new[] { "alle" }.Concat(LocalizeCompetitions(items)).ToArray();
            _Filter.SelectedIndex = 0;
            PrintAction = () =>
            {
                if (_Filter.SelectedIndex == 0)
                    PrintRegistration(items);
                else
                    PrintRegistration(items[_Filter.SelectedIndex - 1]);
            };
        }

        private void PrepareSignUpPrint()
        {
            Competition[] items = { Competition.Mains, Competition.Subs, Competition.Children };
            _Search.IsEnabled = false;
            _Search.Text = String.Empty;
            _Filter.ItemsSource = LocalizeCompetitions(items).ToArray();
            _Filter.SelectedIndex = 0;
            PrintAction = () =>
                PrintSignUp(items[_Filter.SelectedIndex]);
        }

        private void PrepareResultPrint()
        {
            Competition[] items =
            {
                Competition.Mains,
                Competition.Subs,
                Competition.Children,
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
                Competition.ChildBigFemale,
            };
            _Search.IsEnabled = false;
            _Search.Text = String.Empty;
            _Filter.ItemsSource = LocalizeCompetitions(items).ToArray();
            _Filter.SelectedIndex = 0;
            PrintAction = () => PrintResults(items[_Filter.SelectedIndex]);
        }

        private void PrepareCertificatePrint()
        {
            Competition[] items = { Competition.Mains, Competition.Subs, Competition.Children };
            _Search.IsEnabled = true;
            _Search.Text = String.Empty;
            _Filter.ItemsSource = _CompletePrintList(_Starters);

            _Filter.SelectedIndex = 0;
            PrintAction = () =>
            {
                if (Int32.TryParse(_Filter.SelectedItem.ToString().Split('-')[0], out int startNumber))
                    PrintCertificate(_Starters[startNumber - 1]);
                else
                    PrintCertificate(_Starters.Starters.Where(s => items[_Filter.SelectedIndex].HasFlag(s.Competition) && s.Place > 0 && s.Place < 4));
            };
        }

        private Func<Starter, string> GetStarterDisplayText;

        private Action PrintAction;

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            PrintAction();
        }

        private PrintJob<Starter> RegistrationPrintJob(Competition competitionFilter)
        {
            IEnumerable<Starter> splitRelay(Starter[] s)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (!s[i].HasValue)
                        continue;
                    s[i].StartNumberHack = i + 1;
                    yield return s[i];
                    if (!String.IsNullOrWhiteSpace(s[i].TeamName))
                    {
                        Starter temp = new Starter();
                        temp.Self = s[i].Partner;
                        temp.TeamName = s[i].TeamName;
                        temp.Competition = s[i].Competition;
                        temp.StartNumberHack = i + 1;
                        temp.HasValue = true;
                        yield return temp;
                    }
                }
            };

            return new PrintJob<Starter>(
                sa => splitRelay(sa).Where(s => competitionFilter.HasFlag(s.Competition)).OrderBy(s => s.Self.LastName).ToArray(),
                $"Online-Anmeldungen {CompetitionLocalization(competitionFilter, CurrentYear)}",
                new PrintColumn<Starter>("NR", s => s.StartNumberHack, doAlignRight: true),
                new PrintColumn<Starter>("Vorname", s => s.Self.FirstName),
                new PrintColumn<Starter>("Nachname", s => s.Self.LastName),
                new PrintColumn<Starter>("m\\w", s => s.Self.IsMale ? "m" : "w"),
                new PrintColumn<Starter>("JG", s => s.Self.YoB),
                new PrintColumn<Starter>("Teamname", s => s.TeamName),
                new PrintColumn<Starter>("Startgebühr", s => s.Self.PaymentInfo),
                new PrintColumn<Starter>("Unterschrift (Einverständniserklärung)", s => null));
        }

        private void PrintRegistration(Competition competition)
        {
            _Printer.Print(RegistrationPrintJob(competition));
        }

        private void PrintRegistration(Competition[] competitions)
        {
            _Printer.MultiPrint(competitions.Select(c => RegistrationPrintJob(c)).ToArray());
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
                new PrintColumn<Starter>("JG", s => s.Self.YoB),
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
                new PrintColumn<Starter>("JG", s => s.Self.YoB),
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
