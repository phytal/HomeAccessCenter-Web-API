using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace HAC.API.Data.Forms {
    public class IprForm {
        private readonly Dictionary<string, string> _iprDateValues;
        private readonly Dictionary<string, string> _runChangeForm;

        public IprForm(HtmlDocument document) {
            _iprDateValues = new Dictionary<string, string>();
            _runChangeForm = new Dictionary<string, string>();
            InitReportingRunValues(document);
            InitRunChangeForm(document);
        }

        public IEnumerable<string> IprDateNames() {
            return _iprDateValues.Keys.ToList();
        }

        private void InitReportingRunValues(HtmlDocument document) {
            var i = 0;
            foreach (var runOption in document.GetElementbyId("plnMain_ddlIPRDates").ChildNodes) {
                i++;
                if (i % 2 == 1) continue;
                _iprDateValues[runOption.InnerHtml.Trim()] = runOption.Attributes["value"].Value;
            }
        }

        public string GenerateFormBody(string reportingPeriodName) {
            _runChangeForm["ctl00%24plnMain%24ddlIPRDates"] = Utils.PercentEncoder(_iprDateValues[reportingPeriodName]);
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

            _runChangeForm["__EVENTTARGET"] = "ctl00%24plnMain%24ddlIPRDates";
        }
    }
}