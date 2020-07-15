using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using HAC.API.Data.Objects;
using HAC.API.Helpers;
using HtmlAgilityPack;

namespace HAC.API.Data {
    public static class Attendance {
        private static readonly string[] Names =
            {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"};

        public static List<List<List<Day>>> GetAttendances(CookieContainer cookies, Uri requestUri, string link) {
            var calendarList = new List<List<List<Day>>>();
            var documentList = new List<HtmlDocument>();
            var data = Utils.GetData(cookies, requestUri, link, ResponseType.MonthlyView, "Attendance");

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(data.Result);

            var form = new AttendanceForm(htmlDocument);
            var monthKeys = form.MonthKeys();
            foreach (var key in monthKeys) {
                var body = form.GenerateFormBody(key);
                var response = Utils.GetDataWithBody(cookies, requestUri, link, ResponseType.MonthlyView, body,
                    "Attendance");
                var doc = new HtmlDocument();
                doc.LoadHtml(response.Result);
                documentList.Add(doc);
            }

            foreach (var document in documentList) {
                var calendar = new List<List<Day>>();
                var attendanceTable = document.GetElementbyId("plnMain_cldAttendance");
                var weeks = attendanceTable.ChildNodes.ToList();
                weeks.RemoveRange(0, 3);
                weeks.RemoveRange(weeks.Count - 2, 2);
                foreach (var week in weeks) //foreach course
                {
                    var calendarWeek = new List<Day>();
                    foreach (var (item, index) in week.ChildNodes.WithIndex()) {
                        if (!int.TryParse(item.InnerText, out var date)) break;

                        var attendances = new List<AttendanceRecord>();
                        var dayAttrs = item.Attributes;
                        if (dayAttrs.Contains("title")) {
                            var records = dayAttrs["title"].Value;
                            foreach (var line in new LineReader(() => new StringReader(records)))
                                if (attendances[^1].Reason == null)
                                    attendances[^1].Reason = line;

                                else if (int.TryParse(line, out var period))
                                    attendances.Add(new AttendanceRecord {
                                        Period = period
                                    });

                                else
                                    attendances[^1].Note = line;
                        }

                        var calendarDay = new Day {
                            Date = date,
                            DayName = Names[index],
                            DayOff = dayAttrs["style"].Value.Contains("background-color:#CCCCCC"),
                            Attendances = attendances
                        };
                        calendarWeek.Add(calendarDay);
                    }

                    calendar.Add(calendarWeek); //turns the grade (string) received into a double 
                }

                calendarList.Add(calendar);
            }

            return calendarList;
        }
    }
}