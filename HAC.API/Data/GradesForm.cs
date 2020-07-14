using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace HAC.API.Data
{
    public class GradesForm
    {
        private readonly Dictionary<string, string> reportingPeriodValues;
        private readonly Dictionary<string, string> runChangeForm;
    
        public GradesForm(HtmlDocument document)
        {
            reportingPeriodValues = new Dictionary<string, string>();
            runChangeForm = new Dictionary<string, string>();
            InitReportingRunValues(document);
            InitRunChangeForm(document);
        }
        
        public IEnumerable<string> ReportingPeriodNames()
        {
            return reportingPeriodValues.Keys.ToList();
        }

        private void InitReportingRunValues(HtmlDocument document)
        {
            int i = 0;
            foreach (var runOption in document.GetElementbyId("plnMain_ddlReportCardRuns").ChildNodes)
            {
                i++;
                if (i % 2 == 1) continue;
                reportingPeriodValues[runOption.InnerHtml.Trim()] = runOption.Attributes["value"].Value;
            }

            var item = reportingPeriodValues.First(kvp => kvp.Value == "ALL");
            reportingPeriodValues.Remove(item.Key);
        }

        public string GenerateFormBody(string reportingPeriodName)
        {
            runChangeForm["ctl00%24plnMain%24ddlReportCardRuns"] = reportingPeriodValues[reportingPeriodName];
            var bodyBuilder = new StringBuilder();
            foreach (var entry in runChangeForm)
            {
                bodyBuilder.Append(entry.Key + "=" + entry.Value);
                bodyBuilder.Append("&");
            }
            return bodyBuilder.ToString().Remove(bodyBuilder.Length - 1);
        }

        private void InitRunChangeForm(HtmlDocument document)
        {
            foreach (var input in document.DocumentNode.Descendants("input"))
            {
                runChangeForm[Utils.PercentEncoder(input.Attributes["name"].Value)] = Utils.PercentEncoder(input.Attributes["value"].Value);
            }

            runChangeForm["ctl00%24plnMain%24ddlReportCardRuns"] = "";
            runChangeForm["ctl00%24plnMain%24ddlClasses"] = "ALL";
            runChangeForm["ctl00%24plnMain%24ddlCompetencies"] = "ALL";
            runChangeForm["ctl00%24plnMain%24ddlOrderBy"] = "Class";
            runChangeForm["__EVENTTARGET"] = "ctl00%24plnMain%24btnRefreshView";
        }
    }
}