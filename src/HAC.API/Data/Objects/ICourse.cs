namespace HAC.API.Data.Objects {
    public interface ICourse {
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public double CourseAverage { get; set; }
    }
}