// namespace: Monopoly.Domain.Tiles (để dùng chung với PropertyTile)
namespace Monopoly.Domain.Tiles
{
    // Model màu/nhóm để map từ JSON string
    public enum ColorGroup
    {
        Brown, LightBlue, Pink, Orange, Red, Yellow, Green, DarkBlue
    }

    public static class ColorGroupExtensions
    {
        // Map ColorGroup -> PropertyColor (đang dùng trong PropertyTile)
        public static PropertyColor ToPropertyColor(this ColorGroup g) => g switch
        {
            ColorGroup.Brown     => PropertyColor.Brown,
            ColorGroup.LightBlue => PropertyColor.LightBlue,
            ColorGroup.Pink      => PropertyColor.Pink,
            ColorGroup.Orange    => PropertyColor.Orange,
            ColorGroup.Red       => PropertyColor.Red,
            ColorGroup.Yellow    => PropertyColor.Yellow,
            ColorGroup.Green     => PropertyColor.Green,
            ColorGroup.DarkBlue  => PropertyColor.DarkBlue,
            _ => throw new ArgumentOutOfRangeException(nameof(g))
        };

        public static bool TryParse(string? s, out ColorGroup g)
        {
            return Enum.TryParse(s, ignoreCase: true, out g);
        }
    }
}
