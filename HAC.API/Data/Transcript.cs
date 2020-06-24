using System;
using System.Collections.Generic;
using System.Linq;
using HAC.API.Data.Objects;
using HtmlAgilityPack;

namespace HAC.API.Data
{
    public static class Transcript
    {
        public static List<List<TranscriptCourse>> GetTranscript(string data)
        {
            var oldAssignmentList = new List<List<TranscriptCourse>>();
            var oldHtmlDocument = new HtmlDocument();
            oldHtmlDocument.LoadHtml(data); //gets all of the years
            var transcriptGroups = oldHtmlDocument.DocumentNode.SelectNodes("//td[@class='sg-transcript-group']");
            foreach (var group in transcriptGroups)
            {
                var yearlyAssignmentList = new List<TranscriptCourse>();
                var oldCourseHtml = group.Descendants("table")
                    .Where(node => node.GetAttributeValue("class", "")
                        .Equals($"sg-asp-table")).ToList();
                var oldCourseItemList = oldCourseHtml[0].Descendants("tr")
                    .Where(node => node.GetAttributeValue("class", "")
                        .Equals($"sg-asp-table-data-row")).ToList();
                foreach (var courseHtmlItem in oldCourseItemList) //foreach course that is listed in a year
                {
                    var courseName = courseHtmlItem.Descendants("td") //gets course name
                        .ElementAt(1).InnerText
                        .Trim(); //course name is stored at the second instance of td

                    var courseId = courseHtmlItem.Descendants("td") //gets course id
                        .ElementAt(0).InnerText
                        .Trim(); //course name is stored at the first instance of td

                    courseId = courseId.Remove(courseId.Length - 4);

                    courseName = Utils.BeautifyCourseInfo(courseName).Item1;
                    
                    while (courseId.LastOrDefault() == ' ' || courseId.LastOrDefault() == '-' ||
                           courseId.LastOrDefault() == 'A' || courseId.LastOrDefault() == 'B' ||
                           courseId.LastOrDefault() == 'Y' || courseId.LastOrDefault() == 'M')
                    {
                        courseId = courseId.TrimEnd(courseId[^1]);
                    }
                    
                    //total credit of the course
                    var courseCredit = double.Parse(courseHtmlItem.Descendants("td") //gets course grade
                        .ElementAt(5).InnerText);
                    
                    var courseGrade = 0.0;
                    
                    var courseFinalGradeHtml = courseHtmlItem.Descendants("td") //gets course grade
                        .ElementAt(4).InnerText;

                    if (courseFinalGradeHtml == "&nbsp;")
                    {
                        double firstSem, secondSem = 0;
                        for (byte j = 3; j <= 3 && j > 1; j--)
                            //gets grade, starts from second semester avg, if nothing, goes to the first semester
                        {
                            var courseGradeHtml = courseHtmlItem.Descendants("td") //gets course grade
                                .ElementAt(j).InnerText;

                            if (j == 3 && courseGradeHtml != "&nbsp;")
                            {
                                if (courseGradeHtml.Trim() == "P")
                                {
                                    secondSem = -1;
                                    continue;
                                }

                                secondSem = double.Parse(courseGradeHtml);
                            }
                            else if (j == 2)
                            {
                                if (courseGradeHtml != "&nbsp;")
                                {
                                    if (courseGradeHtml.Trim() == "P")
                                    {
                                        if (secondSem <= 0)
                                            courseGrade = -1;
                                        else courseGrade = secondSem;
                                        break;
                                    }

                                    firstSem = double.Parse(courseGradeHtml);
                                    if (secondSem > 0)
                                        courseGrade = (firstSem + secondSem) / 2;
                                    else courseGrade = firstSem;
                                }
                                
                                else courseGrade = secondSem;
                            }
                        }
                    }
                    else courseGrade = double.Parse(courseFinalGradeHtml);

                        yearlyAssignmentList.Add(new TranscriptCourse()
                    {
                        CourseName = courseName,
                        CourseId = courseId,
                        CourseAverage = courseGrade,
                        CourseCredit = courseCredit
                    });
                }
                oldAssignmentList.Add(yearlyAssignmentList);
            }
            
            return oldAssignmentList;
        }
    }
}