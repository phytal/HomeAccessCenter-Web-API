using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAC.API.Data {
    public static class Utils {
        public static string PercentEncoder(string s) {
            var reservedCharacters = new Dictionary<string, string> {
                {"!", "%21"},
                {"#", "%23"},
                {"$", "%24"},
                {"%", "%25"},
                {"&", "%26"},
                {"'", "%27"},
                {"(", "%28"},
                {")", "%29"},
                {"*", "%2A"},
                {"+", "%2B"},
                {",", "%2C"},
                {"/", "%2F"},
                {":", "%3A"},
                {";", "%3B"},
                {"=", "%3D"},
                {"?", "%3F"},
                {"@", "%40"},
                {"[", "%5B"},
                {"]", "%5D"},
                {" ", "+"}
            };
            foreach (var character in reservedCharacters.Keys) s = s.Replace(character, reservedCharacters[character]);

            return s;
        }

        /// <summary>
        ///     Returns the course information
        /// </summary>
        /// <param name="courseName">Course Name</param>
        /// <param name="courseId">Course Id</param>
        /// <returns>Returns course name, course id</returns>
        public static Tuple<string, string> BeautifyCourseInfo(string courseName = null, string courseId = null) {
            if (courseName != null) //removes semester 
                while (courseName.Substring(courseName.Length - 2) == "S1" ||
                       courseName.Substring(courseName.Length - 2) == "S2") {
                    courseName = courseName.Replace(courseName.Substring(courseName.Length - 2), "");
                    while (courseName.LastOrDefault() == ' ' || courseName.LastOrDefault() == '-')
                        courseName = courseName.TrimEnd(courseName[^1]);
                }

            if (courseId != null) {
                courseId = courseId.Remove(courseId.Length - 4);
                //removes excess
                while (courseId.LastOrDefault() == ' ' || courseId.LastOrDefault() == '-' ||
                       courseId.LastOrDefault() == 'A' || courseId.LastOrDefault() == 'B')
                    courseId = courseId.TrimEnd(courseId[^1]);
            }

            return new Tuple<string, string>(courseName, courseId);
        }

        public static string FormatName(string fullName, bool formal) {
            var firstMiddleName = fullName.Split(',')[1].Trim().ToLower();
            var fmName = firstMiddleName.Split(' ');
            var firstNameBuilder = new StringBuilder();
            foreach (var name in fmName) firstNameBuilder.Append(char.ToUpper(name[0]) + name.Substring(1) + " ");

            var firstName = firstNameBuilder.ToString().TrimEnd(' ');
            var lastName = fullName.Split(',')[0].Trim().ToLower();
            lastName = char.ToUpper(lastName[0]) + lastName.Substring(1);
            if (formal)
                fullName = lastName + ", " + firstName;
            else
                fullName = firstName + " " + lastName;

            return fullName;
        }
    }
}