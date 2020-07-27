using System.Collections.Generic;
using JetBrains.Annotations;

namespace HAC.API.Data.Objects {
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Response {
        public string Message { get; set; }
        public Student StudentInfo { get; set; }
        public IEnumerable<IEnumerable<IEnumerable<Day>>> Attendances { get; set; }
        public IEnumerable<IEnumerable<AssignmentCourse>> AssignmentList { get; set; }
        public IEnumerable<IEnumerable<Course>> IprList { get; set; }
        public IEnumerable<IEnumerable<Course>> ReportCardList { get; set; }
        public IEnumerable<IEnumerable<TranscriptCourse>> TranscriptList { get; set; }
    }
}