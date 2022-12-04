using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Windows.Input;
using System.Collections.Generic;

namespace EasyCaster.Alarm.Controls;

public class HotKey
{
    private const uint MAPVK_VK_TO_VSC = 0x00;

    [DllImport("user32.dll")]
    static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

    [DllImport("user32.dll", EntryPoint = "GetKeyNameTextW", CharSet = CharSet.Unicode)]
    private static extern int GetKeyNameText(int lParam, [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder str, int size);


    private static string GetLocalizedKeyStringUnsafe(int keyCode)
    {
        var sb = new StringBuilder(256);
        long scanCode = MapVirtualKeyEx((uint)keyCode, MAPVK_VK_TO_VSC,IntPtr.Zero);

        scanCode = (scanCode << 16);
        if (keyCode == 45 ||
            keyCode == 46 ||
            keyCode == 144 ||
            (33 <= keyCode && keyCode <= 40))
        {
            scanCode |= 0x1000000;
        }
        GetKeyNameText((int)scanCode, sb, 256);
        return sb.ToString();
    }

    private static Dictionary<Key,string> MakeKeyMap()
    {
        Dictionary<Key, string> keyMap = new();
        foreach( Key key in Enum.GetValues(typeof(Key)))
        {
            var virtKey = KeyInterop.VirtualKeyFromKey(key);
            var keyName = GetLocalizedKeyStringUnsafe(virtKey);
            if (String.IsNullOrEmpty(keyName))
                keyName = key.ToString();
            keyMap[key] = keyName;
        }
        return keyMap;
    }

    private static Lazy<Dictionary<Key, string>> keyMapHolder = new (MakeKeyMap);
    static Dictionary<Key, string> KeyMap => keyMapHolder.Value;

    public Key Key { get; }

    public ModifierKeys Modifiers { get; }

    public HotKey(Key key, ModifierKeys modifiers)
    {
        Key = key;
        Modifiers = modifiers;
    }

    public override string ToString()
    {
        var str = new StringBuilder();

        if (Modifiers.HasFlag(ModifierKeys.Control))
            str.Append("Ctrl + ");
        if (Modifiers.HasFlag(ModifierKeys.Shift))
            str.Append("Shift + ");
        if (Modifiers.HasFlag(ModifierKeys.Alt))
            str.Append("Alt + ");
        if (Modifiers.HasFlag(ModifierKeys.Windows))
            str.Append("Win + ");
        str.Append($"{KeyMap[Key]}");

        return str.ToString();
    }
}
