using KWRP.Avalonia.Backend.Enums;
using KWRP.Frontend.Models.Localizer;
using System;
using System.Collections.Generic;

namespace KWRP.Frontend.Services.Extensions
{
    public static class RollerActivityTypeExtensions
    {

        private static readonly Dictionary<RollerActivityType, string> DisplayNames = new()
    {
        { RollerActivityType.Move, "移動" },
        { RollerActivityType.NonCompaction, "無起振転圧" },
        { RollerActivityType.Compaction, "転圧" }
    };

        private static readonly Dictionary<RollerActivityType, string> DisplayCategories = new()
    {
        { RollerActivityType.Move, "移動" },
        { RollerActivityType.NonCompaction, "転圧" },
        { RollerActivityType.Compaction, "転圧" }
    };

        public static string ToDisplayName(this RollerActivityType type)
        {
            return type switch
            {
                RollerActivityType.Move => Localizer.Instance["Domain.Back.Trans"],
                RollerActivityType.NonCompaction => Localizer.Instance["Domain.Back.NoComp"],
                RollerActivityType.Compaction => Localizer.Instance["Domain.Back.Comp"],
                _ => throw new NotImplementedException(),
            };
        }

        public static string ToDisplayCategory(this RollerActivityType type)
        {
            return type switch
            {
                RollerActivityType.Move => Localizer.Instance["Domain.Back.Trans"],
                RollerActivityType.NonCompaction => Localizer.Instance["Domain.Back.Comp"],
                RollerActivityType.Compaction => Localizer.Instance["Domain.Back.Comp"],
                _ => throw new NotImplementedException(),
            };
        }
}
}
