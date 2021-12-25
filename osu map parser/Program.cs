using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using osu_map_parser.beatmap.sections.utils;
using osu_map_parser.beatmap;
using static osu_map_parser.HelperFunctions.CLI;
using static osu_map_parser.BatchBeatmapParser;
using System.Linq;
using Mono.Options;

namespace osu_map_parser {
    static class Program {
        class SimpleHitobject {
            public int start { get; set; }
            public int length { get; set; }

            public SimpleHitobject(int start, int length) {
                this.start = start;
                this.length = length;
            }
        }

        static SimpleHitobject[] BeatmapToSimpleHitobjects(Beatmap beatmap) {
            List<SimpleHitobject> output = new List<SimpleHitobject>();
            foreach (HitObject o in beatmap.Hitobjects.List) {
                var offset = o.Time;
                var length = o.GetHitObjectLength() ?? 0;
                output.Add(new SimpleHitobject(offset, (int)length));
            }
            return output.ToArray();
        }

        static void Main(string[] args) {
            string exeName = Path.GetFileNameWithoutExtension(
                System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName
            );

            string osuPath = null;
            string outputFolder = "";
            string searchString = null;
            string difficultyPattern = null;

            bool verbose = false;
            bool detailed = false;
            bool showAdditionalInfo = false;
            bool halt = false;

            bool getSimpleTimings = false;
            bool getSimpleJson = false;

            var show_help = false;

            var p = new OptionSet() {
                $"Usage: {exeName} [OPTIONS]+",
                $"Example: {exeName} -o test --gamefolder=\"c:\\osu!\" --song=vinxis --difficulty=\"three dimensions\" --halt --h-simple -vda",
                "",
                "Program that converts osu beatmaps to other formats",
                "",
                { "o=", "Output folder. If none specified, outputs files in current directory.", v => outputFolder = $@"{v}\" },
                "",
                { "gamefolder=", "Your osu! folder (must contain Songs folder in it).", v => osuPath = v },
                { "song=", "Artist/song name search pattern (folder name in your Songs folder, actually).", v => searchString = v },
                { "difficulty=", "Difficulty name search pattern.", v => difficultyPattern = v },
                "",
                { "halt",  "Do not exit program after completion.", v => halt = v != null },
                { "h-simple",  "Create file with hitobjects listed in format of 'offset0,length0,offset1,length1...'", v => getSimpleTimings = v != null },
                { "h-simple-json",  "Create json file with hitobjects listen in format '[{{start:0,length: 0}},{{...}}...]'.", v => getSimpleJson = v != null },
                "",
                { "v",  "Verbose: show parsing process in detail.", v => verbose = v != null },
                { "d",  "Detailed: if verbose, show parsed maps info in detail.", v => detailed = v != null },
                { "a",  "Additional info: if verbose, show additional info that is probably useless.", v => showAdditionalInfo = v != null },
                "",
                { "h|help",  "show this message and exit", v => show_help = v != null },
            };

            try {
                p.Parse(args);
            }
            catch (OptionException e) {
                Console.WriteLine(e.Message);
                Console.WriteLine($"Try '{exeName} --help' for more information.");
                return;
            }

            if (show_help) {
                p.WriteOptionDescriptions(Console.Out);
                return;
            }

            if (osuPath == null) {
                do {
                    osuPath = GetUserAnswer("Please enter your osu! directory");
                    if (!Directory.Exists(osuPath + @"\Songs")) {
                        Console.WriteLine("Error. Directory is not a valid osu directory.\n");
                    }
                } while (!Directory.Exists(osuPath + @"\Songs"));
            }
            if (searchString == null) {
                searchString = GetUserAnswer("\nPlease enter search pattern for maps (song name/artist)");
            }
            if (difficultyPattern == null) {
                difficultyPattern = GetUserAnswer("\nEnter search pattern for difficulty");
            }

            var batchParser = new BatchBeatmapParser();
            var searchOptions =new SearchPatterns() {
                Folder = searchString,
                Difficulty = difficultyPattern
            };
            IEnumerable<Beatmap> maps;
            if (verbose) {
                maps = batchParser.ParseFolderVerbose(
                    osuPath,
                    searchOptions,
                    new VerbosityOptions() {
                        DetailedMapInfo = detailed,
                        ShowAdditionalInfo = showAdditionalInfo
                    }
                );
            }
            else {
                maps = batchParser.ParseFolder(osuPath, searchOptions);
            }

            if (getSimpleTimings) {
                if (!string.IsNullOrEmpty(outputFolder)) {
                    Directory.CreateDirectory(outputFolder);
                }
                foreach (Beatmap b in maps) {
                    File.WriteAllText(
                        $"{outputFolder}{Directory.GetParent(b.SourceFile.Path).Name} [{b.Metadata.Version}].txt",
                        string.Join(",", b.Hitobjects.List.Select(o => $"{o.Time},{(int)(o.GetHitObjectLength() ?? 0)}"))
                    );
                }

            }

            if (getSimpleJson) {
                if (!string.IsNullOrEmpty(outputFolder)) {
                    Directory.CreateDirectory(outputFolder);
                }
                var mapsToJsonize = new Dictionary<string, SimpleHitobject[]>();
                foreach (Beatmap b in maps) {
                    mapsToJsonize.Add(
                        $"{Directory.GetParent(b.SourceFile.Path).Name} [{b.Metadata.Version}]",
                        BeatmapToSimpleHitobjects(b)
                    );
                }

                var serializedMaps = 0;
                foreach (KeyValuePair<string, SimpleHitobject[]> kv in mapsToJsonize) {
                    try {
                        File.WriteAllText(
                            $"{outputFolder}{kv.Key}.json",
                            JsonSerializer.Serialize(
                                kv.Value,
                                new JsonSerializerOptions() {
                                    WriteIndented = true
                                }
                            )
                        );
                        ++serializedMaps;
                    }
                    catch (Exception e) {
                        WriteColored(
                            $"Could not serialize {kv.Key}",
                            ConsoleColor.Red,
                            Console.ForegroundColor
                        );
                        using (var sw = File.AppendText(batchParser.LogFileName)) {
                            sw.WriteLine($"Could not serialize {kv.Key}:\n{e.Message}\n");
                        }
                    }
                }
                Console.WriteLine($"Serialized maps: {serializedMaps}.");
            }

            if (halt) {
                var exitAnswer = "";
                do {
                    exitAnswer = GetUserAnswer("Type 'exit' to end the program.");
                } while (exitAnswer.Trim().ToLower() != "exit");
            }
        }
    }
}
