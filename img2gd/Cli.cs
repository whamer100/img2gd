using CommandLine;
using CommandLine.Text;

namespace Img2GD;

public class Cli
{
    private class InternalOptions
    {
        [Option("input", Required = true,
            HelpText = "Target image to render.")]
        public string? ImagePath { get; set; }
        [Option("output", Required = true,
            HelpText = "Destination level to render into.")]
        public string? LevelName { get; set; }
        [Option("revision", Default = 0,
            HelpText = "Level revision.")]
        public int Revision { get; set; }
        [Option("layer", Default = Constants.DefaultEditorLayer,
            HelpText = "Editor Layer to render image to.")]
        public short EditorLayer { get; set; }
        [Option("threshold", Default = Constants.DefaultAlphaThreshold,
            HelpText = "Threshold of what should be considered fully transparent and not rendered.")]
        public byte AlphaThreshold { get; set; }
        [Option("origin", Default = "0,0",
            HelpText = "Origin of where to place the image (bottom left corner, grows upward).")]
        public string? Origin { get; set; }
        [Option("verbose", Default = false,
            HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
        [Option("backup", Default = false,
            HelpText = "Backs up game data before touching anything. (Set flag to disable)")]
        public bool Backup { get; set; }
    }

    public struct ProgramOptions
    {
        public string ImagePath { get; set; }
        public string LevelName { get; set; }
        public int Revision { get; set; }
        public short EditorLayer { get; set; }
        public byte AlphaThreshold { get; set; }
        public ValueTuple<int, int> Origin { get; set; }
        public bool Verbose { get; set; }
        public bool Backup { get; set; }
    }

    public ProgramOptions Options;

    public Cli(string[] args)
    {
        var parser = new Parser(with => with.HelpWriter = null);
        var parserResult = parser.ParseArguments<InternalOptions>(args);

        parserResult
            .WithParsed(o =>
            {
                Options.ImagePath = o.ImagePath!;
                Options.LevelName = o.LevelName!;
                Options.Revision = o.Revision;
                Options.EditorLayer = o.EditorLayer;
                Options.AlphaThreshold = o.AlphaThreshold;
                Options.Origin = Util.ParsePair(o.Origin!);
                Options.Verbose = o.Verbose;
                if (o.Verbose)
                    Logma.IncreaseVerbosity();
                Options.Backup = !o.Backup;
            })
            .WithNotParsed(errs => DisplayHelp(parserResult, errs));
    }

    private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
    {
        HelpText helpText;
        if (errs.IsVersion())  //check if error is version request
            helpText = HelpText.AutoBuild(result);
        else
        {
            helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = $"{AppDomain.CurrentDomain.FriendlyName} v{Constants.Version}";
                h.Copyright = "Copyright (c) 2024 whamer100";
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
        }
        Console.WriteLine(helpText);
        Environment.Exit(1);
    }
}