using System.Windows.Forms;

namespace ConsoleUtils
{
    public class ConsoleUtil
    {
        public static string AskUser(string prompt, int timeout = Timeout.Infinite, string dflt = "")
        {
            var promptMsg = $"{prompt}: ";
            Console.Write(promptMsg);
            try
            {
                var resp = ReadLine(timeout);
                return resp;
            }
            catch (TimeoutException)
            {
                Console.WriteLine();
                return dflt;
            }
        }

        public static bool AskUserYesNo(string prompt, int timeout = Timeout.Infinite, bool dflt = true)
        {
            var promptMsg = $"{prompt} (y/n): ";
            while (true)
            {
                Console.Write(promptMsg);
                try
                {
                    var resp = ReadLine(timeout);
                    if (resp == "y")
                    {
                        return true;
                    }
                    if (resp == "n")
                    {
                        return false;
                    }
                }
                catch (TimeoutException)
                {
                    Console.WriteLine();
                    return dflt;
                }
                Console.WriteLine($"Please respond with \"y\" or \"n\".");
            }
        }

        public static T AskUserEnum<T>(string prompt, int timeoutMilis = Timeout.Infinite, T dflt = default(T)) where T : struct
        {
            var enums = Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(val => Tuple.Create(Enum.GetName(typeof(T), val), val))
                .ToArray();

            // compiler can't cast int to generic type (T), here we just force the issue by casting to object first.
            var inputPrompt = string.Join(
                ", ",
                enums.OrderBy(x => x.Item2).Select(x => $"[{(int)(object)x.Item2}] {x.Item1}")
            );
            var promptMsg = $"{prompt} {inputPrompt}: ";
            while (true)
            {
                Console.Write(promptMsg);
                try
                {
                    var resp = ReadLine(timeoutMilis);

                    // try parse as integer
                    if (int.TryParse(resp, out int intRes))
                    {
                        if (Enum.IsDefined(typeof(T), intRes))
                        {
                            // compiler can't cast int to generic type (T), here we just force the issue by casting to object first.
                            var res = (T)(object)intRes;
                            return res;
                        }
                    }
                    else if (Enum.TryParse(resp, out T res))
                    {
                        return res;
                    }
                }
                catch (TimeoutException)
                {
                    Console.WriteLine();
                    return dflt;
                }
                Console.WriteLine($"Unable to parse input. Please respond with one of then given options via the integer value or the written name.");
            }
        }

        public static string ReadLine(int timeoutMilis = Timeout.Infinite)
        {
            return Reader.ReadLine(timeoutMilis);
        }
    }

    class Reader
    {
        private static readonly Thread inputThread;
        private static readonly AutoResetEvent getInput;
        private static readonly AutoResetEvent gotInput;
        private static string input = string.Empty;

        static Reader()
        {
            getInput = new AutoResetEvent(false);
            gotInput = new AutoResetEvent(false);
            inputThread = new Thread(Read)
            {
                IsBackground = true
            };
            inputThread.Start();
        }

        private static void Read()
        {
            while (true)
            {
                getInput.WaitOne();
                input = Console.ReadLine() + "";
                gotInput.Set();
            }
        }

        public static string ReadLine(int timeOutMillisecs = Timeout.Infinite)
        {
            getInput.Set();
            bool success = gotInput.WaitOne(timeOutMillisecs);
            if (success)
                return input;
            else
                throw new TimeoutException("User did not provide input within the timelimit.");
        }
    }
}