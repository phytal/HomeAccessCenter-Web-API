using System.Collections.Generic;

namespace HAC.API.Data.Objects
{
    public class GradeInfo
    {
        public string GradeType { get; set; }
        public double TotalPointsEarned { get; set; }
        public double TotalPointsMax { get; set; }
        public double TotalPointsPercent { get; set; }
        public double GradeScaleTotal { get; set; }
        public double GradeScaleEarned { get; set; }
        
        // <td>Major Grades</td>
        // <td>630.0000</td>
        // <td>700.00</td>
        // <td>90.000%</td>
        // <td>60.00</td>
        // <td>54.00000</td>
    }
}