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
using static osu_map_parser.HelperFunctions;

namespace osu_map_parser {
    static class Program {
        static void Main(string[] args) {
            #region Option variables
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
            bool fast = false;
            bool halt = false;

            bool? getSimpleTimings = null;
            bool? getSimpleJson = null;

            bool conversionChosen = false;

            var show_help = false;
            #endregion Option variables

            #region cmd args processing
            var p = new OptionSet() {
                $"Usage: ",
                $"{exeName} [OPTIONS]+",
                $"Example: ", 
                (
                    $"{exeName} -o my_folder --gamefolder=\"c:\\osu!\" " + 
                    "--song=vinxis --difficulty=\"three dimensions\" --halt --h-simple -vda"
                ),
                "",
                "Program that converts osu! beatmaps to other formats",
                "",
                { "o=", 
                    "Output folder. If none specified, outputs files in current directory.", 
                    v => outputFolder = $@"{v}\" },
                "",
                { "gamefolder=", 
                    "Your osu! folder (must contain Songs folder in it).", 
                    v => osuPath = v },
                { "song=", 
                    (
                        "Artist/song name search pattern (folder name in your Songs folder, actually). " +
                        "Case insensitive." + 
                        "It is recommended to specify this option if your Songs directory is huge, " + 
                        "since it decreases amount of maps that will be parsed."
                    ), 
                    v => searchString = v },
                { "difficulty=", 
                    "Difficulty name search pattern. Case insensitive.", 
                    v => difficultyPattern = v },
                "",
                { "fast",  
                    "Parse Songs folder only once. Converts to multiple formats faster but requires more memory.", 
                    v => fast = v != null },
                { "halt",  
                    "Do not exit program after completion.", 
                    v => halt = v != null },
                { "h-simple",  
                    "Create file with hitobjects listed in format of 'offset0,length0,offset1,length1...'", 
                    v => { getSimpleTimings = v != null; conversionChosen = true; } },
                { "h-simple-json",  
                    "Create json file with hitobjects listed in format '[{{start:0,length: 0}},{{...}}...]'.", 
                    v => { getSimpleJson = v != null; conversionChosen = true;  } },
                "",
                { "v",  
                    "Verbose: show parsing process in detail.", 
                    v => verbose = v != null },
                { "d",  
                    "Detailed: if verbose, show parsed maps info in detail.", 
                    v => detailed = v != null },
                { "a",  
                    "Additional info: if verbose, show additional info that is probably useless.", 
                    v => showAdditionalInfo = v != null },
                "",
                { "h|help",  "Show this message and exit", 
                    v => show_help = v != null },
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
            #endregion cmd args processing

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
            if (!conversionChosen) {
                getSimpleTimings = GetUserAnswerYesNo(
                    "\nCreate file with hitobjects listed in format of " + 
                    "'offset0,length0,offset1,length1...'?"
                );
                getSimpleJson = GetUserAnswerYesNo(
                    "\nCreate json file with hitobjects listed in format " +
                    "'[{{start:0,length: 0}},{{...}}...]'?"
                );
            }
            else {
                getSimpleTimings ??= false;
                getSimpleJson ??= false;
            }
            #endregion Interactivity

            #region Map parsing
            var batchParser = new BatchBeatmapParser();
            var searchOptions = new SearchPatterns() {
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
            #endregion Map parsing

            if (fast) {
                maps = maps.ToList();
            }

            #region --h-simple
            if (getSimpleTimings.Value) {
                if (!string.IsNullOrEmpty(outputFolder)) {
                    Directory.CreateDirectory(outputFolder);
                }
                var timer = new Timer().Start();
                var fileNames = SerializeMapsIntoSimpleText(
                    maps, outputFolder, batchParser.LogFileName
                );
                Console.WriteLine();
                Console.WriteLine($"Converting to simple timings (--h-simple)...");
                Console.WriteLine($"Done in {timer.Stop():f2}s");
                Console.WriteLine($"Serialized maps: {fileNames.Count()}");
                if (!fast) {
                    Console.WriteLine();
                }
            }
            #endregion --h-simple

            #region --h-simple-json
            if (getSimpleJson.Value) {
                if (!string.IsNullOrEmpty(outputFolder)) {
                    Directory.CreateDirectory(outputFolder);
                }
                var timer = new Timer().Start();
                var fileNames = SerializeMapsIntoSimpleJson(
                    maps, outputFolder, batchParser.LogFileName
                );
                Console.WriteLine();
                Console.WriteLine($"Converting to simple json timings (--h-simple-json)...");
                Console.WriteLine($"Done in {timer.Stop():f2}s");
                Console.WriteLine($"Serialized maps: {fileNames.Count()}");
            }
            #endregion --h-simple-json

            if (halt) {
                var exitAnswer = "";
                do {
                    exitAnswer = GetUserAnswer("\nType 'exit' to end the program.");
                } while (exitAnswer.Trim().ToLower() != "exit");
            }
        }

        static List<string> SerializeMapsIntoSimpleText(
            IEnumerable<Beatmap> beatmaps,
            string outputFolder,
            string logFileName
            ) {
            var createdFiles = new List<string>();
            foreach (Beatmap b in beatmaps) {
                var shortFileName =
                    $"{outputFolder}" +
                    $"{Directory.GetParent(b.SourceFile.Path).Name}" +
                    $" [{b.Metadata.Version}]";
                var fullFileName = $"{shortFileName}.txt";
                try {
                    var hitobjectToSimpleString = new Func<HitObject, string>(
                        o => $"{o.Time},{(int)Math.Round(o.GetKeypressDuration(b) ?? 0)}"
                    );
                    File.WriteAllText(
                        fullFileName,
                        string.Join(",", b.Hitobjects.List.Select(hitobjectToSimpleString))
                    );
                    createdFiles.Add(fullFileName);
                }
                catch (Exception e) {
                    WriteColored(
                        $"Could not serialize {shortFileName}",
                        ConsoleColor.Red,
                        Console.ForegroundColor
                    );
                    using (var sw = File.AppendText(logFileName)) {
                        sw.WriteLine($"Could not serialize {shortFileName}:\n{e.Message}\n");
                    }
                }
            }
            return createdFiles;
        }

        static List<string> SerializeMapsIntoSimpleJson(
            IEnumerable<Beatmap> beatmaps, 
            string outputFolder,
            string logFileName
            ) {
            var mapsToJsonize = new Dictionary<string, SimpleHitobject[]>();
            foreach (Beatmap b in beatmaps) {
                mapsToJsonize.Add(
                    $"{Directory.GetParent(b.SourceFile.Path).Name} [{b.Metadata.Version}]",
                    BeatmapToSimpleHitobjects(b)
                );
            }

            var createdFiles = new List<string>();
            foreach (KeyValuePair<string, SimpleHitobject[]> kv in mapsToJsonize) {
                var shortFileName = $"{outputFolder}{kv.Key}";
                var fullFileName = $"{shortFileName}.json";
                try {
                    File.WriteAllText(
                        fullFileName,
                        JsonSerializer.Serialize(
                            kv.Value,
                            new JsonSerializerOptions() {
                                WriteIndented = true
                            }
                        )
                    );
                    createdFiles.Add(fullFileName);
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
            return createdFiles;
        }

        static SimpleHitobject[] BeatmapToSimpleHitobjects(Beatmap beatmap) {
            List<SimpleHitobject> output = new List<SimpleHitobject>();
            foreach (HitObject o in beatmap.Hitobjects.List) {
                var offset = o.Time;
                var length = o.GetKeypressDuration(beatmap) ?? 0;
                output.Add(new SimpleHitobject(offset, (int)Math.Round(length)));
            }
            return output.ToArray();
        }

        class SimpleHitobject {
            public int start { get; set; }
            public int length { get; set; }

            public SimpleHitobject(int start, int length) {
                this.start = start;
                this.length = length;
            }
        }
    }
}
