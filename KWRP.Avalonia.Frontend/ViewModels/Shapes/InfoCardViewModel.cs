using KWRP.Avalonia.Backend.Model.Shapes.Activity;
using System;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.ViewModels.Shapes
{
    public class InfoCardViewModel : ViewModelBase
    {
        private readonly ActivityModel _model;

        public InfoCardViewModel(ActivityModel model)
        {
            _model = model;
        }

        public string Group => _model.GroupId.ToString();
        public string LaneID => _model.AreaID.ToString();

        public string AreaID => $"{Group}-{LaneID}";
        public string AreaLength => $"L{_model.Length:0.0}";
        public string AreaWidth => $"W{_model.Width:0.0}";


        public double OriginX => _model.InfoPos?.X ?? 0;
        public double OriginY => _model.InfoPos?.Y ?? 0;
        public Vec2 Origin => _model.InfoPos;

        public bool ToUpper => _model.HeadToUp;
        public double Angle => _model.Dir * 180 / Math.PI + (ToUpper ? -90 : 90);
        public double Scale
        {
            get
            {
                if (_model.Width < 2.25)
                    return 0.07;
                if (_model.Width < 4.5)
                    return 0.09;
                return 0.12;
            }
        }
    }
}
