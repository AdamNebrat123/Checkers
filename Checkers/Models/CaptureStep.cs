using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class CaptureStep
    {
        public int CapturedRow { get; set; }
        public int CapturedCol { get; set; }
        public int LandingRow { get; set; }
        public int LandingCol { get; set; }
    }
}
