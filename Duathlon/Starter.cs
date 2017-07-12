using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Duathlon
{
    public class Starter
    {
        public bool HasValue { get; set; }
        public Competition Competition { get; set; } //competition type determined by m/f, age for kids and the actual competition
        public Person Self;
        public Person Partner;
        public string TeamName { get; set; }
        public int SwimPlace { get; set; }
        public int RunPlace { get; set; }
        public int Place { get; set; } //place in the respective competition type

        public TimeSpan SwimTime { get; set; }

        //runtime is not a real value, it results from time - swimtime
        [JsonIgnore]
        public TimeSpan RunTime
        {
            get
            {
                if (Time.Ticks == 0 || SwimTime.Ticks == 0)
                    return TimeSpan.Zero;
                return Time.Subtract(SwimTime);
            }
        }
        
        public TimeSpan Time { get; set; }

        [JsonIgnore]
        public int StartNumberHack { get; set; }

        public bool Contains(string s)
        {
            s = s.Trim().ToLower();
            if (Self.Contains(s))
                return true;
            if (String.IsNullOrWhiteSpace(TeamName))
                return false;
            if (TeamName.ToLower().Contains(s))
                return true;
            if (Partner.Contains(s))
                return true;
            return false;
        }
    }
}