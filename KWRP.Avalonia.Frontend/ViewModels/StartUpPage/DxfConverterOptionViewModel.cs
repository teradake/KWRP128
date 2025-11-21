using KWRP.Avalonia.NetDxf;
using System;
using System.IO;

namespace KWRP.Avalonia.Frontend.ViewModels.StartUpPage
{
    public class DxfConverterOptionViewModel : ViewModelBase
    {
        public DxfConverterOptionViewModel() { }
        public DxfConverterOptionViewModel(DxfConverterOption option)
        {
            Title = option.Title;
            CenterX = option.CenterX;
            CenterY = option.CenterY;
            MapHeight = option.MapHeight;
            MapWidth = option.MapWidth;
            PixelsPerMeter = option.PixelsPerMeter;
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        private string? _title;
        public string? Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(IsTitleValid));
            }
        }

        private string? _dxfFilePath;
        public string? DxfFilePath
        {
            get => _dxfFilePath;
            set
            {
                _dxfFilePath = value;
                OnPropertyChanged(nameof(DxfFilePath));
            }
        }

        public int CenterX { get; set; }
        public int CenterY { get; set; }

        private int _height;
        public int MapHeight
        {
            get => _height;
            set
            {
                _height = value;
                OnPropertyChanged(nameof(MapHeight));
                OnPropertyChanged(nameof(PngSize));
            }
        }

        private int _width;
        public int MapWidth
        {
            get => _width;
            set
            {
                _width = value;
                OnPropertyChanged(nameof(MapWidth));
                OnPropertyChanged(nameof(PngSize));
            }
        }

        private int _resolution;
        public int PixelsPerMeter
        {
            get => _resolution;
            set
            {
                _resolution = value;
                OnPropertyChanged(nameof(PixelsPerMeter));
                OnPropertyChanged(nameof(PngSize));
            }
        }

        public string PngSize => $"{MapWidth * PixelsPerMeter}x{MapHeight * PixelsPerMeter}";
        public bool IsTitleValid =>
            !string.IsNullOrWhiteSpace(Title) 
            && Title.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;

        public DxfConverterOption ToModel() =>
            new DxfConverterOption
            {
                Id = this.Id,
                Title = Title,
                DxfFilePath = DxfFilePath,
                CenterX = CenterX,
                CenterY = CenterY,
                MapHeight = MapHeight,
                MapWidth = MapWidth,
                PixelsPerMeter = PixelsPerMeter
            };

        public static DxfConverterOptionViewModel FromModel(DxfConverterOption m) => new()
        {
            Id = m.Id,
            Title = m.Title ?? "",
            DxfFilePath = m.DxfFilePath ?? "",
            CenterX = m.CenterX,
            CenterY = m.CenterY,
            MapWidth = m.MapWidth,
            MapHeight = m.MapHeight,
            PixelsPerMeter = m.PixelsPerMeter
        };
    }
}
