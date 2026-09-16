namespace NavajaSuiza.Core;

public static class AppConstants
{
    public static class Metronome
    {
        public const int DEFAULT_BPM = 120;
        public const string DEFAULT_TIME_SIGNATURE = "4/4";
    }

    public static class Framing
    {
        public const double MAX_CANVAS_WIDTH = 500;
        public const int DEFAULT_BLUR_INTENSITY = 13;
        public const string DEFAULT_ASPECT_RATIO = "1:1";
        public const string DEFAULT_ASPECT_MODE = "AspectFill";
        public const string DEFAULT_CANVAS_BACKGROUND = "Blur";
        public const string DEFAULT_CANVAS_BACKGROUND_COLOR = "Black";
        public const double DEFAULT_CANVAS_BACKGROUND_OPACITY = 1.0;

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
        public const int LOADING_DELAY_MS = 500;
    }
}
