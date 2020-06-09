using System;
using System.Collections.Generic;
using System.Linq;
using HAC.API.Data.Objects;
using HtmlAgilityPack;

namespace HAC.API.Data
{
    public static class Transcript
    {
        public static List<Course> GetTranscript(string data)
        {
            var oldAssignmentList = new List<Course>();
            var oldHtmlDocument = new HtmlDocument();
            oldHtmlDocument.LoadHtml(data); //gets all of the years
            byte numCourses = 0; //no one will exceed 255 
            for (byte i = 0; i >= 0; i++)
                //what this does is tries to get as many courses as possible until it receives an error, then pass on that number to get that number of courses
            {
                try //get number of years
                {
                    var oldCourseHtml = oldHtmlDocument.DocumentNode
                        .Descendants("table") //initializes variable but isn't used purposely for testing purposes
                        .Where(node => node.GetAttributeValue("id", "")
                            .Equals($"plnMain_rpTranscriptGroup_dgCourses_{i}")).ToList();
                    if (oldCourseHtml.Count == 0) break;
                }
                catch (Exception)
                {
                    break;
                }

                numCourses = i;
            }

            //note: numCourses will always be 1 less because of indexes
            for (byte i = 0; i <= numCourses - 1; i++) //get all years (-1 to exclude the present year)
            {
                var oldCourseHtml = oldHtmlDocument.DocumentNode.Descendants("table")
                    .Where(node => node.GetAttributeValue("id", "")
                        .Equals($"plnMain_rpTranscriptGroup_dgCourses_{i}")).ToList();
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

                    while (courseId.LastOrDefault() == ' ' || courseId.LastOrDefault() == '-' ||
                           courseId.LastOrDefault() == 'A' || courseId.LastOrDefault() == 'B' ||
                           courseId.LastOrDefault() == 'Y' || courseId.LastOrDefault() == 'M')
                    {
                        courseId = courseId.TrimEnd(courseId[^1]);
                    }

                    for (byte j = 4; j <= 4 && j > 1; j--)
                        //gets grade, starts from last element, which is overall avg, if nothing, goes to second semester, then first semester
                    {
                        var courseGrade = ""; //finalized course grade
                        var courseGradeHtml = courseHtmlItem.Descendants("td") //gets course grade
                            .ElementAt(j).InnerText;

                        if (courseGradeHtml == "&nbsp;")
                        {
                            continue; //if it is not a grade and is empty then retry
                        }

                        if (j == 3)
                            break; //if the course avg is available then you dont need to get the semester grades
                        courseGrade = courseGradeHtml;
                        oldAssignmentList.Add(new Course
                        {
                            CourseName = courseName,
                            CourseId = courseId,
                            CourseAverage = double.Parse(courseGrade)
                        }); //turns the grade (string) received into a double 
                    }
                }
            }
            return oldAssignmentList;
        }
    }
}