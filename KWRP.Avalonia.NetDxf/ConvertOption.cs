using System.Text.Json.Serialization;
using Trdk.Geometry;

namespace KWRP.Avalonia.NetDxf
{
    public class DxfConverterOption
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Title { get; set; }
        public string? DxfFilePath { get; set; }
        public int CenterX { get; set; }
        public int CenterY { get; set; }
        public int MapHeight { get; set; }
        public int MapWidth { get; set; }
        public int PixelsPerMeter { get; set; }

        [JsonIgnore] public int Xmin => CenterX - MapWidth / 2;
        [JsonIgnore] public int Ymin => CenterY - MapHeight / 2;
    }

    public class DxfHistoryFile
    {
        public Guid? SelectedId { get; set; }
        public List<DxfConverterOption> Items { get; set; } = [];
    }
}
