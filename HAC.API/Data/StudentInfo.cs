using HAC.API.Data.Objects;
using HtmlAgilityPack;

namespace HAC.API.Data {
    public static class StudentInfo {
        public static Student GetAllStudentInfo(HtmlDocument registrationDoc) {
            var studentName = Utils.FormatName(registrationDoc.GetElementbyId("plnMain_lblRegStudentName").InnerText,
                false);
            var birthDate = registrationDoc.GetElementbyId("plnMain_lblBirthDate").InnerText;
            //var houseTeam = registrationDoc.GetElementbyId("plnMain_lblHouseTeam").InnerText;
            var counselorName =
                Utils.FormatName(registrationDoc.GetElementbyId("plnMain_lblCounselor").InnerText, false);
            var counselorEmail = registrationDoc.GetElementbyId("plnMain_lblCounselor").FirstChild.Attributes[0].Value
                .Substring(7);
            var buildingName = registrationDoc.GetElementbyId("plnMain_lblBuildingName").InnerText;
            var gender = registrationDoc.GetElementbyId("plnMain_lblGender").InnerText;
            var calender = registrationDoc.GetElementbyId("plnMain_lblCalendar").InnerText;
            //var homeroom = registrationDoc.GetElementbyId("plnMain_lblHomeroom").InnerText;
            var grade = registrationDoc.GetElementbyId("plnMain_lblGrade").InnerText;
            var language = registrationDoc.GetElementbyId("plnMain_lblLanguage").InnerText;
            //var homeroomTeacher = registrationDoc.GetElementbyId("plnMain_lblHomeroomTeacher").InnerText;

            return new Student {
                StudentName = studentName,
                BirthDate = birthDate,
                //HouseTeam = houseTeam,
                CounselorName = counselorName,
                CounselorEmail = counselorEmail,
                Building = buildingName,
                Gender = gender,
                Calendar = calender,
                //Homeroom = homeroom,
                Grade = grade,
                Language = language,
                //HomeroomTeacher = homeroomTeacher
            };
        }

        // unused, returns the most basic information
        public static Student GetStudentInfo(HtmlDocument registrationDoc)
        {
            var studentName = registrationDoc.GetElementbyId("plnMain_lblRegStudentName").InnerText;
            var language = registrationDoc.GetElementbyId("plnMain_lblLanguage").InnerText;
        
            return new Student
            {
                StudentName = studentName,
                Language = language
            };
        }
    }
}