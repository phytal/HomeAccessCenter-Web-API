using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace HAC.API.Data {
    public class GradesForm {
        private readonly Dictionary<string, string> _reportingPeriodValues;
        private readonly Dictionary<string, string> _runChangeForm;

        public GradesForm(HtmlDocument document) {
            _reportingPeriodValues = new Dictionary<string, string>();
            _runChangeForm = new Dictionary<string, string>();
            InitReportingRunValues(document);
            InitRunChangeForm(document);
        }

        public IEnumerable<string> ReportingPeriodNames() {
            return _reportingPeriodValues.Keys.ToList();
        }

        private void InitReportingRunValues(HtmlDocument document) {
            var i = 0;
            foreach (var runOption in document.GetElementbyId("plnMain_ddlReportCardRuns").ChildNodes) {
                i++;
                if (i % 2 == 1) continue;
                _reportingPeriodValues[runOption.InnerHtml.Trim()] = runOption.Attributes["value"].Value;
            }

            var item = _reportingPeriodValues.First(kvp => kvp.Value == "ALL");
            _reportingPeriodValues.Remove(item.Key);
        }

        public string GenerateFormBody(string reportingPeriodName) {
            _runChangeForm["ctl00%24plnMain%24ddlReportCardRuns"] = _reportingPeriodValues[reportingPeriodName];
            var bodyBuilder = new StringBuilder();
            foreach (var (key, value) in _runChangeForm) {
                bodyBuilder.Append(key + "=" + value);
                bodyBuilder.Append("&");
            }

            return bodyBuilder.ToString().Remove(bodyBuilder.Length - 1);
        }

        private void InitRunChangeForm(HtmlDocument document) {
            foreach (var input in document.DocumentNode.Descendants("input"))
                _runChangeForm[Utils.PercentEncoder(input.Attributes["name"].Value)] =
                    Utils.PercentEncoder(input.Attributes["value"].Value);

            _runChangeForm["ctl00%24plnMain%24ddlReportCardRuns"] = "";
            _runChangeForm["ctl00%24plnMain%24ddlClasses"] = "ALL";
            _runChangeForm["ctl00%24plnMain%24ddlCompetencies"] = "ALL";
            _runChangeForm["ctl00%24plnMain%24ddlOrderBy"] = "Class";
            _runChangeForm["__EVENTTARGET"] = "ctl00%24plnMain%24btnRefreshView";
        }
    }
}