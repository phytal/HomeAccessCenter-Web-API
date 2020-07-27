using System.Net.Http;
using HAC.API.Data.Objects;
using HtmlAgilityPack;

namespace HAC.API.Data {
    public interface IStudentInfo {
        Student GetAllStudentInfo(string link);
    }

    public class StudentInfo : IStudentInfo {
        private readonly HttpClient _httpClient;

        public StudentInfo(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public Student GetAllStudentInfo(string link) {
            //loads and fetches data
            var studentData = RequestData.GetData(_httpClient, link, ResponseType.Registration);
            var registrationDoc = new HtmlDocument();
            registrationDoc.LoadHtml(studentData.Result);
            
            var studentName = Utils.FormatName(registrationDoc.GetElementbyId("plnMain_lblRegStudentName").InnerText,
                false);
            var birthDate = registrationDoc.GetElementbyId("plnMain_lblBirthDate").InnerText;
            var counselorName =
                Utils.FormatName(registrationDoc.GetElementbyId("plnMain_lblCounselor").InnerText, false);
            var counselorEmail = registrationDoc.GetElementbyId("plnMain_lblCounselor").FirstChild.Attributes[0].Value
                .Substring(7);
            var buildingName = registrationDoc.GetElementbyId("plnMain_lblBuildingName").InnerText;
            var gender = registrationDoc.GetElementbyId("plnMain_lblGender").InnerText;
            var calender = registrationDoc.GetElementbyId("plnMain_lblCalendar").InnerText;
            var grade = registrationDoc.GetElementbyId("plnMain_lblGrade").InnerText;
            var language = registrationDoc.GetElementbyId("plnMain_lblLanguage").InnerText;

            return new Student {
                StudentName = studentName,
                BirthDate = birthDate,
                CounselorName = counselorName,
                CounselorEmail = counselorEmail,
                Building = buildingName,
                Gender = gender,
                Calendar = calender,
                Grade = grade,
                Language = language
            };
        }

        // unused, returns the most basic information
        public Student GetStudentInfo(HtmlDocument registrationDoc) {
            var studentName = registrationDoc.GetElementbyId("plnMain_lblRegStudentName").InnerText;
            var language = registrationDoc.GetElementbyId("plnMain_lblLanguage").InnerText;

            return new Student {
                StudentName = studentName,
                Language = language
            };
        }
    }
}