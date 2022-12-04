using EasyCaster.Alarm.Core.Models;
using InputSimulatorStandard;
using InputSimulatorStandard.Native;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace EasyCaster.Alarm.Core.Helpers;

public static class ActionInvoker
{
    static readonly TimeSpan waitWindowForActivation = TimeSpan.FromMilliseconds(500);

    static readonly IntPtr HWND_TOP = new IntPtr(0);
    public const uint SW_SHOW = 5;

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
    
    [DllImport("user32.dll")]
    static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

    [DllImport("kernel32.dll")]
    public static extern uint GetCurrentThreadId();

    static object locker = new object();

    private static void AttachedThreadInputAction(Action action)
    {
        var foreThread = GetWindowThreadProcessId(GetForegroundWindow(),IntPtr.Zero);
        var appThread = GetCurrentThreadId();
        bool threadsAttached = false;

        try
        {
            threadsAttached = foreThread == appThread || AttachThreadInput(foreThread, appThread, true);

            if (threadsAttached) action();
        }
        finally
        {
            if (threadsAttached)
                AttachThreadInput(foreThread, appThread, false);
        }
    }

    private static (IEnumerable<VirtualKeyCode>, VirtualKeyCode) TranslateEasyCasterKey(EasyCasterKey easyCasterKey)
    {
        var wpfModifiers = (ModifierKeys)easyCasterKey.Modifiers;
        var virtualKeyCode = (VirtualKeyCode)KeyInterop.VirtualKeyFromKey((Key)easyCasterKey.KeyCode);

        List<VirtualKeyCode> modifiers = new();
        if (wpfModifiers.HasFlag(ModifierKeys.Alt))
            modifiers.Add(VirtualKeyCode.MENU);
        if (wpfModifiers.HasFlag(ModifierKeys.Control))
            modifiers.Add(VirtualKeyCode.CONTROL);
        if (wpfModifiers.HasFlag(ModifierKeys.Shift))
            modifiers.Add(VirtualKeyCode.SHIFT);
        return (modifiers, virtualKeyCode);
    }

    public static void Invoke(EasyCasterAction easyCasterAction)
    {
        if (!easyCasterAction.ApplicationName.IsEmpty() && easyCasterAction.EasyCasterKey != null)
        {
            //Prevent activate window in parallel
            //Otherwise it is not possible to send a keystroke to the desired window
            lock (locker)
            {
                var process = Process.GetProcessesByName(easyCasterAction.ApplicationName).FirstOrDefault();
                if (process != null)
                {
                    var windowHandle = process.MainWindowHandle;
                    if (windowHandle != IntPtr.Zero)
                    {
                        var (modifiers, keyCode) = TranslateEasyCasterKey(easyCasterAction.EasyCasterKey);
                        var simulator = new InputSimulator();
                        AttachedThreadInputAction(() =>
                        {
                            BringWindowToTop(windowHandle);
                            ShowWindow(windowHandle, SW_SHOW);
                        });
                        if (GetForegroundWindow() == windowHandle)
                        {
                            simulator.Keyboard.ModifiedKeyStroke(modifiers, keyCode);
                        }
                        else
                        {
                            throw new Exception($"Unable to activate main window for process {easyCasterAction.ApplicationName}");
                        }

                    }
                }
                else
                {
                    throw new Exception($"Unable to find process {easyCasterAction.ApplicationName}");
                }
            }
        }
        if (!easyCasterAction.CommandLine.IsEmpty())
        {
            Process.Start(easyCasterAction.CommandLine);
        }
    }
}
