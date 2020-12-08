using System;
using System.Collections.Generic;
using System.Net.Http;
using HAC.API.Data.Objects;
using Sentry;

namespace HAC.API.Data {
    public interface IHac {
        Response GetAll(string link);
        Response GetStudentInfo(string link);
        Response GetCourses(string link);
        Response GetIpr(string link);
        Response GetReportCard(string link);
        Response GetTranscript(string link);
        Response GetAttendance(string link);
        bool IsValidLogin(string response);
    }

    public class Hac : IHac {
        private readonly IAttendance _attendance;
        private readonly ICourses _courses;
        private readonly IIpr _ipr;
        private readonly IReportCard _reportCard;
        private readonly IStudentInfo _studentInfo;
        private readonly ITranscript _transcript;

        public Hac(IAttendance attendance, ICourses courses, IIpr ipr, IReportCard reportCard, IStudentInfo studentInfo,
            ITranscript transcript) {
            _attendance = attendance;
            _courses = courses;
            _ipr = ipr;
            _reportCard = reportCard;
            _studentInfo = studentInfo;
            _transcript = transcript;
        }

        public Response GetAll(string link) {
            Student studentInfo;
            List<List<List<Day>>> calendarList;
            List<List<TranscriptCourse>> oldAssignmentList;
            List<List<AssignmentCourse>> currentAssignmentList;
            List<List<Course>> reportCardList, iprList;
            
            try {
                //student info
                studentInfo = _studentInfo.GetAllStudentInfo(link);

                //attendance 
                calendarList = _attendance.GetAttendances(link);

                //report card
                reportCardList = _reportCard.CheckReportCardTask(link);

                //ipr
                iprList = _ipr.GetGradesFromIpr(link);

                //current courses
                currentAssignmentList = _courses.GetAssignmentsFromMarkingPeriod(link);

                //past courses/transcript 
                oldAssignmentList = _transcript.GetTranscript(link);
            }
            catch (Exception e) {
                SentrySdk.CaptureException(e);
                return new Response {
                    Message = $"Error 404: Could not fetch information. \nException: {e}"
                };
            }

            return new Response {
                Message = "Success",
                StudentInfo = studentInfo,
                Attendances = calendarList,
                AssignmentList = currentAssignmentList,
                TranscriptList = oldAssignmentList,
                ReportCardList = reportCardList,
                IprList = iprList
            };
        }

        public Response GetStudentInfo(string link) {
            Student studentInfo;

            try {
                studentInfo = _studentInfo.GetAllStudentInfo(link);
            }
            catch (Exception e) {
                SentrySdk.CaptureException(e);
                return new Response {
                    Message = $"Error 404: Could not fetch information. \nException: {e}"
                };
            }

            return new Response {
                Message = "Success",
                StudentInfo = studentInfo
            };
        }


        public Response GetCourses(string link) {
            List<List<AssignmentCourse>> currentAssignmentList;
            var assignmentList = _courses.GetAssignmentsFromMarkingPeriod(link);

            try {
                currentAssignmentList = assignmentList;
            }
            catch (Exception e) {
                SentrySdk.CaptureException(e);
                return new Response {
                    Message = $"Error 404: Could not fetch information. \nException: {e}"
                };
            }

            return new Response {
                Message = "Success",
                AssignmentList = currentAssignmentList
            };
        }

        public Response GetIpr(string link) {
            List<List<Course>> iprList;

            try {
                iprList = _ipr.GetGradesFromIpr(link);
            }
            catch (Exception e) {
                SentrySdk.CaptureException(e);
                return new Response {
                    Message = $"Error 404: Could not fetch information. \nException: {e}"
                };
            }

            return new Response {
                Message = "Success",
                IprList = iprList
            };
        }

        public Response GetReportCard(string link) {
            List<List<Course>> reportCardCourses;
            try {
                reportCardCourses = _reportCard.CheckReportCardTask(link);
            }
            catch (Exception e) {
                SentrySdk.CaptureException(e);
                return new Response {
                    Message = $"Error 404: Could not fetch information. \nException: {e}"
                };
            }

            return new Response {
                Message = "Success",
                ReportCardList = reportCardCourses
            };
        }

        public Response GetTranscript(string link) {
            List<List<TranscriptCourse>> oldAssignmentList;
            try {
                oldAssignmentList = _transcript.GetTranscript(link);
            }
            catch (Exception e) {
                SentrySdk.CaptureException(e);
                return new Response {
                    Message = $"Error 404: Could not fetch information. \nException: {e}"
                };
            }

            return new Response {
                Message = "Success",
                TranscriptList = oldAssignmentList
            };
        }

        public Response GetAttendance(string link) {
            List<List<List<Day>>> calendarList;

            try {
                calendarList = _attendance.GetAttendances(link);
            }
            catch (Exception e) {
                SentrySdk.CaptureException(e);
                return new Response {
                    Message = $"Error 404: Could not fetch information. \nException: {e}"
                };
            }

            return new Response {
                Message = "Success",
                Attendances = calendarList
            };
        }

        public bool IsValidLogin(string response) {
            return !response.Contains("Your attempt to log in was unsuccessful.");
        }
    }
}