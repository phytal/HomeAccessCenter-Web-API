using System;

namespace HAC.API.Data.Objects {
    public class Assignment {
        public string Title { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public DateTime DueDate { get; set; }
        public AssignmentStatus Status { get; set; }
        public string Score { get; set; }
        public double MaxPoints { get; set; }
        public bool CanBeDropped { get; set; }
        public bool ExtraCredit { get; set; }
        public bool HasAttachments { get; set; }
    }

    public enum AssignmentStatus {
        Upcoming,
        Complete,
        Incomplete,
        Missing,
        Excused,
        Late
    }
}