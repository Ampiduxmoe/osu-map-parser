using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using osu_map_parser.beatmap.sections.utils;
using osu_map_parser.beatmap;
using static osu_map_parser.HelperFunctions.CLI;

namespace osu_map_parser {
    static class Program {
        

        class smocerHitobject {
            public int start { get; set; }
            public int length { get; set; }

            public smocerHitobject(int start, int length) {
                this.start = start;
                this.length = length;
            }
        }

        static smocerHitobject[] BeatmapToSmocerMap(Beatmap beatmap) {
            List<smocerHitobject> output = new List<smocerHitobject>();
            foreach (HitObject o in beatmap.Hitobjects.List) {
                var offset = o.Time;
                var length = 0;
                if (o.Type != (int)HitObjectType.Hitcircle) {
                    switch(o.Type) {
                        case (int)HitObjectType.Slider:
                            var sliderParams = o.ObjectParams as SliderParams;
                            length = (int)sliderParams.Length;
                            break;
                        case (int)HitObjectType.Spinner:
                            var spinnerParams = o.ObjectParams as SpinnerParams;
                            length = (int)spinnerParams.EndTime - offset;
                            break;
                    }
                }
                output.Add(new smocerHitobject(offset, length));
            }
            return output.ToArray();
        }

        static void Main(string[] args) {
            var osuPath = @"";
            do {
                osuPath = GetUserAnswer("Please enter your osu! directory");
                if (!Directory.Exists(osuPath + @"\Songs")) {
                    Console.WriteLine("Error. Directory is not a valid osu directory.\n");
                }
            } while (!Directory.Exists(osuPath + @"\Songs"));

            var searchString = GetUserAnswer("\nPlease enter search pattern for maps (song name/artist)");
            var difficultyPattern = GetUserAnswer("\nEnter search pattern for difficulty");

            ParseFolder(osuPath, new SearchPatterns() { Folder = searchString, Difficulty = difficultyPattern });
        }

        struct SearchPatterns {
            public string Folder;
            public string Difficulty;
        }

        static void ParseFolder(string osuPath, SearchPatterns searchPatterns) {
            var smocerMaps = new Dictionary<string, smocerHitobject[]>();

            var searchString = searchPatterns.Folder;
            var difficultyPattern = searchPatterns.Difficulty;

            var songDirs = Directory.EnumerateDirectories(
                $@"{osuPath}\Songs",
                $"*{searchString}*",
                new EnumerationOptions() {
                    MatchCasing = MatchCasing.CaseInsensitive,
                }
            );

            var startDate = DateTime.Now;
            var currentTime = startDate.ToString("s").Replace("T", " ").Replace(":", "-");
            var logFileName = $@"{currentTime}.log";
            var beatmapsTotal = 0;
            var stdBeatmapsTotal = 0;
            var stdBeatmapsParsed = 0;
            var versionsWithoutAR = new List<int?>();
            File.Delete("old_fields.txt");
            foreach (var songDir in songDirs) {
                var files = Directory.EnumerateFiles(songDir, "*.osu");
                foreach (var file in files) {
                    var sourceFile = SourceFile.Read(file);
                    var parser = new Parser(sourceFile);
                    Beatmap beatmap = null;
                    try {
                        ++beatmapsTotal;
                        //Console.WriteLine($@"Trying to parse {Path.GetFileNameWithoutExtension(file)}...");
                        beatmap = parser.ParseBeatmap();
                    }
                    catch (Exception e) {
                        WriteColored(
                            $"Could not parse {file}",
                            ConsoleColor.Red,
                            Console.ForegroundColor
                        );
                        using (var sw = File.AppendText(logFileName)) {
                            sw.WriteLine($"Could not parse {file}:\n{e.Message}\n");
                        }
                        ++stdBeatmapsTotal;
                    }
                    if (beatmap != null) {
                        ++stdBeatmapsParsed;
                        ++stdBeatmapsTotal;
                        var wrongDiff = (
                            !string.IsNullOrEmpty(difficultyPattern) &&
                            !beatmap.Metadata.Version.Contains(
                                difficultyPattern,
                                StringComparison.CurrentCultureIgnoreCase
                            )
                        );
                        if (wrongDiff) {
                            WriteColored(
                                $"Skipping {Path.GetFileNameWithoutExtension(file)} (wrong difficulty)...",
                                ConsoleColor.DarkGray,
                                Console.ForegroundColor
                            );
                            continue;
                        }
                        smocerMaps.Add(
                            $"{Directory.GetParent(file).Name} [{beatmap.Metadata.Version}]", 
                            BeatmapToSmocerMap(beatmap)
                        );
                        if (beatmap.Difficulty.ApproachRate == null) {
                            versionsWithoutAR.Add(beatmap.FileFormatVersion);
                        }
                        Dictionary<HitObjectType, int> hitObjectsCount = new Dictionary<HitObjectType, int> {
                            { HitObjectType.Hitcircle, 0 },
                            { HitObjectType.Slider, 0 },
                            { HitObjectType.Spinner, 0 }
                        };
                        foreach (var hitobj in beatmap.Hitobjects.List) {
                            ++hitObjectsCount[hitobj.GetHitObjectType()];
                        }
                        var toConsole = true;
                        if (toConsole) {
                            var version = beatmap.FileFormatVersion;
                            var cs = beatmap.Difficulty.CircleSize;
                            var od = beatmap.Difficulty.OverallDifficulty;
                            var ar = beatmap.Difficulty.ApproachRate ?? od;
                            var hp = beatmap.Difficulty.HPDrainRate;
                            var sliderTickrate = beatmap.Difficulty.SliderTickRate;
                            Console.WriteLine();
                            WriteColored(
                                $"  {beatmap.Metadata.Artist} - {beatmap.Metadata.Title} [{beatmap.Metadata.Version}]",
                                ConsoleColor.Green,
                                ConsoleColor.Black
                            );
                            Console.WriteLine($"  | osu file format v{version}");
                            Console.WriteLine($"  | BeatmapID: {beatmap.Metadata.BeatmapID}, BeatmapSetID: {beatmap.Metadata.BeatmapSetID}");
                            Console.WriteLine($"  | CS: {cs}, AR: {ar}, OD: {od}, HP: {hp}");
                            Console.WriteLine($"  | Slider Tickrate: {sliderTickrate}");
                            Console.WriteLine($"  | Hitcircle count: {hitObjectsCount[HitObjectType.Hitcircle]}");
                            Console.WriteLine($"  | Slider count: {hitObjectsCount[HitObjectType.Slider]}");
                            Console.WriteLine($"  | Spinner count: {hitObjectsCount[HitObjectType.Spinner]}\n");
                        }
                    }
                }
            }
            var completionDate = DateTime.Now;
            var secondsElapsed = (completionDate - startDate).TotalSeconds;

            Console.WriteLine($"\nDone in {secondsElapsed:f2}s");
            Console.WriteLine($"Parsed maps: {stdBeatmapsParsed}");
            var notParsed = beatmapsTotal - stdBeatmapsParsed;
            if (notParsed > 0) {
                Console.WriteLine($"Could not parse {notParsed} maps");
                var notStd = beatmapsTotal - stdBeatmapsTotal;
                object part = notStd == notParsed ? "All" : notStd;
                Console.WriteLine($"{part} of them were not an std maps and are currently not supported");
                if (part.GetType() != typeof(string)) {
                    Console.WriteLine($"Please see '{logFileName}' for details (std only)");
                }
            }

            var showAdditionalInfo = false;
            if (showAdditionalInfo) {
                var distinctVersionsWithoutAR = versionsWithoutAR.Distinct();
                if (distinctVersionsWithoutAR.Count() > 0) {
                    Console.WriteLine();
                    Console.WriteLine($"File format versions where AR did not exist on its own:");
                    foreach (var version in distinctVersionsWithoutAR.OrderBy(v => v)) {
                        Console.WriteLine($"  osu file format v{version}");
                    }
                }
            }

            var checkOldFields = false;
            if (checkOldFields && File.Exists("old_fields.txt")) {
                var oldFields = File.ReadAllLines("old_fields.txt").Distinct();
                var oldFieldsDic = new Dictionary<string, List<string>>();
                foreach (var line in oldFields) {
                    var split = line.Split(": ");
                    if (!oldFieldsDic.ContainsKey(split[0])) {
                        oldFieldsDic.Add(split[0], new List<string>());
                    }
                    oldFieldsDic[split[0]].Add(split[1]);
                }
                Console.WriteLine();
                Console.WriteLine("Old fields that are not present in osu file format v14: ");
                foreach (var pair in oldFieldsDic) {
                    Console.WriteLine($"  [{pair.Key}]");
                    foreach (var field in pair.Value) {
                        Console.WriteLine($"    {field}");
                    }
                    Console.WriteLine();
                }
            }
            File.Delete("old_fields.txt");

            var serializedMaps = 0;
            foreach (KeyValuePair<string, smocerHitobject[]> kv in smocerMaps) {
                try {
                    File.WriteAllText(
                        $"{kv.Key}.json",
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
                    using (var sw = File.AppendText(logFileName)) {
                        sw.WriteLine($"Could not serialize {kv.Key}:\n{e.Message}\n");
                    }
                }
            }
            Console.WriteLine($"Serialized maps: {serializedMaps}.");
        }
    }
}
