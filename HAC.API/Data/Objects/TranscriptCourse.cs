namespace HAC.API.Data.Objects
{
    public class TranscriptCourse : ICourse
    {
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public double CourseAverage { get; set; }
        public double CourseCredit { get; set; }
    }
}