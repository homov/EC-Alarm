using EasyCaster.Alarm.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyCaster.Alarm.Services;

//NOTE: locked to uk only
public class LanguageService
{
    public static Dictionary<string, string> Languages = new Dictionary<string, string>()
    {
        {"en","English" },
        {"uk","Українська" },
    };

    public event Action<string> LanguageChanged;

    private string currentLanguage;


    public LanguageService()
    {
        ConfigurationService.Instance.ConfigurationChanged += ConfigurationChanged;
    }

    private Task ConfigurationChanged()
    {
        //CurrentLanguage = ConfigurationService.Instance.Configuration.Language;
        CurrentLanguage = "uk";
        return Task.CompletedTask;
    }

    public string CurrentLanguage
    {
        get { return currentLanguage; }
        set
        {
            var language = value;
            if (string.IsNullOrEmpty(value) || !Languages.Keys.Contains(language))
                language = "en";
            if (currentLanguage != language)
            {
                SetLanguage(language);
                LanguageChanged?.Invoke(language);
            }
        }
    }

    private void SetLanguage(string language)
    {
        var ci = GetCulture(language);
        CultureInfo.DefaultThreadCurrentCulture = ci;
        CultureInfo.DefaultThreadCurrentUICulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        LocalizationResourceManager.Current.CurrentCulture = ci;
        currentLanguage = language;
    }

    public static CultureInfo GetCulture(string language)
    {
        if (String.IsNullOrEmpty(language))
            return null;
        return new CultureInfo(language);
    }


    public void Start()
    {
        //CurrentLanguage = ConfigurationService.Instance.Configuration.Language;
        CurrentLanguage = "uk";
    }

}
