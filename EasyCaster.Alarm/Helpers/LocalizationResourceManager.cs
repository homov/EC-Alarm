using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace EasyCaster.Alarm.Helpers;

internal class LocalizationResourceManager : INotifyPropertyChanged
{
    private const string IndexerName = "Item";
    private const string IndexerArrayName = "Item[]";

    public event PropertyChangedEventHandler PropertyChanged;

    static readonly Lazy<LocalizationResourceManager> currentHolder = new Lazy<LocalizationResourceManager>(() => new LocalizationResourceManager());

    public static LocalizationResourceManager Current => currentHolder.Value;

    ResourceManager resourceManager;
    CultureInfo currentCulture = Thread.CurrentThread.CurrentUICulture;
    public void Init(ResourceManager resource) => resourceManager = resource;

    public void Init(ResourceManager resource, CultureInfo initialCulture)
    {
        CurrentCulture = initialCulture;
        Init(resource);
    }

    public CultureInfo CurrentCulture
    {
        get => currentCulture;
        set
        {
            currentCulture = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
        }
    }

    public string GetValue(string text)
    {
        if (resourceManager == null)
            return text;
        var value = resourceManager.GetString(text, CurrentCulture);
        if (value == null)
            return $"{nameof(text)}: {text} not found";
        else
            value = value.Replace("\\n", "\n");
        return value;
    }

    public string this[string text] => GetValue(text);
}