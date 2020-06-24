using System.Collections.Generic;

namespace HAC.API.Data.Objects
{
    public class Response
    {
        public string Message { get; set; }
        public IEnumerable<IEnumerable<AssignmentCourse>> AssignmentList { get; set; }
        public IEnumerable<IEnumerable<Course>> ReportCardList { get; set; }
        public IEnumerable<IEnumerable<TranscriptCourse>> TranscriptList { get; set; }
    }
}
