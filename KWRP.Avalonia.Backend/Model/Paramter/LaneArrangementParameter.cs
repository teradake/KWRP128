using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Backend.Model.Paramter
{
    public class LaneArrangementParameter
    {
        [JsonPropertyName("Description")]
        public string Description { get; set; } = "レーン割に使用するパラメータ";

        [JsonPropertyName("LaneDivisionParameter")]
        public VRParameterModel VrParams { get; set; }

        [JsonPropertyName("LanePairingParameter")]
        public PairingParameterModel PairingParams { get; set; }
    }
}
