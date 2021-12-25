using osu_map_parser.beatmap.sections.utils;
using System.Collections.Generic;
using System.Linq;

namespace osu_map_parser.beatmap.sections {
    class TimingPointsInfo {
        public List<TimingPoint> List;
        public TimingPoint GetTimingPointAtOffset(int offset) {
            return List.Where(v => v.Time <= offset).OrderBy(v => v.Time).Last();
        }
    }
    
}
