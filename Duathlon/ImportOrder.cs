using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using static Duathlon.Statics;
using Newtonsoft.Json;
using System.IO;

namespace Duathlon
{
    public enum ImportField
    {
        FirstName,
        LastName,
        Gender,
        YoB,
        TeamName,
        Competition,
        Club,
        EMail,
    }

    public enum TabItems
    {
        Overview,
        Import,
        Edit,
        Time,
        Print
    }

    [Flags]
    public enum Competition
    {
        None = 0,
        MainSingleMale = 1,
        MainSingleFemale = 2,
        MainRelayMale = 4,
        MainRelayFemale = 8,
        MainRelayMixed = 16,
        SubSingleMale = 32,
        SubSingleFemale = 64,
        SubRelay = 128,
        ChildSmallMale = 256,
        ChildSmallFemale = 512,
        ChildMediumMale = 1024,
        ChildMediumFemale = 2048,
        ChildBigMale = 4096,
        ChildBigFemale = 8192,

        MainsSingle = MainSingleMale | MainSingleFemale,
        MainsRelay = MainRelayMale | MainRelayFemale | MainRelayMixed,
        Mains = MainsSingle | MainsRelay,
        SubsSingle = SubSingleMale | SubSingleFemale,
        Subs = SubsSingle | SubRelay,
        ChildrenMale = ChildBigMale | ChildMediumMale | ChildSmallMale,
        ChildrenFemale = ChildBigFemale | ChildMediumFemale | ChildSmallFemale,
        Children = ChildrenMale | ChildrenFemale,
    }

    public static class Statics
    {
        public static Dictionary<ImportField, int> ImportOrder { get; set; }

        public static List<int> MissingStartNumbers { get; set; }
            = new List<int> { 1, 2, 3, 4 };

        public static GoogleAuthCredentials GoogleAuthCredentials { get; set; }

        private static string CustomSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomSettings.json");

        public static void LoadSettings()
        {
            if (File.Exists(CustomSettingsPath))
            {
                SettingsSerialization settings;
                using (StreamReader reader = new StreamReader(CustomSettingsPath))
                    settings = JsonConvert.DeserializeObject<SettingsSerialization>(reader.ReadToEnd());

                ImportOrder = settings.ImportOrder;
                MissingStartNumbers = settings.MissingStartNumbers;
                GoogleAuthCredentials = settings.GoogleAuthCredentials;
                return;
            }

            //default settings
            ImportOrder = new Dictionary<ImportField, int>
            {
                [ImportField.FirstName] = 1,
                [ImportField.LastName] = 2,
                [ImportField.Gender] = 3,
                [ImportField.YoB] = 4,
                [ImportField.TeamName] = 5,
                [ImportField.Competition] = 6,
                [ImportField.Club] = 7,
                [ImportField.EMail] = 8,
            };
            MissingStartNumbers = new List<int>();
            GoogleAuthCredentials = new GoogleAuthCredentials
            {
                ClientID = "yada",
                ClientSecret = "yada",
            };
        }

        public static void SaveSettings()
        {
            SettingsSerialization settings = new SettingsSerialization
            {
                ImportOrder = ImportOrder,
                MissingStartNumbers = MissingStartNumbers,
                GoogleAuthCredentials = GoogleAuthCredentials,
            };

            using (StreamWriter writer = new StreamWriter(CustomSettingsPath))
                writer.Write(JsonConvert.SerializeObject(settings));
        }

        public static string CompetitionLocalization(Competition competition, int currentYear)
        {
            string head = CompetitionLocalizationHead(competition);
            string tail = CompetitionLocalizationTail(competition, currentYear);

            if (String.IsNullOrWhiteSpace(head) || String.IsNullOrWhiteSpace(tail))
                return $"{head}{tail}";

            return $"{head} - {tail}";
            //switch (competition)
            //{
            //    case Competition.None: return "keine Gruppe ausgewählt";
            
            //    case Competition.MainSingleMale: return "Hauptwettkampf - Einzel männlich";
            //    case Competition.MainSingleFemale: return "Hauptwettkampf - Einzel weiblich";
            //    case Competition.MainRelayMale: return "Hauptwettkampf - Staffel männlich";
            //    case Competition.MainRelayFemale: return "Hauptwettkampf - Staffel weiblich";
            //    case Competition.MainRelayMixed: return "Hauptwettkampf - Staffel mixed";
            
            //    case Competition.SubSingleMale: return "Jedermannwettkampf - Einzel männlich";
            //    case Competition.SubSingleFemale: return "Jedermannwettkampf - Einzel weiblich";
            //    case Competition.SubRelay: return "Jedermannwettkampf - Staffel";
            
            //    case Competition.ChildSmallMale: return $"Kinderwettkampf - {(currentYear - 10).ToString()}/{(currentYear - 9).ToString()} männlich";
            //    case Competition.ChildSmallFemale: return $"Kinderwettkampf - {(currentYear - 10).ToString()}/{(currentYear - 9).ToString()} weiblich";
            //    case Competition.ChildMediumMale: return $"Kinderwettkampf - {(currentYear - 12).ToString()}/{(currentYear - 11).ToString()} männlich";
            //    case Competition.ChildMediumFemale: return $"Kinderwettkampf - {(currentYear - 12).ToString()}/{(currentYear - 11).ToString()} weiblich";
            //    case Competition.ChildBigMale: return $"Kinderwettkampf - {(currentYear - 13).ToString()} männlich";
            //    case Competition.ChildBigFemale: return $"Kinderwettkampf - {(currentYear - 13).ToString()} weiblich";
            
            //    case Competition.MainsSingle: return "Hauptwettkampf - Einzel";
            //    case Competition.MainsRelay: return "Hauptwettkampf - Staffel";
            //    case Competition.Mains: return "Hauptwettkampf";
            //    case Competition.SubsSingle: return "Jedermannwettkampf - Einzel";
            //    case Competition.Subs: return "Jedermannwettkampf";
            //    case Competition.ChildrenMale: return "Kinderwettkampf - männlich";
            //    case Competition.ChildrenFemale: return "Kinderwettkampf - weiblich";
            //    case Competition.Children: return "Kinderwettkampf";

            //    default: return null;
            //}
        }

        public static string CompetitionLocalizationHead(Competition competition)
        {
            switch (competition)
            {
                case Competition.MainSingleMale:
                case Competition.MainSingleFemale:
                case Competition.MainRelayMale:
                case Competition.MainRelayFemale:
                case Competition.MainRelayMixed:

                case Competition.MainsSingle:
                case Competition.MainsRelay:
                case Competition.Mains: return "Hauptwettkampf";

                case Competition.SubSingleMale:
                case Competition.SubSingleFemale:
                case Competition.SubRelay:

                case Competition.SubsSingle:
                case Competition.Subs: return "Jedermannwettkampf";

                case Competition.ChildSmallMale:
                case Competition.ChildSmallFemale:
                case Competition.ChildMediumMale:
                case Competition.ChildMediumFemale:
                case Competition.ChildBigMale:
                case Competition.ChildBigFemale:
                
                case Competition.ChildrenMale:
                case Competition.ChildrenFemale:
                case Competition.Children: return "Kinderwettkampf";

                default: return null;
            }
        }

        public static string CompetitionLocalizationTail(Competition competition, int currentYear)
        {
            switch (competition)
            {
                case Competition.None: return "keine Gruppe ausgewählt";

                case Competition.MainSingleMale: return "Einzel männlich";
                case Competition.MainSingleFemale: return "Einzel weiblich";
                case Competition.MainRelayMale: return "Staffel männlich";
                case Competition.MainRelayFemale: return "Staffel weiblich";
                case Competition.MainRelayMixed: return "Staffel mixed";

                case Competition.SubSingleMale: return "Einzel männlich";
                case Competition.SubSingleFemale: return "Einzel weiblich";
                case Competition.SubRelay: return "Staffel";

                case Competition.ChildSmallMale: return $"{(currentYear - 10).ToString()}/{(currentYear - 9).ToString()} männlich";
                case Competition.ChildSmallFemale: return $"{(currentYear - 10).ToString()}/{(currentYear - 9).ToString()} weiblich";
                case Competition.ChildMediumMale: return $"{(currentYear - 12).ToString()}/{(currentYear - 11).ToString()} männlich";
                case Competition.ChildMediumFemale: return $"{(currentYear - 12).ToString()}/{(currentYear - 11).ToString()} weiblich";
                case Competition.ChildBigMale: return $"{(currentYear - 13).ToString()} männlich";
                case Competition.ChildBigFemale: return $"{(currentYear - 13).ToString()} weiblich";

                case Competition.MainsSingle: return "Einzel";
                case Competition.MainsRelay: return "Staffel";
                case Competition.SubsSingle: return "Einzel";
                case Competition.ChildrenMale: return "männlich";
                case Competition.ChildrenFemale: return "weiblich";

                default: return null;
            }
        }

        public static Dictionary<ImportField, string> ImportFieldLocalization { get; set; } = new Dictionary<ImportField, string>
        {
            [ImportField.FirstName] = "Vorname",
            [ImportField.LastName] = "Nachname",
            [ImportField.Gender] = "Geschlecht",
            [ImportField.YoB] = "Geburtsjahr",
            [ImportField.TeamName] = "Teamname",
            [ImportField.Competition] = "Wettkampfklasse",
            [ImportField.Club] = "Verein",
            [ImportField.EMail] = "E-Mail",
        };
    }

    public class SettingsSerialization
    {
        public Dictionary<ImportField, int> ImportOrder { get; set; }

        public List<int> MissingStartNumbers { get; set; }

        public GoogleAuthCredentials GoogleAuthCredentials { get; set; }
    }

    public class GoogleAuthCredentials
    {
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
    }

    class OrderDefinition
    {
        private Button _Apply;
        private ComboBox[] Boxes = new ComboBox[8];

        public OrderDefinition(ComboBox firstName, ComboBox lastName, ComboBox gender, ComboBox yoB, ComboBox teamName, ComboBox competition, ComboBox club, ComboBox email, Button apply)
        {
            Boxes[(int)ImportField.FirstName] = firstName;
            Boxes[(int)ImportField.LastName] = lastName;
            Boxes[(int)ImportField.Gender] = gender;
            Boxes[(int)ImportField.YoB] = yoB;
            Boxes[(int)ImportField.TeamName] = teamName;
            Boxes[(int)ImportField.Competition] = competition;
            Boxes[(int)ImportField.Club] = club;
            Boxes[(int)ImportField.EMail] = email;
            _Apply = apply;

            _Apply.Click += Apply_Click;

            for (int i = 0; i < Boxes.Length; i++)
            {
                Boxes[i].ItemsSource = Items;
                Boxes[i].SelectedIndex = ImportOrder[(ImportField)i];
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            foreach (ImportField field in Enum.GetValues(typeof(ImportField)))
                ImportOrder[field] = -1;

            List<int> assignedNumbers = new List<int>();
            bool error = false;

            Dictionary<ImportField, int> result = ImportOrder
                .Select(kvp =>
                {
                    int index = Boxes[(int)kvp.Key].SelectedIndex;
                    if (assignedNumbers.Contains(index))
                        error = true;
                    assignedNumbers.Add(index);
                    return new KeyValuePair<ImportField, int>(kvp.Key, index);
                })
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (result.ContainsValue(-1) || error)
            {
                MessageBox.Show("Reihenfolge nicht verarbeitbar");
                return;
            }

            ImportOrder = result;
            MessageBox.Show("Reihenfolge erfolgreich geändert");
        }

        private string[] Items = Enumerable.Range(0, 25).Select(i => i.ToString()).ToArray();
    }
}