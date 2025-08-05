using KWRP.Avalonia.Backend.Enums;
using System.Text;

namespace KWRP.Avalonia.Backend.Model.Shapes.WorkArea
{
    public record WorkAreaAttribute
    {
        public bool IsSelected { get; init; } = false;
        public bool IsEnabled { get; init; } = true;
        public bool IsDirectionSwap { get; init; } = false;
        public GoalAreaLocation GoalAreaLocation { get; init; } = GoalAreaLocation.Prev;
        public WorkAreaLaneTrimType LaneTrimTypeFront { get; init; } = WorkAreaLaneTrimType.None;
        public WorkAreaLaneTrimType LaneTrimTypeRear { get; init; } = WorkAreaLaneTrimType.Cut;

        public static WorkAreaAttribute Default
            => new WorkAreaAttribute
            {
                IsSelected = false,
                IsEnabled = true,
                IsDirectionSwap = false,
                GoalAreaLocation = GoalAreaLocation.Prev,
                LaneTrimTypeFront = WorkAreaLaneTrimType.None,
                LaneTrimTypeRear = WorkAreaLaneTrimType.None,
            };

        public WorkAreaAttribute Copy()
        {
            return new WorkAreaAttribute
            {
                IsSelected = this.IsSelected,
                IsEnabled = this.IsEnabled,
                IsDirectionSwap = this.IsDirectionSwap,
                GoalAreaLocation = this.GoalAreaLocation,
                LaneTrimTypeFront = this.LaneTrimTypeFront,
                LaneTrimTypeRear = this.LaneTrimTypeRear,
            };
        }

        public override string ToString()
        {
            return new StringBuilder()
                .AppendLine($"{nameof(IsEnabled)} = {IsEnabled}")
                .AppendLine($"{nameof(IsDirectionSwap)} = {IsDirectionSwap}")
                .AppendLine($"{nameof(GoalAreaLocation)} = {GoalAreaLocation}")
                .AppendLine($"{nameof(LaneTrimTypeFront)} = {LaneTrimTypeFront}")
                .AppendLine($"{nameof(LaneTrimTypeRear)} = {LaneTrimTypeRear}")
                .ToString();
        }
    }


    public static class WorkAreaAttributeExtensions
    {
        public static WorkAreaAttribute Clone(this WorkAreaAttribute attribute)
        {
            return attribute with { };
        }

        public static WorkAreaAttribute SetIsSelected(this WorkAreaAttribute attribute, bool isSelected)
        {
            return attribute with { IsSelected = isSelected };
        }

        public static WorkAreaAttribute SetGoalArea(this WorkAreaAttribute attribute, GoalAreaLocation goalAreaLocation)
        {
            return attribute with { GoalAreaLocation = goalAreaLocation };
        }

        public static WorkAreaAttribute SetIsEnabled(this WorkAreaAttribute attribute, bool isEnabled)
        {
            return attribute with { IsEnabled = isEnabled };
        }

        public static WorkAreaAttribute SetIsDirectionSwap(this WorkAreaAttribute attribute, bool isDirectionSwap)
        {
            return attribute with { IsDirectionSwap = isDirectionSwap };
        }

        public static WorkAreaAttribute SetLaneTrimTypeFront(this WorkAreaAttribute attribute, WorkAreaLaneTrimType laneTrimTypeFront)
        {
            return attribute with { LaneTrimTypeFront = laneTrimTypeFront };
        }

        public static WorkAreaAttribute SetLaneTrimTypeRear(this WorkAreaAttribute attribute, WorkAreaLaneTrimType laneTrimTypeRear)
        {
            return attribute with { LaneTrimTypeRear = laneTrimTypeRear };
        }

    }
}
