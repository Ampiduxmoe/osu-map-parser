using System.Collections.Generic;
namespace osu_map_parser.beatmap.sections {
    class MetadataInfo {
        public string Title;
        public string TitleUnicode;
        public string Artist;
        public string ArtistUnicode;
        public string Creator;
        public string Version;
        public string Source;
        public List<string> Tags;
        public int BeatmapID;
        public int BeatmapSetID;
    }
}
