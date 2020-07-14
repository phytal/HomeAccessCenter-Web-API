using System.Collections.Generic;

namespace HAC.API.Data.Objects
{
    public class Day
    {
        public string DayName { get; set; }
        public int Date { get; set; }
        public bool DayOff { get; set; }
        public List<AttendanceRecord> Attendances { get; set; }
    }

    public class AttendanceRecord
    {
        public int Period { get; set; }
        public string Reason { get; set; }
        public string Note { get; set; }
    }
}