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
        }
    }
}
