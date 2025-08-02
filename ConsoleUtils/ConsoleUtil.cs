using System.Reflection;

namespace ConsoleUtils
{
    public class ConsoleUtil
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public static string AskUser(string prompt, int timeout = Timeout.Infinite, string dflt = "")
        {
            var promptMsg = $"{prompt}: ";
            log.Debug($"Asking user: \"{promptMsg}\"");
            Console.Write(promptMsg);
            try
            {
                var resp = ReadLine(timeout);
                log.Debug($"User responded: \"{resp}\"");
                return resp;
            }
            catch (TimeoutException)
            {
                Console.WriteLine();
                log.Info($"Timed out. Defaulting to \"{dflt}\"");
                return dflt;
            }
        }

        public static bool AskUserYesNo(string prompt, int timeout = Timeout.Infinite, bool dflt = true)
        {
            var promptMsg = $"{prompt} (y/n): ";
            log.Debug($"Asking user: \"{promptMsg}\"");
            while (true)
            {
                Console.Write(promptMsg);
                try
                {
                    var resp = ReadLine(timeout);
                    if (resp == "y")
                    {
                        log.Debug($"User responded: \"{resp}\"");
                        return true;
                    }
                    if (resp == "n")
                    {
                        log.Debug($"User responded: \"{resp}\"");
                        return false;
                    }
                }
                catch (TimeoutException)
                {
                    Console.WriteLine();
                    log.Info($"Timed out. Defaulting to \"{(dflt ? "y" : "n")}\"");
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
            log.Debug($"Asking user: \"{promptMsg}\"");
            while (true)
            {
                Console.Write(promptMsg);
                try
                {
                    var resp = ReadLine(timeoutMilis);
                    log.Debug($"User responded: \"{resp}\"");

                    // try parse as integer
                    if (int.TryParse(resp, out int intRes))
                    {
                        if (Enum.IsDefined(typeof(T), intRes))
                        {
                            // compiler can't cast int to generic type (T), here we just force the issue by casting to object first.
                            var res = (T)(object)intRes;
                            log.Debug($"User input parsed to: \"{res}\"");
                            return res;
                        }
                    }
                    else if (Enum.TryParse(resp, out T res))
                    {
                        log.Debug($"User input parsed to: \"{res}\"");
                        return res;
                    }
                }
                catch (TimeoutException)
                {
                    Console.WriteLine();
                    log.Info($"Timed out. Defaulting to \"{dflt}\"");
                    return dflt;
                }
                Console.WriteLine($"Unable to parse input. Please respond with one of then given options via the integer value or the written name.");
            }
        }

        public static string ReadLine(int timeoutMilis = Timeout.Infinite)
        {
            return Reader.ReadLine(timeoutMilis);
        }

        public static string ReadClipboard(string prompt)
        {
            var promptMsg = $"{prompt} Please copy your text and then hit enter: ";
            log.Debug($"Asking user: \"{promptMsg}\"");
            Console.Write(promptMsg);

            while (true)
            {
                ReadLine();
                var text = Clipboard.GetText();
                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
                Console.Write("No text present on clipboard. Please copy your text and hit enter: ");
            }
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