namespace KWRP.Backend.Model.Shapes.Activity
{
    public class RollerActivityEntity
    {
        public RollerCommonActivity Common { get; set; } = new();
        public RollerBaseActivity Base { get; set; } = new();
        public RollerUniqueCompactionActivity? Unique_Compaction { get; set; }
        public RollerUniqueMoveActivity? Unique_Move { get; set; }
    }

    public class RollerCommonActivity
    {
        public int activity_id { get; set; }
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
        public string work_area_range { get; set; } = string.Empty;
        public string occupied_range { get; set; } = string.Empty;
    }

    public class RollerBaseActivity
    {
        public int activity_id { get; set; }
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
        public int activity_id { get; set; }   
        public int repeat_count { get; set; }
        public bool vibration_flg { get; set; }
    }

    public class RollerUniqueMoveActivity
    {
        public int activity_id { get; set; }
        public string destination_coordinate { get; set; } = string.Empty;
        public string move_type { get; set; } = string.Empty;
    }
}
