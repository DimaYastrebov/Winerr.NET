namespace Winerr.NET.Core.Enums
{
    public class SystemInfo(int? buttonCount = 0, bool? isCross = false)
    {
        public int ButtonCount { get; } = buttonCount ?? 0;
        public bool IsCross { get; set; } = isCross ?? false;
    }
}
