using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace HAC.API.Data
{
    public class IprForm
    {
        private readonly Dictionary<string, string> iprDateValues;
        private readonly Dictionary<string, string> runChangeForm;
    
        public IprForm(HtmlDocument document)
        {
            iprDateValues = new Dictionary<string, string>();
            runChangeForm = new Dictionary<string, string>();
            InitReportingRunValues(document);
            InitRunChangeForm(document);
        }
        
        public IEnumerable<string> IprDateNames()
        {
            return iprDateValues.Keys.ToList();
        }

        private void InitReportingRunValues(HtmlDocument document)
        {
            int i = 0;
            foreach (var runOption in document.GetElementbyId("plnMain_ddlIPRDates").ChildNodes)
            {
                i++;
                if (i % 2 == 1) continue;
                iprDateValues[runOption.InnerHtml.Trim()] = runOption.Attributes["value"].Value;
            }
        }

        public string GenerateFormBody(string reportingPeriodName)
        {
            runChangeForm["ctl00%24plnMain%24ddlIPRDates"] = CharacterCoded(iprDateValues[reportingPeriodName]);
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
                runChangeForm[CharacterCoded(input.Attributes["name"].Value)] = CharacterCoded(input.Attributes["value"].Value);
            }
            
            runChangeForm["__EVENTTARGET"] = "ctl00%24plnMain%24ddlIPRDates";
        }

        private string CharacterCoded(string s)
        {
            var charToCode = new Dictionary<string, string>
            {
                {"$", "%24"},
                {"=", "%3D"},
                {"+", "%2B"},
                {"/", "%2F"},
                {" ", "+"},
                {"(", "%28"},
                {")", "%29"},
                {"'", "%27"},
                {":", "%3A"}
            };
            foreach (var character in charToCode.Keys)
            {
                s = s.Replace(character, charToCode[character]);
            }

            return s;
        }
    }
}