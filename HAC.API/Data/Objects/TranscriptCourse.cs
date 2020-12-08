using JetBrains.Annotations;

namespace HAC.API.Data.Objects {
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class TranscriptCourse : ICourse {
        public double CourseCredit { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public double CourseAverage { get; set; }
    }
}