using JetBrains.Annotations;

namespace HAC.API.Data.Objects {
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Student {
        public string StudentName { get; set; }
        public string BirthDate { get; set; }
        public string CounselorName { get; set; }
        public string CounselorEmail { get; set; }
        public string Building { get; set; }
        public string Calendar { get; set; }
        public string Grade { get; set; }
        public string Language { get; set; }
    }
}