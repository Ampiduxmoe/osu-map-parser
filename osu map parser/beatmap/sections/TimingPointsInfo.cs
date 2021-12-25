using osu_map_parser.beatmap.sections.utils;
using System.Collections.Generic;
using System.Linq;

namespace osu_map_parser.beatmap.sections {
    class TimingPointsInfo {
        public List<TimingPoint> List;
        public TimingPoint GetTimingPointAtOffset(int offset, byte? uninherited = null) {
            var query = List.Where(v => v.Time <= offset);
            if (uninherited.HasValue) {
                query = query.Where(v => v.Uninherited == uninherited.Value);
            }
            return query.OrderBy(v => v.Time).Last();
        }
    }
    
}
