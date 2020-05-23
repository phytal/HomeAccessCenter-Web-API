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
        public ReportCardList ReportCardList { get; set; }
    }
}
