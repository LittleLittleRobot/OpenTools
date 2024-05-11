using Wesky.Net.OpenTools.NetworkExtensions;

namespace TestConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var hostname = "192.168.0.1";
            var result = PingHelper.PingHost(hostname, 120);
            Console.WriteLine(result.Message);
            Console.ReadLine();
        }
    }
}
