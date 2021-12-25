using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_map_parser {
    static class HelperFunctions {
        public static class CLI {
            public static void WriteColored(string text, ConsoleColor bgColor, ConsoleColor fgColor) {
                var prevBgColor = Console.BackgroundColor;
                var prevFgColor = Console.ForegroundColor;
                Console.BackgroundColor = bgColor;
                Console.ForegroundColor = fgColor;
                Console.Write(text);
                Console.BackgroundColor = prevBgColor;
                Console.ForegroundColor = prevFgColor;
                Console.WriteLine();
            }

            public static string GetUserAnswer(string question, bool endWithNewline = true) {
                Console.WriteLine(question);
                var answer = Console.ReadLine();
                if (endWithNewline) {
                    Console.WriteLine();
                }
                return answer;
            }

            public static bool GetUserAnswerYesNo(string question, bool endWithNewline = true) {
                return new List<string>() { "yes", "y", "" }.Contains(
                    GetUserAnswer(question + " [yes/y/ ] to confirm, anything else to cancel.").Trim().ToLower()
                );
            }
        }

        public class Timer {
            public DateTime? StartTime { get; private set; } = null;
            public DateTime? EndTime { get; private set; } = null;

            public Timer Start() {
                StartTime = DateTime.Now;
                return this;
            }

            public double Stop() {
                EndTime = DateTime.Now;
                return (EndTime.Value - StartTime.Value).TotalSeconds;
            }
        }
    }
}
