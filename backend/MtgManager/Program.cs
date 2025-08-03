using System.Reflection;
using System.Text;
using log4net.Config;
using ConsoleUtils;
using MtgManager.ImportMoxfield;
using MtgManager.ComputeDiff;
using MtgManager.ChooseImages;
using MtgManager.GenerateProxies;
using MtgManager.Test;

namespace MtgManager
{
    public enum ProxyTool
    {
        ImportMoxfield,
        ComputeDiff,
        ChooseImages,
        GenerateProxies,
        Test
    }

    public class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        [STAThread]
        static void Main()
        {
            Init();

            var succ = true;
            try
            {
                RunProgram();
            }
            catch (Exception ex)
            {
                // rethrowing causes duplicate error message on console, just fatal log and exit instead
                log.Fatal(ex);
                succ = false;
            }

            if (!succ)
            {
                ConsoleUtil.AskUser("Program finished. Press any key to exit");
            }
            else
            {
                log.Info("Program finished. Exiting...");
                Thread.Sleep(4 * 1000);
            }
        }

        private static void RunProgram()
        {
            var cont = true;
            while (cont)
            {
                var proxyTool = ConsoleUtil.AskUserEnum<ProxyTool>("Which proxy tool to run?");
                switch (proxyTool)
                {
                    case ProxyTool.ImportMoxfield:
                        ImportMoxfieldStep.RunStep();
                        break;
                    case ProxyTool.ComputeDiff:
                        ComputeDiffStep.RunStep();
                        break;
                    case ProxyTool.ChooseImages:
                        ChooseImagesStep.RunStep();
                        break;
                    case ProxyTool.GenerateProxies:
                        GenerateProxiesStep.RunStep();
                        break;
                    case ProxyTool.Test:
                        MtgProxyTest.RunTest();
                        break;
                    default:
                        throw new Exception("Unhandled enum: " + proxyTool);
                }
                cont = ConsoleUtil.AskUserYesNo("Step completed, would you like to continue?");
            }
        }

        private static void Init()
        {
            // allows for colored console appender to be instantiated in log4net
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            log.Info("Starting MtgManager");
        }
    }
}
