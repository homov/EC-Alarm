namespace EasyCaster;

public static class StringExtensions
{
    public static bool IsEmpty(this string self)
    {
        return String.IsNullOrWhiteSpace(self) || self.Length == 0;
    }
}
