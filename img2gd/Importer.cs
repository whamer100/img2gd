using GeometryDashAPI.Data;
using GeometryDashAPI.Data.Enums;
using GeometryDashAPI.Levels;
using GeometryDashAPI.Levels.GameObjects;
using GeometryDashAPI.Levels.GameObjects.Default;
using GeometryDashAPI.Levels.GameObjects.Triggers;
using GeometryDashAPI.Levels.Structures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using ProgramOptions = Img2GD.Cli.ProgramOptions;
using Color = GeometryDashAPI.Levels.Color;
using Hsv = GeometryDashAPI.Levels.Hsv;

namespace Img2GD;

public class Importer
{
    private readonly ProgramOptions _opt;
    private readonly Image<Argb32> image;

    public Importer(ProgramOptions options)
    {
        _opt = options;

        if (!File.Exists(_opt.ImagePath))
        {
            Logma.Fatal($"File \"{_opt.ImagePath}\" does not exist!");
            Environment.Exit(1);
        }

        if (_opt.Backup)
            BackupData();

        var local = LocalLevels.LoadFile();
        if (!local.LevelExists(_opt.LevelName))
        {
            Logma.Fatal($"Level \"{_opt.LevelName}\" does not exist!");
            Environment.Exit(1);
        }

        var levelEntry = local.GetLevel(_opt.LevelName, _opt.Revision);
        Logma.Info($"Loading level \"{levelEntry.Name} by {levelEntry.AuthorName}\" [Song id: {levelEntry.MusicId}]");
        var level = levelEntry.LoadLevel();
        var duration = TimeSpan.Zero;
        var blockCount = level.CountBlock;
        if (blockCount > 0)
            duration = level.Duration;
        Logma.Debug($"bv:{levelEntry.BinaryVersion},r:{levelEntry.Revision},d:{duration},b:{blockCount},c:{level.CountColor}");
        level.AddColor(new Color(999)
        {
            Rgb = RgbColor.FromHex("#FF0000"),
            Opacity = 1
        });

        image = Image.Load<Argb32>(_opt.ImagePath);
        EvaluateImage();

        var drawn = 0;
        var bounds = image.Bounds;
        (int x, int y) origin = _opt.Origin;
        for (var x = 0; x < bounds.Right; x++)
        for (var y = 0; y < bounds.Bottom; y++)
        {
            var col = image[x, y];
            var conv = new Rgb24(col.R, col.G, col.B);
            if (col.A >= _opt.AlphaThreshold) // TODO: implement alpha officially using new colors
            {
                DrawPixel(ref level, x + origin.x, (bounds.Bottom - y - 1) + origin.y, Rgb2Hsv(conv));
                ++drawn;
            }
        }
        Logma.Info($"Drawn pixels: {drawn}");

        Logma.Info("Saving data...");
        local.GetLevel(_opt.LevelName, revision: _opt.Revision).SaveLevel(level);
        local.Save();
    }

    ~Importer()
    {
        image.Dispose();
    }

    private static void EvaluateImage()
    {
        // TODO: calculate best fit for alpha colors
    }

    private static void BackupData()
    {
        Logma.Info("Backing up game data...");
        var backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "backups");
        Logma.Debug($"Writing to {backupDirectory}");
        Directory.CreateDirectory(backupDirectory);
        var backupTimestamp = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'_'mm'_'ss");

        var gmPath = GameData.ResolveFileName(GameDataType.GameManager);
        var llPath = GameData.ResolveFileName(GameDataType.LocalLevels);

        File.Copy(gmPath, Path.Combine(backupDirectory, $"CCGameManager_{backupTimestamp}.dat"));
        File.Copy(llPath, Path.Combine(backupDirectory, $"CCLocalLevels_{backupTimestamp}.dat"));
    }

    private void DrawPixel(ref Level lvl, int x, int y, Hsv col)
    {
        var block = new BaseBlock(id: Constants.PixelBlockId)
        {
            PositionX = x * 5 + 2.5f,
            PositionY = y * 5 + 2.5f,
            ColorBase = 999,  // todo: allow transparency
            Scale = 5f,
            Hsv = col,
            EditorL = _opt.EditorLayer
        };
        lvl.AddBlock(block);
    }

    private static Hsv Rgb2Hsv(Rgb24 col)
    {
        var hsv = ColorSpaceConverter.ToHsv(col);
        var gdHsv = new Hsv
        {
            Hue = hsv.H,
            Saturation = hsv.S - 1,
            Brightness = hsv.V - 1,
            DeltaBrightness = true,
            DeltaSaturation = true
        };
        return gdHsv;
    }
}