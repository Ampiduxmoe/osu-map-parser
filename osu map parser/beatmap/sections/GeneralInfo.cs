using System.Collections.Generic;

namespace osu_map_parser.beatmap.sections {
    public class GeneralInfo {
        public string AudioFilename;
        public int AudioLeadIn = 0;
        public string AudioHash;
        public int PreviewTime = -1;
        public int Countdown = 1;
        public string SampleSet = utils.SampleSet.Normal.ToString();
        public decimal StackLeniency = 0.7m;
        public int Mode = 0;
        public byte LetterboxInBreaks = 0;
        public byte StoryFireInFront = 1;
        public byte UseSkinSprites = 0;
        public byte AlwaysShowPlayfield = 0;
        public string OverlayPosition = utils.OverlayPosition.NoChange.ToString();
        public string SkinReference;
        public byte EpilepsyWarning = 0;
        public int CountdownOffset = 0;
        public byte SpecialStyle = 0;
        public byte WidescreenStoryboard = 0;
        public byte SamplesMatchPlaybackRate = 0;

        public List<string> DeprecatedOptions { get; } = new List<string>()
        {
            "AudioHash",
            "StoryFireInFront",
            "AlwaysShowPlayfield"
        };
    }
}
