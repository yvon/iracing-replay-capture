using iRacingSdkWrapper;
using System;
using System.Threading;
using static iRacingSdkWrapper.SdkWrapper;

namespace IracingReplayCapture
{
    static class Simulator
    {
        public struct ConnectionInfo
        {
            public SdkWrapper Sdk;
            public SessionInfo SessionInfo;
        }

        static public ConnectionInfo Connect()
        {
            EventHandler<SessionInfoUpdatedEventArgs> handler = null;
            ManualResetEvent connected = new ManualResetEvent(false);
            ConnectionInfo connectionInfo = new ConnectionInfo(); ;

            Console.WriteLine("Waiting for Iracing...");

            handler = (s, e) =>
            {
                Console.WriteLine("Connected to Iracing");
                connectionInfo.SessionInfo = e.SessionInfo;
                connected.Set();
            };

            var sdk = connectionInfo.Sdk = new SdkWrapper();
            sdk.SessionInfoUpdated += handler;

            Thread t = new Thread(new ThreadStart(sdk.Start));
            t.Start();
            connected.WaitOne();

            return connectionInfo;
        }
    }
}
