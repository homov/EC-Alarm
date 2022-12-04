using CommunityToolkit.Mvvm.ComponentModel;

namespace EasyCaster.Alarm.Models;

public partial class IndexedItem<T>: ObservableObject
{

    [ObservableProperty]
    int index;

    [ObservableProperty]
    T value;

    public IndexedItem(int index, T value)
    {
        Index = index;
        Value = value;
    }

}
