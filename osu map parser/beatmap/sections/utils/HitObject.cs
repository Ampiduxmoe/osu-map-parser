namespace osu_map_parser.beatmap.sections.utils {
    class HitObject {
        public int X;
        public int Y;
        public int Time;
        public int Type;
        public int HitSound;
        public IObjectParams ObjectParams;
        public HitSample HitSample;

        public HitObjectType GetHitObjectType() {
            foreach (var type in Constants.AllHitObjectTypes) {
                if ((Type & (int)type) != 0) {
                    return type;
                }
            }
            return HitObjectType.None;
        }

        // Provide beatmap when trying to get slider length
        public decimal? GetKeypressDuration(Beatmap beatmap = null) {
            switch (GetHitObjectType()) {
                case HitObjectType.Hitcircle:
                    return null;
                case HitObjectType.Slider:
                    if (beatmap == null) {
                        throw new System.Exception("No beatmap provided to determine length of a slider");
                    }
                    var currentRedSection = beatmap.TimingPoints.GetTimingPointAtOffset(Time, 1);
                    currentRedSection ??= beatmap.TimingPoints.GetNextTimingPoint(Time, 1);
                    var currentGreenSection = beatmap.TimingPoints.GetTimingPointAtOffset(Time, 0);
                    var greenSectionMultiplier = currentGreenSection != null ? -100 / currentGreenSection.BeatLength : 1;
                    var sliderParams = ObjectParams as SliderParams;

                    var sliderLength = sliderParams.Length;
                    var sliderMultiplier = beatmap.Difficulty.SliderMultiplier * greenSectionMultiplier;
                    var beatLength = currentRedSection.BeatLength;
                    var slides = sliderParams.Slides;

                    return sliderLength / (sliderMultiplier * 100) * beatLength * slides;
                case HitObjectType.Spinner:
                    return (ObjectParams as SpinnerParams).EndTime - Time;
                default:
                    throw new System.Exception("Could not determine type of hitobject");
            }
        }
    }
}
