using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HAC.API.Data.Objects;
using HtmlAgilityPack;

namespace HAC.API.Data {
    public interface IReportCard {
        List<List<Course>> CheckReportCardTask(string link);
    }

    public class ReportCard : IReportCard {
        private readonly HttpClient _httpClient;

        public ReportCard(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public List<List<Course>> CheckReportCardTask(string link) {
            //fetches and loads data
            var reportCardData = RequestData.GetData(_httpClient, link, ResponseType.ReportCards);
            var reportCardDocument = new HtmlDocument();
            reportCardDocument.LoadHtml(reportCardData.Result);
            //checks the reporting period
            var reportCardHeader = reportCardDocument.DocumentNode.Descendants("div")
                .FirstOrDefault(node => node.GetAttributeValue("class", "")
                    .Equals("sg-header"));
            //gets reporting period number
            var reportCardNumber = reportCardHeader.Descendants("label")
                .FirstOrDefault(node => node.GetAttributeValue("id", "")
                    .Equals("plnMain_lblTitle")).InnerText.Trim();

            var reportingPeriod = byte.Parse(reportCardNumber.ElementAt(33).ToString());

            return Enumerable.Range(1, reportingPeriod).Select(period => GetReportCard(reportCardDocument, period))
                .ToList();
        }

        private static IEnumerable<Course> ReportCardScraping(HtmlDocument reportCardDocument, int markingPeriod) {
            var reportCardAssignmentList = new List<Course>();
            var allReportCardHtml = reportCardDocument.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                    .Equals("sg-content-grid")).ToList();
            var reportCardCourseList = allReportCardHtml[0].Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                    .Equals("sg-content-grid")).ToList();
            var reportCardCourseItemList = reportCardCourseList[0].Descendants("tr")
                .Where(node => node.GetAttributeValue("class", "")
                    .Equals("sg-asp-table-data-row")).ToList();
            foreach (var reportCardCourse in reportCardCourseItemList) //foreach report card
            {
                var courseName = reportCardCourse.Descendants("a") //gets course name
                    .FirstOrDefault(node => node.GetAttributeValue("href", "")
                        .Equals("#"))?.InnerText.Trim();

                var courseId = reportCardCourse.Descendants("td") //gets course id
                    .FirstOrDefault()?.InnerText.Trim();

                var elementNumber = markingPeriod switch {
                    1 => 2,
                    2 => 4,
                    3 => 5,
                    4 => 6,
                    _ => 0
                };

                var grades = new List<string> {
                    reportCardCourse.Descendants("a") //gets course grade
                        .ElementAt(elementNumber).InnerText.Trim()
                };

                if (grades.Contains("P")) continue; //for classes that you have received credit

                if (grades.Count != 1 || grades[0] == "") continue; //if it is not a grade and is empty then retry

                var courseInfo = Utils.BeautifyCourseInfo(courseName, courseId);
                (courseName, courseId) = courseInfo;

                var avg = grades.Sum(grade => int.Parse(grade.Trim()));
                avg /= grades.Count;
                var courseGrade = avg.ToString(); //finalized course grade
                reportCardAssignmentList.Add(new Course {
                    CourseId = courseId,
                    CourseName = courseName,
                    CourseAverage = double.Parse(courseGrade)
                }); //turns the grade (string) received into a double 
            }

            return reportCardAssignmentList;
        }

        //prevents duplicates
        private static List<Course> GetReportCard(HtmlDocument reportCardDocument, int markingPeriod) {
            var coursesFromReportCard = new List<Course>();
            var reportingPeriodCourses = ReportCardScraping(reportCardDocument, markingPeriod);
            foreach (var course in reportingPeriodCourses) {
                if (coursesFromReportCard.Contains(course)) {
                    var existingCourseIndex = coursesFromReportCard.FindIndex(x => x.CourseId == course.CourseId);
                    var existingCourse = coursesFromReportCard.ElementAt(existingCourseIndex);
                    var newAvg = (existingCourse.CourseAverage + course.CourseAverage) / 2;
                    existingCourse.CourseAverage = newAvg;
                }

                coursesFromReportCard.Add(course);
            }

            return coursesFromReportCard;
        }
    }
}