namespace Duathlon
{
    public class MyRow
    {
        public int Nr { get; set; }
        public string Vorname { get; set; }
        public string Name { get; set; }
        public string Geburtsjahr { get; set; }
        public string Geschlecht { get; set; }
        public string Verein { get; set; }
        public string EMail { get; set; }
        public string SchwimmZeit { get; set; }
        public int SchwimmPlatz { get; set; }
        public string LaufZeit { get; set; }
        public int LaufPlatz { get; set; }
        public string GesamtZeit { get; set; }
        public int GesamtPlatz { get; set; }

        public MyRow(Starter starter, int startNo)
        {
            Nr = startNo;
            Vorname = starter.Self.FirstName;
            Name = starter.Self.LastName;
            Geburtsjahr = starter.Self.YoB.ToString();
            if (starter.Self.IsMale)
                Geschlecht = "männlich";
            else
                Geschlecht = "weiblich";
            Verein = starter.Self.Club;
            EMail = starter.Self.E_Mail;
            SchwimmZeit = starter.SwimTime.ToString();
            SchwimmPlatz = starter.SwimPlace;
            LaufZeit = starter.RunTime.ToString();
            LaufPlatz = starter.RunPlace;
            GesamtZeit = starter.Time.ToString();
            GesamtPlatz = starter.Place;
            if (starter.TeamName != "")
            {
                Vorname += '\n' + starter.Partner.FirstName;
                Name += '\n' + starter.Partner.LastName;
                Geburtsjahr += '\n' + starter.Partner.YoB.ToString();
                if (starter.Partner.IsMale)
                    Geschlecht += '\n' + "männlich";
                else
                    Geschlecht += '\n' + "weiblich";
                Verein += '\n' + starter.Partner.Club;
                EMail += '\n' + starter.Partner.E_Mail;
            }
        }
    }
}