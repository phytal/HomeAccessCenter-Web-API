using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace HAC.API.Data {
    public class AttendanceForm {
        private readonly List<string> _monthKeys;

        private readonly Dictionary<string, int> _months = new Dictionary<string, int> {
            {"January", 1},
            {"February", 2},
            {"March", 3},
            {"April", 4},
            {"May", 5},
            {"June", 6},
            {"July", 7},
            {"August", 8},
            {"September", 9},
            {"October", 10},
            {"November", 11},
            {"December", 12}
        };

        private readonly Dictionary<string, string> _runChangeForm;

        public AttendanceForm(HtmlDocument document) {
            _monthKeys = new List<string>();
            _runChangeForm = new Dictionary<string, string>();
            InitMonthKeys(document);
            InitRunChangeForm(document);
        }

        public IEnumerable<string> MonthKeys() {
            return _monthKeys;
        }

        private void InitMonthKeys(HtmlDocument document) {
            var calendarInfo = document.DocumentNode.Descendants("table")
                .FirstOrDefault(node => node.GetAttributeValue("class", "").Equals("sg-asp-calendar-header"));

            if (calendarInfo == null) return;

            var monthInfo = calendarInfo.ChildNodes[1].InnerText.Split(' ');

            var month = monthInfo[0].Replace("&lt;", " ").Trim();
            var year = int.Parse(monthInfo[1]);
            var monthNumber = _months[month];
            var daysInPrevious = DateTime.DaysInMonth(year, monthNumber - 1);

            var regex = new Regex(@"\('ctl00\$plnMain\$cldAttendance','([A-Za-z0-9\-]+)'\)$");
            var previousMonthKey = calendarInfo.ChildNodes[1].FirstChild.FirstChild.FirstChild.Attributes["href"].Value;
            var match = regex.Match(previousMonthKey);
            if (match.Success) previousMonthKey = match.Groups[1].Value;

            string currentMonthKey;
            char previousMonthPrefix;
            if (int.TryParse(previousMonthKey.Substring(1), out var previousMonthKeyNumber)) {
                var currentMonthKeyNumber = previousMonthKeyNumber + daysInPrevious;
                previousMonthPrefix = previousMonthKey[0];
                currentMonthKey = previousMonthPrefix.ToString() + currentMonthKeyNumber;
            }
            else {
                throw new FormatException("Month key in the HTML is in the incorrect format!\n" +
                                          calendarInfo.InnerHtml);
            }

            var nextMonthKey = calendarInfo.ChildNodes[1].LastChild.FirstChild.FirstChild.Attributes["href"].Value;
            match = regex.Match(nextMonthKey);
            if (match.Success) nextMonthKey = match.Groups[1].Value;

            //_monthKeys.Add(nextMonthKey);
            _monthKeys.Add(currentMonthKey);
            _monthKeys.Add(previousMonthKey);

            var workableNumber = previousMonthKeyNumber;
            //Get the keys from previous months until August
            if (monthNumber < 8) {
                for (var i = monthNumber - 2; i >= 1; i--) {
                    var daysInMonth = DateTime.DaysInMonth(year, i);
                    workableNumber -= daysInMonth;
                    _monthKeys.Add(previousMonthPrefix + workableNumber.ToString());
                }

                for (var i = 12; i >= 8; i--) {
                    var daysInMonth = DateTime.DaysInMonth(year, i);
                    workableNumber -= daysInMonth;
                    _monthKeys.Add(previousMonthPrefix + workableNumber.ToString());
                }
            }
            else if (monthNumber > 9) {
                for (var i = monthNumber - 2; i >= 8; i--) {
                    var daysInMonth = DateTime.DaysInMonth(year, i);
                    workableNumber -= daysInMonth;
                    _monthKeys.Add(previousMonthPrefix + workableNumber.ToString());
                }
            }
        }

        public string GenerateFormBody(string monthKey) {
            _runChangeForm["__EVENTARGUMENT"] = Utils.PercentEncoder(monthKey);
            var bodyBuilder = new StringBuilder();
            foreach (var (key, value) in _runChangeForm) {
                bodyBuilder.Append(key + "=" + value);
                bodyBuilder.Append("&");
            }

            return bodyBuilder.ToString().Remove(bodyBuilder.Length - 1);
        }

        private void InitRunChangeForm(HtmlDocument document) {
            var headers = document.DocumentNode.Descendants("input");

            //var headers = headersList.SelectMany(node => node.Descendants("input")).ToList();
            foreach (var input in headers) {
                string value;
                try {
                    value = input.Attributes["value"].Value;
                }
                catch {
                    value = "";
                }

                _runChangeForm[Utils.PercentEncoder(input.Attributes["name"].Value)] = Utils.PercentEncoder(value);
            }

            _runChangeForm["__EVENTTARGET"] = "ctl00%24plnMain%24cldAttendance";
        }
    }
}