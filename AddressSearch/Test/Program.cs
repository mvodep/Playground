using Dapper;
using Npgsql;
using phonet4n.Core;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Test
{
    class Program
    {
        private static Phonetizer phonet = new Phonetizer(true);
        private static Regex addressRegex = new Regex(@"^(?<street>[^\d\,]*[^\d,\s])\s*(?<house>[\/\dA-Za-z]+)?(?:,\s*(?<plz>\d{1,4})\s*(?<city>.*))?$", RegexOptions.Compiled);
        private static object syncRoot = new object();

        static void Main(string[] args)
        {
            var timer = new System.Timers.Timer();
            var inputBuffer = new StringBuilder();

            timer.Elapsed += (sender, e) =>
            {               
                ((System.Timers.Timer)sender).Stop();

                lock (syncRoot)
                {
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine(inputBuffer);
                    Console.SetCursorPosition(0, 2);
                    Console.WriteLine(GetResult(inputBuffer.ToString()));
                    Console.SetCursorPosition(inputBuffer.Length, 0);
                }
            };

            timer.Interval = 1000;
            timer.Enabled = false;
           
            while (true)
            {
                var pressedKey = Console.ReadKey();

                timer.Stop();
                timer.Start();

                if (!char.IsControl(pressedKey.KeyChar))
                {
                    inputBuffer.Append(pressedKey.KeyChar);
                }

                var charWasRemoved = false;

                if (pressedKey.Key == ConsoleKey.Backspace && inputBuffer.Length > 0)
                {
                    inputBuffer.Remove(inputBuffer.Length - 1, 1);
                    charWasRemoved = true;
                }

                lock (syncRoot)
                {
                    Console.SetCursorPosition(0, 0);
                    Console.Write(inputBuffer);

                    if(charWasRemoved)
                    {
                        Console.Write(" ");
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    }
                }
            }
        }

        private static string GetResult(string userQuery)
        {
            var match = addressRegex.Match(userQuery);

            if (match.Success)
            {
                var result = new StringBuilder();

                using (var connection = new NpgsqlConnection($"Server=localhost;Database=playground;User Id=postgres;Password=postgres;"))
                {
                    connection.Open();

                    var result_tgrm = Enumerable.Empty<string>();
                    var result_phonet = Enumerable.Empty<string>();

                    if (match.Groups["plz"].Success)
                    {
                        result_tgrm = connection.Query<string>("SELECT CONCAT(stroffi, ', ', plznr, ' ', zustort, ' ', (stroffi <-> @name)) FROM playground.streets WHERE plznr LIKE @plz AND stroffi <-> @name < 0.9 ORDER BY stroffi <-> @name LIMIT 10;", new { name = match.Groups["street"].Value, plz = $"{match.Groups["plz"].Value }%" });
                        result_phonet = connection.Query<string>("SELECT CONCAT(stroffi, ', ', plznr, ' ', zustort, ' ', (code <-> @phonet)) FROM playground.streets WHERE plznr LIKE @plz AND code <-> @phonet < 0.9 ORDER BY code <-> @phonet LIMIT 10;", new { phonet = phonet.Phonetize(match.Groups["street"].Value), plz = $"{match.Groups["plz"].Value }%" });
                    }
                    else
                    {
                        result_tgrm = connection.Query<string>("SELECT CONCAT(stroffi, ', ', plznr, ' ', zustort, ' ', (stroffi <-> @name)) FROM playground.streets WHERE stroffi <-> @name < 0.9 ORDER BY stroffi <-> @name LIMIT 10;", new { name = match.Groups["street"].Value });
                        result_phonet = connection.Query<string>("SELECT CONCAT(stroffi, ', ', plznr, ' ', zustort, ' ', (code <-> @phonet)) FROM playground.streets WHERE code <-> @phonet < 0.9 ORDER BY code <-> @phonet LIMIT 10;", new { phonet = phonet.Phonetize(match.Groups["street"].Value) });
                    }

                    result.AppendLine(string.Join(Environment.NewLine, result_tgrm));
                    result.AppendLine();
                    result.AppendLine(string.Join(Environment.NewLine, result_phonet));

                    return result.ToString();
                }
            }
            else
            {
                return "Syntax error: <streetname> <optinal doornumber>, <optional plz> <optinal city>";
            }
        }
    }