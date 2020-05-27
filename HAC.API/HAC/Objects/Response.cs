using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAC.API.HAC.Objects
{
    public class Response
    {
        public string Message { get; set; }
        public CurrentAssignmentList CurrentAssignmentList { get; set; }
        public OldAssignmentList OldAssignmentList { get; set; }
        public ReportCardList ReportCardList1 { get; set; }
        public ReportCardList ReportCardList2 { get; set; }
        public ReportCardList ReportCardList3 { get; set; }
        public ReportCardList ReportCardList4 { get; set; }
    }
}
