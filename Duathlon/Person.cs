using System;

namespace Duathlon
{
    public struct Person
    {
        public string Club { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsMale { get; set; } //0 - female, 1 - male
        public int YoB { get; set; } //year of birth
        public string E_Mail { get; set; }

        public bool Contains(string s)
        {
            s = s.Trim().ToLower();
            if (!String.IsNullOrWhiteSpace(FirstName) && FirstName.ToLower().Contains(s))
                return true;
            if (!String.IsNullOrWhiteSpace(LastName) && LastName.ToLower().Contains(s))
                return true;
            if (!String.IsNullOrWhiteSpace(Club) && Club.ToLower().Contains(s))
                return true;
            return false;
        }
    }
}