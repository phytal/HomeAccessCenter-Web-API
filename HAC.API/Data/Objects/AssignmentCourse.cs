using System.Collections.Generic;

namespace HAC.API.Data.Objects
{
    public class AssignmentCourse : ICourse
    {
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public double CourseAverage { get; set; }
        public IEnumerable<Assignment> Assignments { get; set; }
        public GradeInfo GradeInfo { get; set; }
        public string Teacher { get; set; }
        public string RoomNumber { get; set; }
    }
}