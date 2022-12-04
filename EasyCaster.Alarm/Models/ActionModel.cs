using CommunityToolkit.Mvvm.ComponentModel;
using EasyCaster.Alarm.Controls;
using EasyCaster.Alarm.Core.Models;
using System.Windows.Input;

namespace EasyCaster.Alarm.Models;

public partial class ActionModel: ObservableObject
{
    [ObservableProperty]
    string applicationName;

    [ObservableProperty]
    HotKey hotKey;

    [ObservableProperty]
    string commandLine;

    public static ActionModel FromEasyCasterAction(EasyCasterAction easyCasterAction)
    {
        if (easyCasterAction == null)
            return null;

        var model = new ActionModel()
        {
            ApplicationName = easyCasterAction.ApplicationName,
            CommandLine = easyCasterAction.CommandLine,
        };
        if (easyCasterAction.EasyCasterKey!=null)
        {
            model.HotKey = new HotKey
            (
                (Key)easyCasterAction.EasyCasterKey.KeyCode,
                (ModifierKeys)easyCasterAction.EasyCasterKey.Modifiers
            );
        }
        return model;
    }

    public EasyCasterAction ToEasyCasterAction()
    {
        var action = new EasyCasterAction()
        {
            ApplicationName = applicationName,
            CommandLine = commandLine
        };
        if (HotKey!=null)
        {
            action.EasyCasterKey = new EasyCasterKey()
            {
                KeyCode = (int)HotKey.Key,
                Modifiers = (int)HotKey.Modifiers
            };
        }
        return action;
    }
}
