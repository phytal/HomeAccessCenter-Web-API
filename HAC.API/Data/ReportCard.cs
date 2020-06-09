using System.Collections.Generic;
using System.Linq;
using HAC.API.Data.Objects;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace HAC.API.Data
{
    public static class ReportCard
    {
        public static IEnumerable<Course>[] CheckReportCardTask(HtmlDocument reportCardDocument)
        {
            var coursesFromReportCard1 = new List<Course>();
            var reportCardList = new IEnumerable<Course>[4];

            //checks the reporting period
            var reportCardHeader = reportCardDocument.DocumentNode.Descendants("div")
                .FirstOrDefault(node => node.GetAttributeValue("class", "")
                    .Equals($"sg-header"));
            //gets reporting period number
            var reportCardNumber = reportCardHeader.Descendants("label")
                    .FirstOrDefault(node => node.GetAttributeValue("id", "")
                        .Equals($"plnMain_lblTitle")).InnerText.Trim();

            var reportingPeriod = byte.Parse(reportCardNumber.ElementAt(33).ToString());
            
            if (reportingPeriod == 4)
            {
                reportCardList[3] = GetReportCard(reportCardDocument, 4);
            }
            
            if (reportingPeriod >= 3)
            {
                reportCardList[2] = GetReportCard(reportCardDocument, 3);;
            }

            if (reportingPeriod >= 2)
            {
                reportCardList[1] = GetReportCard(reportCardDocument, 2);;
            }

            if (reportingPeriod >= 1)
            {
                List<Course> reportingPeriod1Courses = ReportCardScraping(reportCardDocument, 1);
                foreach (var course in reportingPeriod1Courses)
                {
                    coursesFromReportCard1.Add(course);
                }
                reportCardList[0] = coursesFromReportCard1;
            }

            return reportCardList;
        }

        private static List<Course> ReportCardScraping(HtmlDocument reportCardDocument, int markingPeriod)
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

                string courseGrade; //finalized course grade
                var elementNumber = markingPeriod switch
                {
                    1 => 2,
                    2 => 4,
                    3 => 5,
                    4 => 7,
                    _ => 0
                };

                var grades = new List<string>();

                grades.Add(reportCardCourse.Descendants("a") //gets course grade
                    .ElementAt(elementNumber).InnerText.Trim());

                if (grades.Contains("P")) continue; //ex: sub marching band

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
                courseGrade = avg.ToString();
                reportCardAssignmentList.Add(new Course
                {
                    CourseId = courseID,
                    CourseName = courseName,
                    CourseAverage = double.Parse(courseGrade)
                }); //turns the grade (string) received into a double 
            }

            return reportCardAssignmentList;
        }

        private static IEnumerable<Course> GetReportCard(HtmlDocument reportCardDocument, int markingPeriod)
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