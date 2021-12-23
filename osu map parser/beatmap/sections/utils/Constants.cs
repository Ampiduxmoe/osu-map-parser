using System;
using System.Collections.Generic;
namespace osu_map_parser.beatmap.sections.utils
{
    public enum GameMode { osu, taiko, @catch, mania }
    public enum SampleSet { Default, Normal, Soft, Drum }
    public enum OverlayPosition { NoChange, Below, Above }
    public enum HitObjectType { None = 0, Hitcircle = 1, Slider = 2, Spinner = 8 }
    public enum Effect { None = 0, Kiai = 1 }

    static class Constants
    {
        public static List<HitObjectType> AllHitObjectTypes = new List<HitObjectType>
        {
            HitObjectType.Hitcircle,
            HitObjectType.Slider,
            HitObjectType.Spinner
        };
    }
}
