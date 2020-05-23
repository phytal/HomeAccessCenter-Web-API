using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAC.API.HAC.Objects
{
    public class CurrentAssignmentList : ICourseList
    {
        public IEnumerable<Course> List { get; set; }
    }
}
