using EasyCaster.Alarm.Helpers;
using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace EasyCaster.Alarm.Extensions;

public class TranslateExtension: MarkupExtension
{
    public string Key { get; set; }

    public string Context { get; set; }

    public TranslateExtension(string key)
    {
        this.Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var keyToUse = Key;
        if (!string.IsNullOrWhiteSpace(Context))
            keyToUse = $"{Context}/{Key}";

        var binding = new Binding($"[{keyToUse}]")
        {
            Mode = BindingMode.OneWay,
            Source = LocalizationResourceManager.Current
        };

        return binding.ProvideValue(serviceProvider);
    }

}
