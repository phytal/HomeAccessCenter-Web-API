using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

namespace HAC.API.Data.Objects
{
    public static class Ipr
    {
        public static List<List<Course>> GetGradesFromIpr(CookieContainer cookies, Uri requestUri, string link)
        {
            var iprList = new List<List<Course>>();
            var documentList = new List<HtmlDocument>();
            string data = Utils.GetData(cookies, requestUri, link, ResponseType.InterimProgress);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(data);

            var form = new IprForm(htmlDocument);
            var iprDateNames = form.IprDateNames();
            foreach (var name in iprDateNames)
            {
                var body = form.GenerateFormBody(name);
                var response = Utils.GetDataFromIprDate(cookies, requestUri, link, body);
                var doc = new HtmlDocument();
                doc.LoadHtml(response);
                documentList.Add(doc);
            }

            foreach (var document in documentList)
            {
                List<Course> courseList = new List<Course>();
                var iprTable = document.GetElementbyId("plnMain_dgIPR");
                var iprCourses = iprTable.ChildNodes.Where(node => node.GetAttributeValue("class", "")
                        .Equals($"sg-asp-table-data-row")).ToList();
                foreach (var iprCourse in iprCourses) //foreach course
                {
                    var courseName = iprCourse.Descendants("a") //gets course name
                        .FirstOrDefault(node => node.GetAttributeValue("href", "")
                            .Equals("#")).InnerText.Trim();

                    var courseId = iprCourse.Descendants("td") //gets course id
                        .FirstOrDefault().InnerText.Trim();

                    var grade = iprCourse.Descendants("td") //gets course grade
                        .ElementAt(5).InnerText.Trim();

                    var courseInfo = Utils.BeautifyCourseInfo(courseName, courseId);
                    courseName = courseInfo.Item1;
                    courseId = courseInfo.Item2;
                    
                    courseList.Add(new Course
                    {
                        CourseId = courseId,
                        CourseName = courseName,
                        CourseAverage = double.Parse(grade)
                    }); //turns the grade (string) received into a double 
                }
                iprList.Add(courseList);
            }
            
            return iprList;
        }
    }
}