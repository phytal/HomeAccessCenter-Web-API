using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HAC.API.Data.Objects;
using HAC.API.Helpers;
using HtmlAgilityPack;

namespace HAC.API.Data {
    public static class Courses {
        public static List<List<AssignmentCourse>> GetAssignmentsFromMarkingPeriod(CookieContainer cookies,
            Uri requestUri, string link) {
            var courseList = new List<List<AssignmentCourse>>();
            var documentList = new List<HtmlDocument>();
            var data = Utils.GetData(cookies, requestUri, link, ResponseType.Assignments);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(data.Result);

            var form = new GradesForm(htmlDocument);
            var reportingPeriodNames = form.ReportingPeriodNames();
            foreach (var name in reportingPeriodNames) {
                var body = form.GenerateFormBody(name);
                var response = Utils.GetDataWithBody(cookies, requestUri, link, ResponseType.Assignments, body);
                var doc = new HtmlDocument();
                doc.LoadHtml(response.Result);
                documentList.Add(doc);
            }

            foreach (var (htmlDocument1, run) in documentList.WithIndex()) {
                var localCourseList = new List<AssignmentCourse>();
                var courseHtml = htmlDocument1.DocumentNode.Descendants("div")
                    .Where(node => node.GetAttributeValue("class", "")
                        .Equals("AssignmentClass")).ToList();

                foreach (var (htmlNode, i) in courseHtml.WithIndex()) {
                    var courseNode = htmlNode.Descendants("a")
                        .FirstOrDefault(node => node.GetAttributeValue("class", "")
                            .Equals("sg-header-heading"));

                    if (courseNode == null) continue;

                    //gets teacher information (name and room)
                    //gets section key
                    var sectionKey = courseNode.Attributes["onclick"].Value;

                    sectionKey = Regex.Match(sectionKey, @"\d+").Groups[0].Value;
                    var param = $"?section_key={sectionKey}&rcrun={run + 1}";

                    //gets html for popup
                    var popUpData = Utils.GetData(cookies, requestUri, link, ResponseType.ClassPopUp, "Student", param);
                    var popUpDoc = new HtmlDocument();
                    popUpDoc.LoadHtml(popUpData.Result);

                    //extracts info
                    var sessionInfo = popUpDoc.GetElementbyId("plnMain_dgSessionInfo");
                    var sessionData = sessionInfo.ChildNodes[2].ChildNodes;
                    var teacherName = Utils.FormatName(sessionData[1].InnerText.Trim(), true);
                    var teacherRoom = sessionData[2].InnerText.Trim();

                    //gets and formats course info
                    var course = courseNode.InnerText.Trim();
                    var x = new Regex(@"\w+\s-\s\d\s");

                    var courseName = x.Replace(course, @"").Trim();
                    var courseId = x.Match(course).ToString().Trim();

                    var courseInfo = Utils.BeautifyCourseInfo(courseName, courseId);
                    (courseName, courseId) = courseInfo;

                    string courseGrade;
                    try {
                        courseGrade = htmlDocument1
                            .GetElementbyId($"plnMain_rptAssigmnetsByCourse_lblOverallAverage_{i}")
                            .InnerText.Trim();
                    }
                    catch {
                        continue;
                    }

                    //Gets grading information
                    //Prone to error as this typo is a hac problem
                    var courseInfoTable =
                        htmlDocument1.GetElementbyId($"plnMain_rptAssigmnetsByCourse_dgCourseCategories_{i}");
                    var gradeData = courseInfoTable.Descendants("tr").Where(node => node.GetAttributeValue("class", "")
                        .Equals("sg-asp-table-data-row"));

                    string gradeType = null;
                    double totalPointsMax, totalPointsPercent, gradeScaleTotal, gradeScaleEarned;
                    var totalPointsEarned =
                        totalPointsMax = totalPointsPercent = gradeScaleTotal = gradeScaleEarned = 0;

                    foreach (var gradeInput in gradeData) {
                        var gradeInputNodes = gradeInput.ChildNodes;
                        gradeType = gradeInputNodes[1].InnerText;
                        totalPointsEarned = double.Parse(gradeInputNodes[2].InnerText);
                        totalPointsMax = double.Parse(gradeInputNodes[3].InnerText);
                        totalPointsPercent = double.Parse(gradeInputNodes[4].InnerText.TrimEnd('%'));
                        gradeScaleTotal = double.Parse(gradeInputNodes[5].InnerText);
                        gradeScaleEarned = double.Parse(gradeInputNodes[6].InnerText);
                    }

                    var gradeInfo = new GradeInfo {
                        GradeType = gradeType,
                        TotalPointsEarned = totalPointsEarned,
                        TotalPointsMax = totalPointsMax,
                        TotalPointsPercent = totalPointsPercent,
                        GradeScaleTotal = gradeScaleTotal,
                        GradeScaleEarned = gradeScaleEarned
                    };

                    var assignmentList = new List<Assignment>();

                    //gets all the assignments for a course
                    var assignmentTable = htmlNode.Descendants("table").FirstOrDefault(node =>
                        node.GetAttributeValue("class", "").Equals("sg-asp-table"));

                    foreach (var assignmentNode in assignmentTable.Descendants("tr").Where(node =>
                        node.GetAttributeValue("class", "").Equals("sg-asp-table-data-row"))) {
                        var assignment = new Assignment();
                        //Regex pattern = new Regex(".+:\\s");
                        var assignmentData = assignmentNode.ChildNodes[3].Descendants("a").FirstOrDefault()
                            .Attributes["title"].Value;
                        var parsedAssignmentData = Regex.Replace(assignmentData, ".+:", "").Trim();


                        foreach (var (item, index) in new LineReader(() => new StringReader(parsedAssignmentData))
                            .WithIndex())
                            switch (index) {
                                case 0:
                                    assignment.Title = item.Trim();
                                    break;
                                case 1:
                                    assignment.Name = FixAssignmentTitle(item.Trim());
                                    break;
                                case 2:
                                    assignment.Category = item.Trim();
                                    break;
                                case 3:
                                    var date = DateTime.Parse(item.Trim());
                                    assignment.DueDate = date;
                                    break;
                                case 4:
                                    assignment.MaxPoints = double.Parse(item.Trim());
                                    break;
                                case 5:
                                    assignment.CanBeDropped = item.Contains("Y");
                                    break;
                                case 6:
                                    assignment.ExtraCredit = item.Contains("Y");
                                    break;
                                case 7:
                                    assignment.HasAttachments = item.Contains("Y");
                                    break;
                            }

                        var score = assignmentNode.ChildNodes[5].InnerText.Trim();

                        assignment.Status = score switch {
                            "M" => AssignmentStatus.Missing,
                            "I" => AssignmentStatus.Incomplete,
                            "EXC" => AssignmentStatus.Excused,
                            "L" => AssignmentStatus.Late,
                            _ => AssignmentStatus.Upcoming
                        };

                        if (double.TryParse(score, out var points))
                            assignment.Status = AssignmentStatus.Complete;

                        assignment.Score = points.ToString(CultureInfo.CurrentCulture);
                        assignmentList.Add(assignment);
                    }

                    localCourseList.Add(new AssignmentCourse {
                        CourseName = courseName, CourseId = courseId, CourseAverage = double.Parse(courseGrade),
                        Assignments = assignmentList, GradeInfo = gradeInfo, Teacher = teacherName,
                        RoomNumber = teacherRoom
                    });
                }

                courseList.Add(localCourseList);
            }

            return courseList;
        }

        //Update as needed
        private static string FixAssignmentTitle(string s) {
            var symbols = new Dictionary<string, string> {
                {"&quot;", "\""},
                {"&amp;", "&"}
            };

            foreach (var symbol in symbols.Keys) s = s.Replace(symbol, symbols[symbol]);

            return s;
        }
    }
}