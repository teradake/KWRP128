using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Backend.Model.Paramter
{
    public class PairingParameterModel
    {
        [JsonPropertyName("CountMin")]
        public int CountMin { get; init; }

        [JsonPropertyName("CountMax")]
        public int CountMax { get; init; }
    }
}
