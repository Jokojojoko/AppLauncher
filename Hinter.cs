using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace AppLauncher
{
    class Hinter
    {
        public static string ReadHintedLine<T, TResult>(IEnumerable<T> hintSource, IEnumerable<T> hintSource2, Func<T, TResult> hintField, string inputRegex = ".*", ConsoleColor hintColor = ConsoleColor.DarkGray)
        {
            ConsoleKeyInfo input;

            var suggestion = string.Empty;
            var userInput = string.Empty;
            var readLine = string.Empty;

            while (ConsoleKey.Enter != (input = Console.ReadKey()).Key)
            {
                if (input.Key == ConsoleKey.Backspace)
                    userInput = userInput.Any() ? userInput.Remove(userInput.Length - 1, 1) : string.Empty;

                else if (input.Key == ConsoleKey.Tab)
                    userInput = suggestion ?? userInput;

                else if (input.Key == ConsoleKey.Escape)
                    userInput = "";

                else if (input.Key == ConsoleKey.LeftWindows);
                else if (input.Key == ConsoleKey.DownArrow);
                else if (input.Key == ConsoleKey.UpArrow);
                else if (input.Key == ConsoleKey.LeftArrow);
                else if (input.Key == ConsoleKey.RightArrow);

                else if (input != null && Regex.IsMatch(input.KeyChar.ToString(), inputRegex))
                    userInput += input.KeyChar;
                userInput = FixRussian(userInput);
                if (userInput.Length>1 && userInput.Substring(0, 2) == "g ")
                {
                    suggestion = hintSource2.Select(item => hintField(item).ToString())
                        .FirstOrDefault(item => item.Length > userInput.Length - 2 && item.Substring(0, userInput.Length - 2) == userInput.Substring(2));
                    if (suggestion != null)
                        suggestion = "g " + suggestion;
                }
                else
                    suggestion = hintSource.Select(item => hintField(item).ToString())
                        .FirstOrDefault(item => item.Length > userInput.Length && item.Substring(0, userInput.Length) == userInput);

                readLine = suggestion == null ? userInput : suggestion;

                ClearCurrentConsoleLine();
                Console.Write(userInput);

                var originalColor = Console.ForegroundColor;

                Console.ForegroundColor = hintColor;

                if (userInput.Any())
                {
                    Console.Write(readLine.Substring(userInput.Length, readLine.Length - userInput.Length));
                }

                Console.ForegroundColor = originalColor;
            }
            ClearCurrentConsoleLine();
            Console.WriteLine(userInput);
            return userInput.Any() ? userInput : string.Empty;
        }

        static string FixRussian(string input)
        {
            Dictionary<char, char> ru_eng = new Dictionary<char, char> { ['й'] = 'q', ['ц'] = 'w', ['у'] = 'e', ['к'] = 'r', ['е'] = 't', ['н'] = 'y', ['г'] = 'u', ['ш'] = 'i', ['щ'] = 'o', ['з'] = 'p', ['х'] = '[', ['ъ'] = ']', ['ф'] = 'a', ['ы'] = 's', ['в'] = 'd', ['а'] = 'f', ['п'] = 'g', ['р'] = 'h', ['о'] = 'j', ['л'] = 'k', ['д'] = 'l', ['ж'] = ';', ['э'] = '\'', ['я'] = 'z', ['ч'] = 'x', ['с'] = 'c', ['м'] = 'v', ['и'] = 'b', ['т'] = 'n', ['ь'] = 'm', ['б'] = ',', ['ю'] = '.' };
            foreach (var letter in input)
            {
                if (ru_eng.Keys.Contains(letter))
                {
                    input = input.Replace(letter, ru_eng[letter]);
                }
            }
            return input;
        }

        private static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}
