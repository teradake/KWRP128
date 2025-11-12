using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics.CodeAnalysis;

namespace KWRP.Frontend.Models.Localizer
{
    public class LocalizeExtension : MarkupExtension
    {
        public LocalizeExtension(string key)
        {
            Key = key;
            Context = string.Empty;
        }

        public string Key { get; set; }

        public string Context { get; set; }

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Localizer))]
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var keyToUse = Key;
            if (!string.IsNullOrWhiteSpace(Context))
                keyToUse = $"{Context}/{Key}";

            var binding = new ReflectionBindingExtension
            {
                Path = $"[{keyToUse}]",
                Mode = BindingMode.OneWay,
                Source = Localizer.Instance,
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
