using System.Collections.Generic;

namespace HAC.API.Data.Objects
{
    public class Response
    {
        public string Message { get; set; }
        public IEnumerable<AssignmentCourse> AssignmentList1 { get; set; }
        public IEnumerable<AssignmentCourse> AssignmentList2 { get; set; }
        public IEnumerable<AssignmentCourse> AssignmentList3 { get; set; }
        public IEnumerable<AssignmentCourse> AssignmentList4 { get; set; }
        public IEnumerable<Course> ReportCardList1 { get; set; }
        public IEnumerable<Course> ReportCardList2 { get; set; }
        public IEnumerable<Course> ReportCardList3 { get; set; }
        public IEnumerable<Course> ReportCardList4 { get; set; }
        public IEnumerable<Course> TranscriptList { get; set; }
    }
}
