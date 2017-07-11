using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Duathlon
{
    class StarterIO
    {
        private static string _Path;
        private static string Path
        {
            get
            {
                return _Path;
            }
            set
            {
                _Path = value;
                NameChanged(System.IO.Path.GetFileNameWithoutExtension(_Path), new EventArgs());
            }
        }

        public static bool IsSaved { get; set; } = true;
        public static bool IsDataExisting { get; set; }

        public static event EventHandler NameChanged;

        public static StarterWrapper NewDoc(StarterWrapper starters)
        {
            if (!IsSaved)
            {
                MessageBoxResult result = MessageBox.Show("Änderungen speichern?", "Achtung!", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save(starters);
                        if (!IsSaved)
                        {
                            return null;
                        }
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        return null;
                }
            }
            IsDataExisting = false;
            IsSaved = true;
            Path = null;
            return StarterWrapper.CreateNew();
        }

        public static void Save(StarterWrapper starters)
        {
            if (IsSaved)
            {
                return;
            }
            if (Path != null && MessageBox.Show("Datei Überschreiben?", "Achtung", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                SaveData(Path, starters);
                IsSaved = true;
                return;
            }
            SaveAs(starters);
        }

        public static void SaveAs(StarterWrapper starters)
        {
            SaveFileDialog savefileDialog = new SaveFileDialog();
            savefileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + @"saves";
            savefileDialog.Filter = "JSON Datei (*.json)|*.json";
            if ((bool)savefileDialog.ShowDialog())
            {
                Path = savefileDialog.FileName;
                SaveData(Path, starters);
                IsSaved = true;
            }
        }

        public static StarterWrapper Open(StarterWrapper starters)
        {
            if (!IsSaved)
            {
                MessageBoxResult result = MessageBox.Show("Datei vorm Öffnen speichern?", "Achtung!", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save(starters);
                        if (!IsSaved)
                        {
                            return null;
                        }
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        return null;
                }
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + @"saves";
            openFileDialog.Filter = "JSON Datei (*.json)|*.json";

            if (openFileDialog.ShowDialog() != true)
            {
                return null;
            }
            Path = openFileDialog.FileName;
            IsSaved = true;
            IsDataExisting = true;
            return LoadData(Path);

        }

        private static void SaveData(string path, StarterWrapper starters)
        {
            using (FileStream stream = File.Create(path))
            using (StreamWriter writer = new StreamWriter(stream))
                writer.Write(JsonConvert.SerializeObject(starters));
        }

        private static StarterWrapper LoadData(string path)
        {
            StarterWrapper foo;
            using (FileStream stream = File.OpenRead(path))
            using (StreamReader reader = new StreamReader(stream))
                foo = JsonConvert.DeserializeObject<StarterWrapper>(reader.ReadToEnd());
            return foo;
        }

        public static bool CanWindowBeClosed(StarterWrapper starters)
        {
            if (!IsSaved)
            {
                MessageBoxResult result = MessageBox.Show("Änderungen speichern?", "Achtung!", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save(starters);
                        if (!IsSaved)
                            return true;
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        return true;
                }
            }
            return false;
        }
    }
}
