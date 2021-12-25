using osu_map_parser.beatmap;
using osu_map_parser.beatmap.sections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;
using osu_map_parser.beatmap.sections.utils;
using System.IO;

namespace osu_map_parser {
    // Currently does not fully support old file formats and game modes other than standard
    // Fully supports osu file format v14 except events
    class Parser {
        public readonly SourceFile SourceFile;

        private int currerntLineNo;
        private int currentInlineCursorPos;
        private string CurrerntLine => SourceFile.Lines[currerntLineNo].Trim();
        public Parser(SourceFile sourceFile) {
            SourceFile = sourceFile;
            currerntLineNo = 0;
            currentInlineCursorPos = 0;
        }

        public Beatmap ParseBeatmap() {
            int? fileFormatVersion = null;
            GeneralInfo generalInfo = null;
            EditorInfo editorInfo = null;
            MetadataInfo metadataInfo = null;
            DifficultyInfo difficultyInfo = null;
            EventsInfo eventsInfo = null;
            TimingPointsInfo timingPointsInfo = null;
            ColoursInfo coloursInfo = null;
            HitObjectsInfo hitObjectsInfo = null;
            try {
                fileFormatVersion = ParseFileFormatVersion();
                generalInfo = ParseGeneralInfo();
                if ((GameMode)generalInfo.Mode != GameMode.osu) {
                    return null;
                }
                var section = GetCurrentLineSection();
                while (section != null) {
                    switch (section) {
                        case "Editor":
                            editorInfo = ParseEditorInfo();
                            break;

                        case "Metadata":
                            metadataInfo = ParseMetadataInfo();
                            break;

                        case "Difficulty":
                            difficultyInfo = ParseDifficultyInfo();
                            break;

                        case "Events":
                            eventsInfo = ParseEventsInfo();
                            break;

                        case "TimingPoints":
                            timingPointsInfo = ParseTimingPointsInfo();
                            break;

                        case "Colours":
                            coloursInfo = ParseColoursInfo();
                            break;

                        case "HitObjects":
                            hitObjectsInfo = ParseHitObjectsInfo();
                            break;
                    }
                    section = GetCurrentLineSection();
                }
            }
            catch (Exception e) {
                throw new Exception(SourceFile.CounstructErrorMessage(e.Message, currerntLineNo, currentInlineCursorPos));
            }
            AssertSectionInfoNotNull(generalInfo);
            AssertSectionInfoNotNull(metadataInfo);
            AssertSectionInfoNotNull(difficultyInfo);
            AssertSectionInfoNotNull(timingPointsInfo);
            AssertSectionInfoNotNull(hitObjectsInfo);
            var beatmap = new Beatmap
                (
                SourceFile,
                fileFormatVersion,
                generalInfo,
                editorInfo,
                metadataInfo,
                difficultyInfo,
                eventsInfo,
                timingPointsInfo,
                coloursInfo,
                hitObjectsInfo
                );
            return beatmap;

        }

        private int ParseFileFormatVersion() {
            ResetCursor();
            ExpectFileFormatVersion();
            var regex = new Regex(@"[1-9][0-9]*");
            MoveCursor("osu file format v".Length);
            var version = int.Parse(regex.Match(CurrerntLine).Value);
            ReadNextLine();
            return version;
        }

        private GeneralInfo ParseGeneralInfo() {
            ResetCursor();
            ExpectSection("General");
            ReadNextLine();
            var generalInfo = new GeneralInfo();
            var splitString = ": ";
            while (!CurrentLineIsSection) {
                ResetCursor();
                var option = CurrerntLine.Split(splitString.Trim()).Select(s => s.Trim()).ToArray();
                MoveCursor(option[0].Length + splitString.Length);
                SetPrimitiveFieldValue(generalInfo, option[0], option[1]);
                ReadNextLine();
            }
            return generalInfo;
        }

        private EditorInfo ParseEditorInfo() {
            // This section is not present in some versions
            try {
                ExpectSection("Editor");
            }
            catch {
                return null;
            }
            ReadNextLine();
            var editorInfo = new EditorInfo();
            var splitString = ": ";
            while (!CurrentLineIsSection) {
                ResetCursor();
                var option = CurrerntLine.Split(splitString.Trim()).Select(s => s.Trim()).ToArray();
                MoveCursor(option[0].Length + splitString.Length);
                if (option[0] == "Bookmarks") {
                    editorInfo.Bookmarks = option[1].Split(",").Select(s => int.Parse(s)).ToList();
                }
                else {
                    SetPrimitiveFieldValue(editorInfo, option[0], option[1]);
                }
                ReadNextLine();
            }
            return editorInfo;
        }

        private MetadataInfo ParseMetadataInfo() {
            ResetCursor();
            ExpectSection("Metadata");
            ReadNextLine();
            var metadataInfo = new MetadataInfo();
            var splitString = ":";
            while (!CurrentLineIsSection) {
                ResetCursor();
                var option = CurrerntLine.Split(splitString, 2);
                MoveCursor(option[0].Length + splitString.Length);
                if (option[0] == "Tags") {
                    metadataInfo.Tags = option[1].Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else {
                    SetPrimitiveFieldValue(metadataInfo, option[0], option[1]);
                }
                ReadNextLine();
            }
            return metadataInfo;
        }

        private DifficultyInfo ParseDifficultyInfo() {
            ResetCursor();
            ExpectSection("Difficulty");
            ReadNextLine();
            var difficultyInfo = new DifficultyInfo();
            var splitString = ":";
            while (!CurrentLineIsSection) {
                ResetCursor();
                var option = CurrerntLine.Split(splitString);
                MoveCursor(option[0].Length + splitString.Length);
                SetPrimitiveFieldValue(difficultyInfo, option[0], option[1]);
                ReadNextLine();
            }
            return difficultyInfo;
        }

        private EventsInfo ParseEventsInfo() {
            ResetCursor();
            ExpectSection("Events");
            ReadNextLine();
            var eventsInfo = new EventsInfo();
            eventsInfo.List = new List<Event>();
            while (!CurrentLineIsSection) {
                ReadNextLine();
                // It is a bit complicated so I'll just skip this section
                continue;
                if (CurrerntLine.Substring(0, 2) == "//") {
                    continue;
                }
                var parameters = CurrerntLine.Split(",");
                ReadNextLine();
            }
            return eventsInfo;
        }

        private TimingPointsInfo ParseTimingPointsInfo() {
            ResetCursor();
            ExpectSection("TimingPoints");
            ReadNextLine();
            var timingPointsInfo = new TimingPointsInfo();
            timingPointsInfo.List = new List<TimingPoint>();
            var splitString = ",";
            while (!CurrentLineIsSection) {
                ResetCursor();
                var parameters = CurrerntLine.Split(splitString);
                TimingPoint t = new TimingPoint();

                t.Time = decimal.Parse(parameters[0], CultureInfo.InvariantCulture);
                MoveCursor(parameters[0].Length + splitString.Length);

                t.BeatLength = decimal.Parse(parameters[1], CultureInfo.InvariantCulture);
                MoveCursor(parameters[1].Length + splitString.Length);

                // osu file format v3 timing points had only time and beat length
                if (parameters.Length > 2) {
                    t.Meter = int.Parse(parameters[2]);
                    MoveCursor(parameters[2].Length + splitString.Length);

                    t.SampleSet = int.Parse(parameters[3]);
                    MoveCursor(parameters[3].Length + splitString.Length);

                    t.SampleIndex = int.Parse(parameters[4]);
                    MoveCursor(parameters[4].Length + splitString.Length);

                    t.Volume = int.Parse(parameters[5]);
                    MoveCursor(parameters[5].Length + splitString.Length);

                    t.Uninherited = parameters.Length > 6 ? byte.Parse(parameters[6]) : (byte)1;
                    if (parameters.Length > 6) {
                        MoveCursor(parameters[6].Length + splitString.Length);
                    }

                    t.Effects = parameters.Length > 7 ? int.Parse(parameters[7]) : (int)Effect.None;
                }
                else {
                    t.Meter = 4;
                    t.SampleSet = (int)SampleSet.Normal;
                    t.SampleIndex = 0;
                    t.Volume = 100;
                    t.Uninherited = 1;
                    t.Effects = (int)Effect.None;
                }
                timingPointsInfo.List.Add(t);
                ReadNextLine();
            }
            return timingPointsInfo;
        }

        private ColoursInfo ParseColoursInfo() {
            // This section is not present in some versions
            try {
                ExpectSection("Colours");
            }
            catch {
                return null;
            }
            ReadNextLine();
            var coloursInfo = new ColoursInfo();
            var splitString = " : ";
            while (!CurrentLineIsSection) {
                ResetCursor();
                var option = CurrerntLine.Split(splitString.Trim()).Select(s => s.Trim()).ToArray();
                MoveCursor(option[0].Length + splitString.Length);
                var additionalSplitString = ",";
                var rgb = option[1].Split(additionalSplitString);
                var colour = new Colour();

                colour.R = int.Parse(rgb[0]);
                MoveCursor(rgb[0].Length + additionalSplitString.Length);

                colour.G = int.Parse(rgb[1]);
                MoveCursor(rgb[1].Length + additionalSplitString.Length);

                colour.B = int.Parse(rgb[2]);

                ResetCursor();
                if (CurrentLineIsCombo) {
                    coloursInfo.Combos ??= new Dictionary<int, Colour>();
                    MoveCursor("Combo".Length);
                    var regex = new Regex(@"[1-9]");
                    var comboNo = int.Parse(regex.Match(option[0]).Value);
                    coloursInfo.Combos.Add(comboNo, colour);
                }
                else {
                    coloursInfo.GetType().GetField(option[0]).SetValue(coloursInfo, colour);
                }
                ReadNextLine();

            }
            return coloursInfo;
        }

        private HitObjectsInfo ParseHitObjectsInfo() {
            ResetCursor();
            ExpectSection("HitObjects");
            ReadNextLine();
            var hitobjectsInfo = new HitObjectsInfo();
            hitobjectsInfo.List = new List<HitObject>();
            var splitString0 = ",";
            while (currerntLineNo < SourceFile.Lines.Length) {
                ResetCursor();
                var parameters = CurrerntLine.Split(splitString0);
                HitObject h = new HitObject();
                h.X = int.Parse(parameters[0]);
                MoveCursor(parameters[0].Length + splitString0.Length);

                h.Y = int.Parse(parameters[1]);
                MoveCursor(parameters[1].Length + splitString0.Length);

                h.Time = int.Parse(parameters[2]);
                MoveCursor(parameters[2].Length + splitString0.Length);

                h.Type = int.Parse(parameters[3]);
                MoveCursor(parameters[3].Length + splitString0.Length);

                h.HitSound = int.Parse(parameters[4]);
                MoveCursor(parameters[4].Length + splitString0.Length);

                var hitSampleParamsIndex = -1;
                switch (h.GetHitObjectType()) {
                    case HitObjectType.None:
                        ResetCursor();
                        MoveCursor(string.Join(splitString0, parameters[0], parameters[1], parameters[2]).Length + 1);
                        throw new Exception("Could not determine type of hitobject");

                    case HitObjectType.Hitcircle:
                        h.ObjectParams = null;
                        hitSampleParamsIndex = 5;
                        break;

                    case HitObjectType.Slider:
                        var sliderParameters = new string[] {
                            parameters[5],
                            parameters[6],
                            parameters[7],
                            parameters.Length > 8 ? parameters[8] : null,
                            parameters.Length > 9 ? parameters[9] : null
                        };
                        var splitString1 = "|";
                        var curve = sliderParameters[0].Split(splitString1, 2);
                        var sliderParams = new SliderParams();

                        sliderParams.CurveType = Convert.ToChar(curve[0]);
                        MoveCursor(curve[0].Length + splitString1.Length);

                        var splitString2 = "|";
                        var curvePoints = curve[1].Split(splitString2);
                        MoveCursor(curve[1].Length + splitString2.Length);

                        sliderParams.CurvePoints = new List<Point>();
                        foreach (var pair in curvePoints) {
                            var splitString3 = ":";
                            var values = pair.Split(splitString3);

                            Point point = new Point();
                            point.X = int.Parse(values[0]);
                            MoveCursor(values[0].Length + splitString3.Length);

                            point.Y = int.Parse(values[1]);
                            MoveCursor(values[1].Length + splitString2.Length);

                            sliderParams.CurvePoints.Add(point);
                        }

                        sliderParams.Slides = int.Parse(sliderParameters[1]);
                        MoveCursor(sliderParameters[1].Length + splitString1.Length);

                        sliderParams.Length = decimal.Parse(sliderParameters[2], CultureInfo.InvariantCulture);
                        MoveCursor(sliderParameters[2].Length + splitString1.Length);

                        hitSampleParamsIndex = 8;
                        if (parameters.Length > 8) {
                            var splitString4 = "|";
                            var edgeSounds = sliderParameters[3].Split(splitString4);
                            sliderParams.EdgeSounds = new List<int>();
                            foreach (var sound in edgeSounds) {
                                sliderParams.EdgeSounds.Add(int.Parse(sound));
                                MoveCursor(sound.Length + splitString4.Length);
                            }
                            MoveCursor(splitString2.Length - splitString4.Length);
                            hitSampleParamsIndex = 9;
                        }

                        if (parameters.Length > 9) {
                            var splitString5 = "|";
                            var edgeSets = sliderParameters[4].Split(splitString5);
                            sliderParams.EdgeSets = new List<EdgeSet>();
                            foreach (var pair in edgeSets) {
                                var splitString6 = ":";
                                var normalAndAddition = pair.Split(splitString6);

                                var edgeSet = new EdgeSet();
                                edgeSet.NormalSet = int.Parse(normalAndAddition[0]);
                                MoveCursor(normalAndAddition[0].Length + splitString6.Length);

                                edgeSet.AdditionSet = int.Parse(normalAndAddition[1]);
                                MoveCursor(normalAndAddition[1].Length + splitString5.Length);

                                sliderParams.EdgeSets.Add(edgeSet);
                            }
                            hitSampleParamsIndex = 10;
                        }
                        h.ObjectParams = sliderParams;
                        break;

                    case HitObjectType.Spinner:
                        var spinnerParams = new SpinnerParams();
                        spinnerParams.EndTime = int.Parse(parameters[5]);
                        MoveCursor(parameters[5].Length + splitString0.Length);
                        hitSampleParamsIndex = 6;
                        h.ObjectParams = spinnerParams;
                        break;
                }
                string[] hitSampleParameters;
                var splitString7 = ":";
                if (hitSampleParamsIndex == parameters.Length || string.IsNullOrWhiteSpace(parameters[hitSampleParamsIndex])) {
                    hitSampleParameters = "0:0:0:0:".Split(splitString7);
                }
                else {
                    hitSampleParameters = parameters[hitSampleParamsIndex].Split(splitString7);
                }
                var hs = new HitSample();
                for (int i = 0; i < hitSampleParameters.Length; ++i) {
                    switch (i) {
                        case 0:
                            hs.NormalSet = int.Parse(hitSampleParameters[0]);
                            MoveCursor(hitSampleParameters[0].Length + splitString7.Length);
                            break;

                        case 1:
                            hs.AdditionalSet = int.Parse(hitSampleParameters[1]);
                            MoveCursor(hitSampleParameters[1].Length + splitString7.Length);
                            break;

                        case 2:
                            hs.Index = int.Parse(hitSampleParameters[2]);
                            MoveCursor(hitSampleParameters[2].Length + splitString7.Length);
                            break;

                        case 3:
                            hs.Volume = int.Parse(hitSampleParameters[3]);
                            MoveCursor(hitSampleParameters[3].Length + splitString7.Length);
                            break;

                        case 4:
                            hs.Filename = hitSampleParameters[4];
                            break;
                    }
                }

                h.HitSample = hs;
                hitobjectsInfo.List.Add(h);
                ReadNextLine();
            }
            return hitobjectsInfo;
        }

        private void ExpectSection(string sectionName) {
            if (CurrerntLine != $"[{sectionName}]") {
                throw new Exception($"Expected {sectionName} section");
            }
        }

        private void ExpectFileFormatVersion() {
            var regex = new Regex(@"osu file format v[1-9][0-9]*");
            var match = regex.Match(CurrerntLine);
            if (match.Value != CurrerntLine) {
                throw new Exception(SourceFile.CounstructErrorMessage($"Expected osu file format version", currerntLineNo, 0));
            }
        }

        private bool CurrentLineIsSection {
            get {
                var regex = new Regex(@"\[[a-zA-Z]+\]");
                var match = regex.Match(CurrerntLine);
                if (match.Value == CurrerntLine) {
                    return true;
                }
                return false;
            }
        }

        private string GetCurrentLineSection() {
            if (currerntLineNo < SourceFile.Lines.Length && CurrentLineIsSection) {
                return CurrerntLine.Substring(1, CurrerntLine.Length - 2);
            }
            else {
                return null;
            }
        }

        private bool CurrentLineIsCombo {
            get {
                var regex = new Regex(@"Combo[1-9] : [0-9]{1,3},[0-9]{1,3},[0-9]{1,3}");
                var match = regex.Match(CurrerntLine);
                if (match.Value == CurrerntLine) {
                    return true;
                }
                return false;
            }
        }

        private void ReadNextLine() {
            do {
                currerntLineNo++;
            } while (currerntLineNo < SourceFile.Lines.Length && string.IsNullOrWhiteSpace(CurrerntLine));
        }

        private void ResetCursor() {
            currentInlineCursorPos = 0;
        }

        private void MoveCursor(int offset) {
            currentInlineCursorPos += offset;
        }

        private void SetPrimitiveFieldValue(object sectionInfo, string field, string value) {
            var fieldInfo = sectionInfo.GetType().GetField(field);
            if (fieldInfo != null) {
                var type = fieldInfo.FieldType;
                var converter = TypeDescriptor.GetConverter(type);
                var newFieldValue = converter.ConvertFromInvariantString(value);
                fieldInfo.SetValue(sectionInfo, newFieldValue);
            }
            else {
                using (var sw = File.AppendText("old_fields.txt")) {
                    var typeName = sectionInfo.GetType().Name;
                    sw.WriteLine($"{typeName.Substring(0, typeName.IndexOf("Info"))}: {field}");
                }
            }
        }

        private void AssertSectionInfoNotNull(object sectionInfo) {
            if (sectionInfo == null) {
                var typeName = sectionInfo.GetType().Name;
                throw new Exception($"{typeName.Substring(0, typeName.IndexOf("Info"))} was null");
            }
        }
    }
}
