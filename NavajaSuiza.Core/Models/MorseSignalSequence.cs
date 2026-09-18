namespace NavajaSuiza.Core.Models;

public sealed record MorseFrame(bool IsOn, int DurationMs);

public static class MorseSignalSequence
{
    public const int UNIT_MS = 100;
    public const int DOT_MS = UNIT_MS;
    public const int DASH_MS = 3 * UNIT_MS;
    public const int INTRACHAR_GAP_MS = UNIT_MS;
    public const int CHAR_GAP_MS = 3 * UNIT_MS;
    public const int WORD_GAP_MS = 7 * UNIT_MS;

    private static readonly IReadOnlyDictionary<char, string> Morse = new Dictionary<char, string>
    {
        ['A'] = ".-",
        ['B'] = "-...",
        ['C'] = "-.-.",
        ['D'] = "-..",
        ['E'] = ".",
        ['F'] = "..-.",
        ['G'] = "--.",
        ['H'] = "....",
        ['I'] = "..",
        ['J'] = ".---",
        ['K'] = "-.-",
        ['L'] = ".-..",
        ['M'] = "--",
        ['N'] = "-.",
        ['O'] = "---",
        ['P'] = ".--.",
        ['Q'] = "--.-",
        ['R'] = ".-.",
        ['S'] = "...",
        ['T'] = "-",
        ['U'] = "..-",
        ['V'] = "...-",
        ['W'] = ".--",
        ['X'] = "-..-",
        ['Y'] = "-.--",
        ['Z'] = "--.."
    };

    public static IReadOnlyList<MorseFrame> Sos { get; } = Build("SOS");

    public static IReadOnlyList<MorseFrame> Help { get; } = Build("HELP");

    public static IReadOnlyList<MorseFrame> Build(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            throw new ArgumentException("La palabra no puede estar vacía.", nameof(word));

        var frames = new List<MorseFrame>();

        for (var i = 0; i < word.Length; i++)
        {
            var c = char.ToUpperInvariant(word[i]);

            if (c == ' ')
            {
                frames.Add(new MorseFrame(false, WORD_GAP_MS));
                continue;
            }

            if (!Morse.TryGetValue(c, out var pattern))
                throw new ArgumentException($"Carácter no soportado en Morse: '{word[i]}'.", nameof(word));

            for (var j = 0; j < pattern.Length; j++)
            {
                frames.Add(new MorseFrame(true, pattern[j] == '.' ? DOT_MS : DASH_MS));

                if (j < pattern.Length - 1)
                    frames.Add(new MorseFrame(false, INTRACHAR_GAP_MS));
            }

            if (i < word.Length - 1)
                frames.Add(new MorseFrame(false, CHAR_GAP_MS));
        }

        return frames;
    }
}