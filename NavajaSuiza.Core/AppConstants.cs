namespace NavajaSuiza.Core;

public static class AppConstants
{
    public static class Metronome
    {
        public const int DefaultBPM = 120;
        public const string DefaultTimeSignature = "4/4";
    }

    public static class Framing
    {
        public const double MaxCanvasWidth = 500;
        public const int DefaultBlurIntensity = 13;
        public const string DefaultAspectRatio = "1:1";
        public const string DefaultAspectMode = "AspectFill";
        public const string DefaultCanvasBackground = "Blur";
        public const string DefaultCanvasBackgroundColor = "Black";
        public const double DefaultCanvasBackgroundOpacity = 1.0;

        public static readonly Dictionary<string, double> AspectRatios = new()
        {
            { "1:1", 1.0 },
            { "4:5", 1.25 },
            { "9:16", 1.777 },
            { "16:9", 0.5625 }
        };
    }

    public static class Instruments
    {
        public const int LoadingDelayMs = 500;
    }
}
