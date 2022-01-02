using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using osu_map_parser.beatmap;
using osu_map_parser.beatmap.sections.utils;
using osu_map_parser.files;
using static osu_map_parser.HelperFunctions.CLI;

namespace osu_map_parser {
    public class BatchBeatmapParser {
        public DateTime StartDate { get; private set; }
        public DateTime CompletionDate { get; private set; }
        public string LogFileName { get; private set; } = null;
        public int? BeatmapsTotal { get; private set; } = null;
        public int? StdBeatmapsFound { get; private set; } = null;
        public int? StdBeatmapsParsed { get; private set; } = null;

        public BatchBeatmapParser() {
            var currentTime = DateTime.Now.ToString("s").Replace("T", " ").Replace(":", "-");
            LogFileName = $@"{currentTime}.log";
        }

        public struct SearchPatterns {
            public string Folder;
            public string Difficulty;
        }

        public struct VerbosityOptions {
            public bool DetailedMapInfo;
            public bool ShowAdditionalInfo;
        }

        public IEnumerable<Beatmap> ParseFolder(string osuPath, SearchPatterns searchPatterns) {

            var folderPattern = searchPatterns.Folder;
            var difficultyPattern = searchPatterns.Difficulty;

            var songDirs = Directory.EnumerateDirectories(
                $@"{osuPath}\Songs",
                $"*{folderPattern}*",
                new EnumerationOptions() {
                    MatchCasing = MatchCasing.CaseInsensitive,
                }
            );

            StartDate = DateTime.Now;
            BeatmapsTotal = 0;
            StdBeatmapsFound = 0;
            StdBeatmapsParsed = 0;

            File.Delete("old_fields.txt");
            Console.WriteLine("Parsing your osu! Songs directory...");
            foreach (var songDir in songDirs) {
                var files = Directory.EnumerateFiles(songDir, "*.osu");
                foreach (var file in files) {
                    var sourceFile = SourceFile.Read(file);
                    var parser = new Parser(sourceFile);
                    Beatmap beatmap = null;
                    try {
                        ++BeatmapsTotal;
                        beatmap = parser.ParseBeatmap();
                        ++StdBeatmapsParsed;
                    }
                    catch (Exception e) {
                        using (var sw = File.AppendText(LogFileName)) {
                            sw.WriteLine($"Could not parse {file}:\n{e.Message}\n");
                        }
                    }
                    finally {
                        ++StdBeatmapsFound;
                    }
                    if (beatmap != null) {
                        var wrongDiff = (
                            !string.IsNullOrEmpty(difficultyPattern) &&
                            !beatmap.Metadata.Version.Contains(
                                difficultyPattern,
                                StringComparison.CurrentCultureIgnoreCase
                            )
                        );
                        if (wrongDiff) {
                            continue;
                        }
                        yield return beatmap;
                    }
                }
            }
            CompletionDate = DateTime.Now;
            var secondsElapsed = (CompletionDate - StartDate).TotalSeconds;
            Console.WriteLine($"Done in {secondsElapsed:f2}s");
            Console.WriteLine($"Parsed maps: {StdBeatmapsParsed}");
        }

        public IEnumerable<Beatmap> ParseFolderVerbose
            (
            string osuPath, 
            SearchPatterns searchPatterns, 
            VerbosityOptions verbosityOptions
            ) {
            var folderPattern = searchPatterns.Folder;
            var difficultyPattern = searchPatterns.Difficulty;

            var songDirs = Directory.EnumerateDirectories(
                $@"{osuPath}\Songs",
                $"*{folderPattern}*",
                new EnumerationOptions() {
                    MatchCasing = MatchCasing.CaseInsensitive,
                }
            );

            StartDate = DateTime.Now;
            BeatmapsTotal = 0;
            StdBeatmapsFound = 0;
            StdBeatmapsParsed = 0;

            var versionsWithoutAR = new List<int?>();
            File.Delete("old_fields.txt");
            Console.WriteLine("Parsing your osu songs directory...\n");
            foreach (var songDir in songDirs) {
                var files = Directory.EnumerateFiles(songDir, "*.osu");
                foreach (var file in files) {
                    var sourceFile = SourceFile.Read(file);
                    var parser = new Parser(sourceFile);
                    Beatmap beatmap = null;
                    try {
                        ++BeatmapsTotal;
                        beatmap = parser.ParseBeatmap();
                    }
                    catch (Exception e) {
                        WriteColored(
                            $"Could not parse {file}",
                            ConsoleColor.DarkRed,
                            ConsoleColor.White
                        );
                        using (var sw = File.AppendText(LogFileName)) {
                            sw.WriteLine($"Could not parse {file}:\n{e.Message}\n");
                        }
                        ++StdBeatmapsFound;
                    }
                    if (beatmap != null) {
                        ++StdBeatmapsParsed;
                        ++StdBeatmapsFound;
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
                        var version = beatmap.FileFormatVersion;
                        var cs = beatmap.Difficulty.CircleSize;
                        var od = beatmap.Difficulty.OverallDifficulty;
                        var ar = beatmap.Difficulty.ApproachRate ?? od;
                        var hp = beatmap.Difficulty.HPDrainRate;
                        var sliderTickrate = beatmap.Difficulty.SliderTickRate;

                        var writeDetailed = verbosityOptions.DetailedMapInfo;
                        if (writeDetailed) {
                            Console.WriteLine();
                        }
                        WriteColored(
                            $"{(writeDetailed?"  ":"")}{beatmap.Metadata.Artist} - {beatmap.Metadata.Title} [{beatmap.Metadata.Version}]",
                            ConsoleColor.Green,
                            ConsoleColor.Black
                        );
                        if (writeDetailed) {
                            Console.WriteLine($"  | osu file format v{version}");
                            Console.WriteLine($"  | BeatmapID: {beatmap.Metadata.BeatmapID}, BeatmapSetID: {beatmap.Metadata.BeatmapSetID}");
                            Console.WriteLine($"  | CS: {cs}, AR: {ar}, OD: {od}, HP: {hp}");
                            Console.WriteLine($"  | Slider Tickrate: {sliderTickrate}");
                            Console.WriteLine($"  | Hitcircle count: {hitObjectsCount[HitObjectType.Hitcircle]}");
                            Console.WriteLine($"  | Slider count: {hitObjectsCount[HitObjectType.Slider]}");
                            Console.WriteLine($"  | Spinner count: {hitObjectsCount[HitObjectType.Spinner]}");
                            Console.WriteLine();
                        }
                        yield return beatmap;
                    }
                }
            }
            CompletionDate = DateTime.Now;
            var secondsElapsed = (CompletionDate - StartDate).TotalSeconds;

            Console.WriteLine($"\nDone in {secondsElapsed:f2}s");
            Console.WriteLine($"Parsed maps: {StdBeatmapsParsed}");
            var notParsed = BeatmapsTotal - StdBeatmapsParsed;
            if (notParsed > 0) {
                Console.WriteLine($"Could not parse {notParsed} maps");
                var notStd = BeatmapsTotal - StdBeatmapsFound;
                object part = notStd == notParsed ? "All" : notStd;
                Console.WriteLine($"{part} of them were not an std maps and are currently not supported");
                if (part.GetType() != typeof(string)) {
                    Console.WriteLine($"Please see '{LogFileName}' for details (std only)");
                }
            }

            var writeAdditionalFacts = verbosityOptions.ShowAdditionalInfo;
            if (writeAdditionalFacts) {
                var distinctVersionsWithoutAR = versionsWithoutAR.Distinct();
                if (distinctVersionsWithoutAR.Count() > 0) {
                    Console.WriteLine();
                    Console.WriteLine($"File format versions where AR did not exist on its own:");
                    foreach (var version in distinctVersionsWithoutAR.OrderBy(v => v)) {
                        Console.WriteLine($"  osu file format v{version}");
                    }
                }
            }

            if (writeAdditionalFacts && File.Exists("old_fields.txt")) {
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
        }
    }
}
