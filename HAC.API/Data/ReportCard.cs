using System.Collections.Generic;
using System.Linq;
using HAC.API.Data.Objects;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace HAC.API.Data
{
    public static class ReportCard
    {
        public static List<List<Course>> CheckReportCardTask(HtmlDocument reportCardDocument)
        {
            var reportCardList = new List<List<Course>>();

            //checks the reporting period
            var reportCardHeader = reportCardDocument.DocumentNode.Descendants("div")
                .FirstOrDefault(node => node.GetAttributeValue("class", "")
                    .Equals($"sg-header"));
            //gets reporting period number
            var reportCardNumber = reportCardHeader.Descendants("label")
                    .FirstOrDefault(node => node.GetAttributeValue("id", "")
                        .Equals($"plnMain_lblTitle")).InnerText.Trim();

            var reportingPeriod = byte.Parse(reportCardNumber.ElementAt(33).ToString());

            foreach (var period in Enumerable.Range(1, reportingPeriod))
            {
                reportCardList.Add(GetReportCard(reportCardDocument, period));
            }

            return reportCardList;
        }

        private static IEnumerable<Course> ReportCardScraping(HtmlDocument reportCardDocument, int markingPeriod)
        {
            List<Course> reportCardAssignmentList = new List<Course>();
            var allReportCardHtml = reportCardDocument.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                    .Equals($"sg-content-grid")).ToList();
            var reportCardCourseList = allReportCardHtml[0].Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                    .Equals($"sg-content-grid")).ToList();
            var reportCardCourseItemList = reportCardCourseList[0].Descendants("tr")
                .Where(node => node.GetAttributeValue("class", "")
                    .Equals($"sg-asp-table-data-row")).ToList();
            foreach (var reportCardCourse in reportCardCourseItemList) //foreach report card
            {
                var courseName = reportCardCourse.Descendants("a") //gets course name
                    .FirstOrDefault(node => node.GetAttributeValue("href", "")
                        .Equals($"#")).InnerText.Trim();

                var courseID = reportCardCourse.Descendants("td") //gets course id
                    .FirstOrDefault().InnerText.Trim();
                
                var elementNumber = markingPeriod switch
                {
                    1 => 2,
                    2 => 4,
                    3 => 5,
                    4 => 6,
                    _ => 0
                };

                var grades = new List<string>
                {
                    reportCardCourse.Descendants("a") //gets course grade
                        .ElementAt(elementNumber).InnerText.Trim()
                };

                if (grades.Contains("P")) continue; //for classes that you have received credit

                if (grades.Count != 1 || grades[0] == "")
                {
                    continue; //if it is not a grade and is empty then retry
                }

                while (courseName.Substring(courseName.Length - 2) == "S1" ||
                       courseName.Substring(courseName.Length - 2) == "S2")
                {
                    courseName = courseName.Replace(courseName.Substring(courseName.Length - 2), "");
                    while (courseName.LastOrDefault() == ' ' || courseName.LastOrDefault() == '-')
                    {
                        courseName = courseName.TrimEnd(courseName[^1]);
                    }
                }

                courseID = courseID.Remove(courseID.Length - 4);

                //removes excess
                while (courseID.LastOrDefault() == ' ' || courseID.LastOrDefault() == '-' ||
                       courseID.LastOrDefault() == 'A' || courseID.LastOrDefault() == 'B')
                {
                    courseID = courseID.TrimEnd(courseID[^1]);
                }

                var avg = 0;
                foreach (var grade in grades)
                    avg += int.Parse(grade.Trim());
                avg /= grades.Count;
                var courseGrade = avg.ToString(); //finalized course grade
                reportCardAssignmentList.Add(new Course
                {
                    CourseId = courseID,
                    CourseName = courseName,
                    CourseAverage = double.Parse(courseGrade)
                }); //turns the grade (string) received into a double 
            }

            return reportCardAssignmentList;
        }

        private static List<Course> GetReportCard(HtmlDocument reportCardDocument, int markingPeriod)
        {
            var coursesFromReportCard = new List<Course>();
            var reportingPeriodCourses = ReportCardScraping(reportCardDocument, markingPeriod);
            foreach (var course in reportingPeriodCourses)
            {
                if (coursesFromReportCard.Contains(course))
                {
                    var existingCourseIndex = coursesFromReportCard.FindIndex(x => x.CourseId == course.CourseId);
                    Course existingCourse = coursesFromReportCard.ElementAt(existingCourseIndex);
                    var newAvg = (existingCourse.CourseAverage + course.CourseAverage) / 2;
                    existingCourse.CourseAverage = newAvg;
                }
                coursesFromReportCard.Add(course);
            }

            return coursesFromReportCard;
        }
    }
}