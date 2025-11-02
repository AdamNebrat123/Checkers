using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Checkers.GameLogic
{
    public class ModeParametersWrapper
    {
        public string Mode { get; set; } = string.Empty;
        public JsonElement Parameters { get; set; }
    }
}
