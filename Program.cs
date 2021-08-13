using System.CommandLine.Builder;

namespace IracingReplayCapture
{
    class Program
    {
        static int Main(string[] args)
        {
            return CommandBuilder.InvokeRootCommand(args);
        }
    }
}
