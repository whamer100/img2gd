using System.Globalization;

namespace Img2GD;

public static class Util
{
    public static (int x, int y) ParsePair(string pair)
    {
        var parts = pair.Split(',');
        int.TryParse(parts[0], NumberStyles.Integer, null, out var n0);
        int.TryParse(parts[1], NumberStyles.Integer, null, out var n1);
        return (n0, n1);
    }
}