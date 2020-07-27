using System.Collections.Generic;
using JetBrains.Annotations;

namespace HAC.API.Data.Objects {
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AssignmentCourse : ICourse {
        public IEnumerable<Assignment> Assignments { get; set; }
        public GradeInfo GradeInfo { get; set; }
        public string Teacher { get; set; }
        public string RoomNumber { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public double CourseAverage { get; set; }
    }
}