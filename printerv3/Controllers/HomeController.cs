using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using Microsoft.AspNetCore.Mvc;
using printerv3.Models;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace POSWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static BasePrinter printer;
        private static ICommandEmitter e;


        private static string ip = "192.168.1.226";
        private static string networkPort = "9100";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Print()
        {
            printer = new NetworkPrinter(settings: new NetworkPrinterSettings() { ConnectionString = $"{ip}:{networkPort}" });
            e = new EPSON();
            printer.Write(Tests.SingleLinePrinting(e));

            return Ok(Tests.SingleLinePrinting(e));
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
