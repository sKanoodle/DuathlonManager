using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Google.GData.Client;
using static Duathlon.Statics;

namespace Duathlon
{
    public class StarterWrapper
    {
        public int CurrentYear {get; set; } = DateTime.Now.Year;
        public int Length {get; set; } = 400;
        public int relayNumbersStartAt {get; set; } = 300;
        public int YoBLowerBoundary {get; set; } = 1900;

        public Starter[] Starters {get; set;}
        
        [Newtonsoft.Json.JsonIgnore]
        public Starter this[int i]
        {
            get
            {
                return Starters[i];
            }
            set
            {
                StarterIO.IsDataExisting = true;
                StarterIO.IsSaved = false;
                Starters[i] = value;
            }
        }

        public int GetIndex(Starter starter)
        {
            for (int index = 0; index < Starters.Length; index++)
                if (starter == Starters[index])
                    return index;
            return -1;
        }

        public static Competition GetCompetition(out bool doThrowAway, string competition, bool isMale, int optionalAge = -1, bool optionalRelayIsMale = false)
        {
            doThrowAway = false;
            if (competition == "Hauptwettkampf (Einzel)" && isMale)
                return Competition.MainSingleMale;
            if (competition == "Hauptwettkampf (Einzel)" && !isMale)
                return Competition.MainSingleFemale;
            if ((competition == "Hauptwettkampf (Staffel-Schwimmer)" || competition == "Hauptwettkampf (Staffel-Läufer)") && isMale && optionalRelayIsMale)
                return Competition.MainRelayMale;
            if ((competition == "Hauptwettkampf (Staffel-Schwimmer)" || competition == "Hauptwettkampf (Staffel-Läufer)") && isMale == false && optionalRelayIsMale == false)
                return Competition.MainRelayFemale;
            if ((competition == "Hauptwettkampf (Staffel-Schwimmer)" || competition == "Hauptwettkampf (Staffel-Läufer)") && (isMale && optionalRelayIsMale == false || isMale == false && optionalRelayIsMale))
                return Competition.MainRelayMixed;
            if (competition == "Jedermann-Wettkampf (Einzel)" && isMale)
                return Competition.SubSingleMale;
            if (competition == "Jedermann-Wettkampf (Einzel)" && isMale == false)
                return Competition.SubSingleFemale;
            if (competition == "Jedermann-Wettkampf (Staffel-Schwimmer)" || competition == "Jedermann-Wettkampf (Staffel-Läufer)")
                return Competition.SubRelay;
            if (competition == "Kinderwettkampf" && optionalAge < 11 && isMale)
                return Competition.ChildSmallMale;
            if (competition == "Kinderwettkampf" && optionalAge < 11 && isMale == false)
                return Competition.ChildSmallFemale;
            if (competition == "Kinderwettkampf" && optionalAge < 13 && isMale)
                return Competition.ChildMediumMale;
            if (competition == "Kinderwettkampf" && optionalAge < 13 && isMale == false)
                return Competition.ChildMediumFemale;
            if (competition == "Kinderwettkampf" && optionalAge < 14 && isMale)
                return Competition.ChildBigMale;
            if (competition == "Kinderwettkampf" && optionalAge < 14 && isMale == false)
                return Competition.ChildBigFemale;

            doThrowAway = true;
            if (competition == "Kinderwettkampf")
                return Competition.None;
            throw new Exception();
        }

        public void ReadBulkSignUp(string[,] starters)
        {
            Dictionary<string, int> relayStartersCache = new Dictionary<string, int>();
            
            CopyTo(this, CreateNew());
            int nextSingleStarterIndex = 0;
            int nextRelayStarterIndex = relayNumbersStartAt - 1;
            int dataIndex;

            //TODO: handle null values better
            for (int i = 1; i < starters.GetLength(0); i++)
            {
                if (starters[i, ImportOrder[ImportField.FirstName]] == null || starters[i, ImportOrder[ImportField.LastName]] == null)
                    continue;

                string teamName = starters[i, ImportOrder[ImportField.TeamName]] ?? String.Empty;

                //the current person is the partner of an already existing team
                if (relayStartersCache.ContainsKey(teamName))
                {
                    //assign the data to the chached startnumber of the team
                    if (AssignData(starters, i, out Starters[relayStartersCache[teamName]].Partner) == false)
                    {
                        MessageBox.Show(String.Format("Fehler in Starter '{0} {1}'", Starters[relayStartersCache[teamName]].Partner.FirstName, Starters[relayStartersCache[teamName]].Partner.LastName));
                    }

                    //determine the competition the team starts in
                    bool doThrowAway;
                    Starters[relayStartersCache[teamName]].Competition = GetCompetition(
                        out doThrowAway,
                        competition: starters[i, ImportOrder[ImportField.Competition]], 
                        isMale: Starters[relayStartersCache[teamName]].Self.IsMale, 
                        optionalRelayIsMale: Starters[relayStartersCache[teamName]].Partner.IsMale);

                    if (doThrowAway)
                    {
                        Starters[relayStartersCache[teamName]] = new Starter();
                        relayStartersCache.Remove(teamName);
                    }
                    
                    //swap persons in the team in case the swimmer is saved last
                    if (starters[i, ImportOrder[ImportField.Competition]].Contains("Schwimmer"))
                    {
                        Person swaptemp = Starters[relayStartersCache[teamName]].Partner;
                        Starters[relayStartersCache[teamName]].Partner = Starters[relayStartersCache[teamName]].Self;
                        Starters[relayStartersCache[teamName]].Self = swaptemp;
                    }
                    continue;
                }

                //the current person is the first partner of a new team
                if (teamName != "")
                {
                    while (MissingStartNumbers.Contains(nextRelayStarterIndex + 1))
                        nextRelayStarterIndex++;
                    dataIndex = nextRelayStarterIndex;
                    nextRelayStarterIndex++;
                    relayStartersCache.Add(teamName, dataIndex);
                    if(AssignData(starters, i, out Starters[dataIndex].Self) == false)
                        MessageBox.Show(String.Format("Fehler in Starter '{0} {1}'", Starters[dataIndex].Self.FirstName, Starters[dataIndex].Self.LastName));
                }
                //the current person is a single competitor
                else
                {
                    while (MissingStartNumbers.Contains(nextSingleStarterIndex + 1))
                        nextSingleStarterIndex++;
                    dataIndex = nextSingleStarterIndex;
                    nextSingleStarterIndex++;

                    if (AssignData(starters, i, out Starters[dataIndex].Self) == false)
                        MessageBox.Show(String.Format("Fehler in Starter '{0} {1}'", Starters[dataIndex].Self.FirstName, Starters[dataIndex].Self.LastName));

                    bool doThrowAway;
                    Starters[dataIndex].Competition = GetCompetition(
                        out doThrowAway,
                        competition: starters[i, ImportOrder[ImportField.Competition]], 
                        isMale: Starters[dataIndex].Self.IsMale,
                        optionalAge: CurrentYear - Starters[dataIndex].Self.YoB);

                    if (doThrowAway)
                    {
                        Starters[dataIndex] = new Starter();
                        nextSingleStarterIndex--;
                    }
                }
                //assign teamname and flag the starter as valid
                Starters[dataIndex].TeamName = teamName;
                Starters[dataIndex].HasValue = true;
            }
        }

        public bool AssignData(string[,] data, int index, out Person person)
        {
            person = new Person();
            person.FirstName = data[index, ImportOrder[ImportField.FirstName]];
            person.LastName = data[index, ImportOrder[ImportField.LastName]];
            person.Club = data[index, ImportOrder[ImportField.Club]];
            person.E_Mail = data[index, ImportOrder[ImportField.EMail]];
            person.PaymentInfo = data[index, ImportOrder[ImportField.PaymentInfo]];

            if (data[index, ImportOrder[ImportField.Gender]] == "männlich")
                person.IsMale = true;
            else if (data[index, ImportOrder[ImportField.Gender]] == "weiblich")
                person.IsMale = false;
            else
                return false;

            int yearOfBirth;
            if(int.TryParse(data[index, ImportOrder[ImportField.YoB]], out yearOfBirth) == false)
                return false;
            if(yearOfBirth > CurrentYear)
                return false;
            person.YoB = yearOfBirth;
            return true;
        }

        public static StarterWrapper CreateNew()
        {
            StarterWrapper result = new StarterWrapper();
            result.Starters = new Starter[result.Length];
            for (int i = 0; i < result.Starters.Length; i++)
                result.Starters[i] = new Starter();
            return result;
        }

        public static void CopyTo(StarterWrapper destination, StarterWrapper source)
        {
            destination.CurrentYear = source.CurrentYear;
            destination.Length = source.Length;
            destination.relayNumbersStartAt = source.relayNumbersStartAt;
            destination.YoBLowerBoundary = source.YoBLowerBoundary;
            destination.Starters = source.Starters;
        }
    }
}
