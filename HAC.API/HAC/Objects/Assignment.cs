using System;

namespace HAC.API.HAC.Objects
{
    public class Assignment
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public double Score { get; set; }
        public double MaxPoints { get; set; }
        public bool CanBeDropped { get; set; }
        public bool ExtraCredit { get; set; }
        public bool HasAttachments { get; set; }
    }
}