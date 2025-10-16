using KWRP.Avalonia.Backend;
using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Model.Shapes.Activity;

namespace KWRP.Backend.Model.Shapes.Activity.Entity
{
    public static class ActivityEntityExtensions
    {
        public static RollerActivityEntity? ToEntity(this ActivityModel act, double laneDirectionRadian)
        {
            var ent = new RollerActivityEntity
            {
                Common = new RollerCommonActivity
                {
                    activity_id = act.GroupId * KWRPConstants.C_IINF + act.AreaID,
                    machine_type = string.Empty,
                    plan_machine_id = $"VR{act.GroupId:d2}",
                    machine_model = string.Empty,
                    plan_working_time = (int)act.WorkTime.TotalSeconds,
                    plan_start_time = DateTime.MinValue,
                    plan_end_time = DateTime.MinValue.Add(act.WorkTime),
                    trigger_activity_id = string.Empty,
                    successor_activities = string.Empty,
                    job_number = -1,
                    manned_construction = false,
                    area_name = $"1-1-{act.AreaID}",
                    work_area_range = "Not Yet Implemented!!!!!!!!!",
                    occupied_range = "Not Yet Implemented!!!!!!!!!"
                },
                Base = new RollerBaseActivity
                {
                    activity_id = act.GroupId * KWRPConstants.C_IINF + act.AreaID,
                    work_type = act.ActivityType.ToTag(),
                    construction_direction = act.Dir,
                    material_type = string.Empty,
                    elevation = -1,
                    MC_workDataName = string.Empty,
                    gradient_reference_line = string.Empty,
                    gradient_value = string.Empty,
                    lane_direction = laneDirectionRadian,
                    reference_speed = act.RefSpeed,
                    edge_speed = act.EdgeSpeed,
                    edge_position = "" + (act.Edge[0] ? "F" : "") + (act.Edge[1] ? "B" : ""),
                },
                Unique_Compaction = null,
                Unique_Move = null
            };

            // 移動アクティビティの場合、UniqueMoveの情報を付加
            if (act.ActivityType == Avalonia.Backend.Enums.RollerActivityType.Move)
            {
                ent.Unique_Move = new RollerUniqueMoveActivity
                {
                    activity_id = act.GroupId * KWRPConstants.C_IINF + act.AreaID,
                    destination_coordinate = "Not Yet Implemented!!!!!!!!!",
                    move_type = string.Empty,
                };
            }
            // 転圧アクティビティの場合、UniqueCompactionの情報を付加
            else if (act.ActivityType == Avalonia.Backend.Enums.RollerActivityType.Compaction
                || act.ActivityType == Avalonia.Backend.Enums.RollerActivityType.NonCompaction)
            {
                ent.Unique_Compaction = new RollerUniqueCompactionActivity
                {
                    activity_id = act.GroupId * KWRPConstants.C_IINF + act.AreaID,
                    repeat_count = act.RepeatNum,
                    vibration_flg = act.Comp,
                };
            }
            // 未定義の場合、nullを返す
            else
            {
                ent = null;
            }

            return ent;
        }
    }
}
