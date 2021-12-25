using System.IO;

namespace osu_map_parser {
    class SourceFile {
        public readonly string Path;
        public readonly string[] Lines;

        public SourceFile(string path, string[] lines) {
            Path = path;
            Lines = lines;
        }

        public static SourceFile Read(string path) {
            var lines = File.ReadAllLines(path);
            return new SourceFile(path, lines);
        }

        public string CounstructErrorMessage(string message, int lineNo, int errorPos) {
            return $"Error. Could not parse line {lineNo + 1} at symbol {errorPos + 1}:\n" +
                   $"{message}\n\n" +
                   $"{Lines[lineNo]}\n" +
                   $"{MakeArrow(errorPos)}";
        }

        private string MakeArrow(int length) {
            var pointer = new string(' ', length) + "^";
            var body = new string('_', length) + "|";
            return pointer + "\n" + body;
        }
    }
}
