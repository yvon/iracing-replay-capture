using System;

namespace IracingReplayCapture
{
    static public class ReplayControl
    {
        static public void Goto(int frameNum)
        {
            var connectionInfo = Simulator.Connect();
            connectionInfo.Sdk.Replay.SetPosition(frameNum);
            Console.WriteLine($"Playhead now at frame {frameNum}");
            connectionInfo.Sdk.Stop();
        }

        static public void PrintCurrentFrame()
        {
            var connectionInfo = Simulator.Connect();
            connectionInfo.Sdk.TelemetryUpdated += (s, e) =>
            {
                int frame = e.TelemetryInfo.ReplayFrameNum.Value;
                Console.WriteLine($"Current frame: {frame}");
                connectionInfo.Sdk.Stop();
            };
        }
    }
}
