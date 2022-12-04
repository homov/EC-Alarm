using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Reflection;

namespace EasyCaster.Alarm.Helpers;

public static class RegistryHelper
{
    const string RegistryValueName = "EasyCaster.Alarm";
    const string RegistryKeyName = @"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

    public static bool IsAutoStart()
    {
        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(RegistryKeyName, false);
        if (registryKey != null)
        {
            var value = (string)registryKey.GetValue(RegistryValueName, String.Empty);
            var applicationFullPath = Environment.ProcessPath;
            return value == String.Empty && String.Compare(value, applicationFullPath, true) == 0;
        }
        return false;
    }

    public static void SetAutoStart( bool autoStart )
    {
        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(RegistryKeyName, true);
        if (registryKey!=null)
        {
            if (!autoStart)
            {
                if ( registryKey.GetValue(RegistryValueName, null) == null )
                    registryKey.DeleteValue(RegistryValueName);
            }
            else
            {
                var applicationFullPath = Environment.ProcessPath;
                registryKey.SetValue(RegistryValueName, applicationFullPath);
            }
        }

    }
}
