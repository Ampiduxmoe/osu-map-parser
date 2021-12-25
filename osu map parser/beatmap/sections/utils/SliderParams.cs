using System.Collections.Generic;

namespace osu_map_parser.beatmap.sections.utils {
    class SliderParams : IObjectParams {
        public char CurveType;
        public List<Point> CurvePoints;
        public int Slides;
        public decimal Length;
        public List<int> EdgeSounds;
        public List<EdgeSet> EdgeSets;
    }
}
