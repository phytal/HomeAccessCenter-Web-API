using System;
using System.Collections.Generic;
using System.Linq;
using HAC.API.HAC.Objects;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace HAC.API.HAC
{
    public class CheckReportCard
    {
        public static ReportCardList[] CheckReportCardTask(HtmlDocument reportCardDocument)
        {
            var coursesFromReportCard1 = new List<Course>();
            var coursesFromReportCard2 = new List<Course>();
            var coursesFromReportCard3 = new List<Course>();
            var coursesFromReportCard4 = new List<Course>();
            var reportCardList = new ReportCardList[4];

            //checks the reporting period
            var reportCardHeader = reportCardDocument.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                    .Equals($"sg-header")).FirstOrDefault();
            //gets reporting period number
            var reportCardNumber = reportCardHeader.Descendants("label")
                    .Where(node => node.GetAttributeValue("id", "")
                        .Equals($"plnMain_lblTitle")).FirstOrDefault().InnerText.Trim();

            var reportingPeriod = byte.Parse(reportCardNumber.ElementAt(33).ToString());
            
            if (reportingPeriod == 4)
            {
                var reportingPeriod3Courses = ReportCardScraping(reportCardDocument, 4);
                foreach (var course in reportingPeriod3Courses)
                {
                    if (coursesFromReportCard4.Contains(course))
                    {
                        var existingCourseIndex = coursesFromReportCard4.FindIndex(x => x.courseID == course.courseID);
                        Course existingCourse = coursesFromReportCard4.ElementAt(existingCourseIndex);
                        double newAvg = (existingCourse.courseAverage + course.courseAverage) / 2;
                        existingCourse.courseAverage = newAvg;
                    }
                    coursesFromReportCard4.Add(course);
                }
                reportCardList[3] = new ReportCardList
                {
                    List = coursesFromReportCard4
                };
            }
            
            if (reportingPeriod >= 3)
            {
                var reportingPeriod3Courses = ReportCardScraping(reportCardDocument, 3);
                foreach (var course in reportingPeriod3Courses)
                {
                    if (coursesFromReportCard3.Contains(course))
                    {
                        var existingCourseIndex = coursesFromReportCard3.FindIndex(x => x.courseID == course.courseID);
                        Course existingCourse = coursesFromReportCard3.ElementAt(existingCourseIndex);
                        double newAvg = (existingCourse.courseAverage + course.courseAverage) / 2;
                        existingCourse.courseAverage = newAvg;
                    }
                    coursesFromReportCard3.Add(course);
                }
                reportCardList[2] = new ReportCardList
                {
                    List = coursesFromReportCard3
                };
            }

            if (reportingPeriod >= 2)
            {
                List<Course> reportingPeriod2Courses = ReportCardScraping(reportCardDocument, 2);
                foreach (Course course in reportingPeriod2Courses)
                {
                    if (coursesFromReportCard2.Contains(course))
                    {
                        var existingCourseIndex = coursesFromReportCard2.FindIndex(x => x.courseID == course.courseID);
                        Course existingCourse = coursesFromReportCard2.ElementAt(existingCourseIndex);
                        double newAvg = (existingCourse.courseAverage + course.courseAverage) / 2;
                        existingCourse.courseAverage = newAvg;
                    }
                    coursesFromReportCard2.Add(course);
                }
                reportCardList[1] = new ReportCardList
                {
                    List = coursesFromReportCard2
                };
            }

            if (reportingPeriod >= 1)
            {
                List<Course> reportingPeriod1Courses = ReportCardScraping(reportCardDocument, 1);
                foreach (Course course in reportingPeriod1Courses)
                {
                    coursesFromReportCard1.Add(course);
                }
                reportCardList[0] = new ReportCardList
                {
                    List = coursesFromReportCard1
                };
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
                    .Where(node => node.GetAttributeValue("href", "")
                        .Equals($"#")).FirstOrDefault().InnerText.Trim();

                var courseID = reportCardCourse.Descendants("td") //gets course id
                    .FirstOrDefault().InnerText.Trim();

                var courseGrade = ""; //finalized course grade
                int elementNumber = 0;
                switch (markingPeriod)
                {
                    case 1:
                        elementNumber = 2;
                        break;
                    case 2:
                        elementNumber = 4;
                        break;
                    case 3:
                        elementNumber = 5;
                        break;
                    case 4:
                        elementNumber = 7;
                        break;
                }

                List<string> grades = new List<string>();

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
                        courseName = courseName.TrimEnd(courseName[courseName.Length - 1]);
                    }
                }

                courseID = courseID.Remove(courseID.Length - 4);

                //removes excess
                while (courseID.LastOrDefault() == ' ' || courseID.LastOrDefault() == '-' ||
                       courseID.LastOrDefault() == 'A' || courseID.LastOrDefault() == 'B')
                {
                    courseID = courseID.TrimEnd(courseID[courseID.Length - 1]);
                }

                var avg = 0;
                foreach (var grade in grades)
                    avg += int.Parse(grade.Trim());
                avg /= grades.Count;
                courseGrade = avg.ToString();
                reportCardAssignmentList.Add(new Course
                {
                    courseID = courseID,
                    courseName = courseName,
                    courseAverage = double.Parse(courseGrade)
                }); //turns the grade (string) received into a double 
            }

            return reportCardAssignmentList;
        }
    }
}