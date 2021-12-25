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
        public decimal? GetHitObjectLength(Beatmap beatmap = null) {
            switch (GetHitObjectType()) {
                case HitObjectType.Hitcircle:
                    return null;
                case HitObjectType.Slider:
                    var sliderLength = (ObjectParams as SliderParams).Length;
                    var sliderMultiplier = beatmap.Difficulty.SliderMultiplier;
                    var beatLength = beatmap.TimingPoints.GetTimingPointAtOffset(Time).BeatLength;
                    return sliderLength / (sliderMultiplier * 100) * beatLength;
                case HitObjectType.Spinner:
                    return (ObjectParams as SpinnerParams).EndTime - Time;
                default:
                    throw new System.Exception("Could not determine type of hitobject");
            }
        }
    }
}
