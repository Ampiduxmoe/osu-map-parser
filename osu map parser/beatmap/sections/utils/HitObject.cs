namespace osu_map_parser.beatmap.sections.utils
{
    class HitObject
    {
        public int X;
        public int Y;
        public int Time;
        public int Type;
        public int HitSound;
        public IObjectParams ObjectParams;
        public HitSample HitSample;

        public HitObjectType GetHitObjectType()
        {
            foreach (var type in Constants.AllHitObjectTypes)
            {
                if ((Type & (int)type) != 0)
                    return type;
            }
            return HitObjectType.None;
        }
    }
}
