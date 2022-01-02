using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using osu_map_parser.beatmap;
using static osu_map_parser.HelperFunctions.CLI;
using static osu_map_parser.BatchBeatmapParser;

namespace osu_map_parser {
    static class UsageExample {
        static void Main(string[] args) {
            string osuPath = null;
            string searchString = null;
            string difficultyPattern = null;

            #region Interactivity
            if (osuPath == null) {
                do {
                    osuPath = GetUserAnswer("Please enter your osu! directory");
                    if (!Directory.Exists(osuPath + @"\Songs")) {
                        Console.WriteLine("Error. Directory is not a valid osu! directory.\n");
                    }
                } while (!Directory.Exists(osuPath + @"\Songs"));
            }
            if (searchString == null) {
                searchString = GetUserAnswer(
                    "\nPlease enter search pattern for maps (song name/artist)"
                );
            }
            if (difficultyPattern == null) {
                difficultyPattern = GetUserAnswer(
                    "\nEnter search pattern for difficulty"
                );
            }
            #endregion Interactivity

            #region Map parsing
            var batchParser = new BatchBeatmapParser();
            var searchOptions = new SearchPatterns() {
                Folder = searchString,
                Difficulty = difficultyPattern
            };
            IEnumerable<Beatmap> maps;
            var verbose = true;
            if (verbose) {
                maps = batchParser.ParseFolderVerbose(
                    osuPath,
                    searchOptions,
                    new VerbosityOptions() {
                        DetailedMapInfo = true,
                        ShowAdditionalInfo = true
                    }
                );
            }
            else {
                maps = batchParser.ParseFolder(osuPath, searchOptions);
            }
            #endregion Map parsing

            var lazy = false;
            if (!lazy) {
                // Evaluate collection one time by storing result
                maps = maps.ToList();
            }

            Console.WriteLine();
            foreach (var map in maps) {
                var avgX = map.Hitobjects.List.Select(o => o.X).Sum() / map.Hitobjects.List.Count();
                var avgY = map.Hitobjects.List.Select(o => o.Y).Sum() / map.Hitobjects.List.Count();
                Console.WriteLine($"Average object position on map {map.Metadata.BeatmapID} is ({avgX}, {avgY})");
            }
        }
    }
}
