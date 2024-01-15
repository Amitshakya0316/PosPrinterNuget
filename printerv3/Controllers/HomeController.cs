using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Newtonsoft.Json;


namespace POSWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static BasePrinter printer;
        private static ICommandEmitter e;


        private static string ip = "192.168.1.226";
        private static string networkPort = "9100";
        private static PrinterStatusEventArgs status = new PrinterStatusEventArgs();


        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Print()
        {
            printer = new NetworkPrinter(settings: new NetworkPrinterSettings() { ConnectionString = $"{ip}:{networkPort}" });
            e = new EPSON();
            Setup(true);

            printer.Write(Tests.SingleLinePrinting(e));


            return Ok(status.IsPrinterOnline);
        }
        private static void StatusChanged(object sender, EventArgs ps)
        {
            status = (PrinterStatusEventArgs)ps;
            if (status == null) { Console.WriteLine("Status was null - unable to read status from printer."); return; }
            Console.WriteLine($"Printer Online Status: {status.IsPrinterOnline}");
            Console.WriteLine(JsonConvert.SerializeObject(status));
        }
        private static bool _hasEnabledStatusMonitoring = false;

        private static void Setup(bool enableStatusBackMonitoring)
        {
            if (printer != null)
            {
                // Only register status monitoring once.
                if (!_hasEnabledStatusMonitoring)
                {
                    printer.StatusChanged += StatusChanged;
                    _hasEnabledStatusMonitoring = true;
                }
                printer?.Write(e.Initialize());
                printer?.Write(e.Enable());
                if (enableStatusBackMonitoring)
                {
                    printer.Write(e.EnableAutomaticStatusBack());
                }
            }
        }
    }
    public static partial class Tests
    {
        public static byte[][] MultiLinePrinting(ICommandEmitter e) => new byte[][] {
            e.Print("Multiline Test: Windows...\r\nOSX...\rUnix...\n"),
            //TODO: sanitize test.
            e.PrintLine("Feeding 250 dots."),
            e.FeedDots(250),
            e.PrintLine("Feeding 3 lines."),
            e.FeedLines(3),
            e.PrintLine("Done Feeding."),
            e.PrintLine("Reverse Feeding 6 lines."),
            e.FeedLinesReverse(6),
            e.PrintLine("Done Reverse Feeding.")
        };

        public static byte[][] SingleLinePrinting(ICommandEmitter e) => new byte[][] {
            e.Print("This is a single line from my controller\r\n This is second single line from my controller\r\nThis is third line from my controller\r\n"),
        };
    }
}
