using osu_map_parser.beatmap.sections;

namespace osu_map_parser.beatmap
{
    class Beatmap
    {
        public readonly SourceFile SourceFile;
        public readonly int? FileFormatVersion;
        public readonly GeneralInfo General;
        public readonly EditorInfo Editor;
        public readonly MetadataInfo Metadata;
        public readonly DifficultyInfo Difficulty;
        public readonly EventsInfo Events;
        public readonly TimingPointsInfo TimingPoints;
        public readonly ColoursInfo Colours;
        public readonly HitObjectsInfo Hitobjects;

        public Beatmap
            (
            SourceFile sourceFile, 
            int? fileFormatVersion,
            GeneralInfo generalInfo, 
            EditorInfo editorInfo, 
            MetadataInfo metadataInfo, 
            DifficultyInfo difficultyInfo, 
            EventsInfo eventsInfo, 
            TimingPointsInfo timingPointsInfo, 
            ColoursInfo coloursInfo, 
            HitObjectsInfo hitobjectsInfo
            )
        {
            SourceFile = sourceFile;
            FileFormatVersion = fileFormatVersion;
            General = generalInfo;
            Editor = editorInfo;
            Metadata = metadataInfo;
            Difficulty = difficultyInfo;
            Events = eventsInfo;
            TimingPoints = timingPointsInfo;
            Colours = coloursInfo;
            Hitobjects = hitobjectsInfo;
        }
    }
}
