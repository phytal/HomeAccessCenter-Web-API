using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HAC.API.Data.Objects;
using HtmlAgilityPack;
using static System.String;

namespace HAC.API.Data {
    public static class Ipr {
        public static List<List<Course>> GetGradesFromIpr(CookieContainer cookies, Uri requestUri, string link) {
            var iprList = new List<List<Course>>();
            var documentList = new List<HtmlDocument>();
            var data = Utils.GetData(cookies, requestUri, link, ResponseType.InterimProgress);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(data.Result);

            var form = new IprForm(htmlDocument);
            var iprDateNames = form.IprDateNames();
            foreach (var name in iprDateNames) {
                var body = form.GenerateFormBody(name);
                var response = Utils.GetDataWithBody(cookies, requestUri, link, ResponseType.InterimProgress, body);
                var doc = new HtmlDocument();
                doc.LoadHtml(response.Result);
                documentList.Add(doc);
            }

            foreach (var document in documentList) {
                var courseList = new List<Course>();
                var iprTable = document.GetElementbyId("plnMain_dgIPR");
                var iprCourses = iprTable.ChildNodes.Where(node => node.GetAttributeValue("class", "")
                    .Equals("sg-asp-table-data-row")).ToList();
                foreach (var iprCourse in iprCourses) //foreach course
                {
                    var courseNameNode = iprCourse.Descendants("a") //gets course name
                        .FirstOrDefault(node => node.GetAttributeValue("href", "")
                            .Equals("#"));

                    var courseIdNode = iprCourse.Descendants("td") //gets course id
                        .FirstOrDefault();

                    string courseName;
                    var courseId = courseName = Empty;

                    if (courseNameNode != null && courseIdNode != null) {
                        courseName = courseNameNode.InnerText.Trim();
                        courseId = courseIdNode.InnerText.Trim();
                    }

                    var grade = iprCourse.Descendants("td") //gets course grade
                        .ElementAt(5).InnerText.Trim();

                    var courseInfo = Utils.BeautifyCourseInfo(courseName, courseId);
                    (courseName, courseId) = courseInfo;

                    courseList.Add(new Course {
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