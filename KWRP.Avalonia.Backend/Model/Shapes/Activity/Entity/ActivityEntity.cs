using System.Xml.Serialization;

namespace KWRP.Backend.Model.Shapes.Activity.Entity
{

    public class RollerActivityEntity
    {
        public int GroupId { get; set; }
        public int AreaId { get; set; }
        public int ActivityId { get; set; }
        public RollerCommonActivity Common { get; set; } = new();
        public RollerBaseActivity Base { get; set; } = new();
        public RollerUniqueCompactionActivity? Unique_Compaction { get; set; }
        public RollerUniqueMoveActivity? Unique_Move { get; set; }
    }

    public class RollerCommonActivity
    {
        public string machine_type {  get; set; } = string.Empty;
        public string plan_machine_id { get; set; } = string.Empty;
        public string machine_model { get; set; } = string.Empty;
        public string work_type { get; set; } = string.Empty;
        public int plan_working_time { get; set; }
        public DateTime plan_start_time { get; set; }
        public DateTime plan_end_time { get; set; }
        public string trigger_activity_id { get; set; } = string.Empty;
        public string successor_activities { get; set; } = string.Empty;
        public int job_number { get; set; }
        public bool manned_construction { get; set; }
        public string area_name { get; set; } = string.Empty;
        public List<Vector2d> work_area_range { get; set; } = [];
        public List<Vector2d> occupied_range { get; set; } = [];
    }

    public class RollerBaseActivity
    {
        public string work_type { get; set; } = string.Empty;
        public double construction_direction { get; set; }
        public string material_type { get; set; } = string.Empty;
        public double elevation { get; set; }
        public string MC_workDataName { get; set; } = string.Empty;
        public string gradient_reference_line { get; set; } = string.Empty;
        public string gradient_value { get; set; } = string.Empty;
        public double lane_direction { get; set; }
        public double reference_speed { get; set; }
        public double edge_speed { get; set; }
        public string edge_position { get; set; } = string.Empty;
    }

    public class RollerUniqueCompactionActivity
    {
        public int repeat_count { get; set; }
        public bool vibration_flg { get; set; }
    }

    public class RollerUniqueMoveActivity
    {
        public List<Vector2d> destination_coordinate { get; set; } = [];
        public string move_type { get; set; } = string.Empty;
    }

    public class Vector2d
    {
        private double _x;
        private double _y;

        public double X
        {
            get { return _x; }
            set
            {
                _x = Math.Round(value, 4, MidpointRounding.AwayFromZero);
            }
        }

        public double Y
        {
            get { return _y; }
            set
            {
                _y = Math.Round(value, 4, MidpointRounding.AwayFromZero);
            }
        }
    }
}
