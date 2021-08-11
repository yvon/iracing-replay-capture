using iRacingSdkWrapper;
using iRacingSdkWrapper.Bitfields;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using static iRacingSdkWrapper.SdkWrapper;

namespace IracingReplayCapture
{
    public class Player
    {
        string[] _cameras;
        FrameRange[] _frameRanges;
        SdkWrapper _sdk;
        SessionInfo _sessionInfo;
        Dictionary<string, int> _camera_ids;

        public class NewFrameRangeArgs : EventArgs
        {
            public FrameRange FrameRange;

            public NewFrameRangeArgs(FrameRange frameRange)
            {
                FrameRange = frameRange;
            }
        }

        public class NewCameraArgs : EventArgs
        {
            public string Camera;

            public NewCameraArgs(string camera)
            {
                Camera = camera;
            }
        }

        public event EventHandler PlaybackStarting;
        public event EventHandler PlaybackEnding;
        public event EventHandler<NewFrameRangeArgs> NewFrameRange;
        public event EventHandler<NewCameraArgs> NewCamera;

        static public void Play(string[] cameras, FrameRange[] ranges)
        {
            new Player(cameras, ranges).Play();
        }

        public Player(string[] cameras, FrameRange[] frameRanges)
        {
            _cameras = cameras;

            if (frameRanges.Length == 0)
            {
                var wholeTape = new FrameRange { from = 0 };
                _frameRanges = new[] { wholeTape };
                return;
            }

            _frameRanges = frameRanges;
        }

        public void Play()
        {
            ConnectToIracing();
            GetCameraIds();
            HideInterface();

            Console.CancelKeyPress += OnExit;
            AppDomain.CurrentDomain.ProcessExit += OnExit;

            foreach (FrameRange frameRange in _frameRanges)
            {
                Console.WriteLine(frameRange);
                NewFrameRange?.Invoke(this, new NewFrameRangeArgs(frameRange));
                foreach (string camera in _cameras)
                {
                    string normalizedCamera = CleanWhitespaces(camera);

                    if (SwitchCamera(normalizedCamera))
                    {
                        NewCamera?.Invoke(this, new NewCameraArgs(normalizedCamera));
                        PlayRange(frameRange);
                    }
                }
            }
            Environment.Exit(0);
        }

        private static readonly Regex sWhitespace = new Regex(@"\s+");

        private static string CleanWhitespaces(string input, string replacement = "")
        {
            return sWhitespace.Replace(input, replacement);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Console.WriteLine("Exiting player");
            _sdk.Replay.Pause();
            _sdk.Stop();
        }

        private void PlayRange(FrameRange frameRange)
        {
            _sdk.Replay.SetPosition((int)frameRange.from);
            WaitForTelemetryEvent((info) => frameRange.from == info.ReplayFrameNum.Value);
            PlaybackStarting?.Invoke(this, EventArgs.Empty);
            _sdk.Replay.SetPlaybackSpeed(1);

            WaitForTelemetryEvent((info) =>
            {
                return frameRange.to == null ?
                    info.ReplayFrameNumEnd.Value <= 1 :
                    frameRange.to <= info.ReplayFrameNum.Value;
            });

            PlaybackEnding?.Invoke(this, EventArgs.Empty);
        }

        private delegate bool TelemetryCondition(TelemetryInfo info);

        private void WaitForTelemetryEvent(TelemetryCondition cond)
        {
            AutoResetEvent condMet = new AutoResetEvent(false);

            void handler(object s, TelemetryUpdatedEventArgs e)
            {
                if (cond(e.TelemetryInfo))
                {
                    condMet.Set();
                }
            }

            _sdk.TelemetryUpdated += handler;
            condMet.WaitOne();
            _sdk.TelemetryUpdated -= handler;
        }

        private bool SwitchCamera(string name)
        {
            if (!_camera_ids.TryGetValue(name, out int id))
            {
                Console.Error.WriteLine($"Camera {name} does not exist!");
                return false;
            }

            Console.WriteLine($"{name.ToUpper()} camera (#{id})");
            _sdk.Camera.SwitchToCar(_sdk.DriverId, id);
            return true;
        }

        private void GetCameraIds()
        {
            _camera_ids = new Dictionary<string, int>();

            for (int i = 1; i < 100; i++)
            {
                var query = _sessionInfo["CameraInfo"]["Groups"]["GroupNum", i];
                if (query["GroupName"].TryGetValue(out string name))
                {
                    _camera_ids.Add(CleanWhitespaces(name.ToLower()), i);
                }
            }
        }
        private void HideInterface()
        {
            Console.WriteLine("Hiding interface");
            _sdk.Camera.SetCameraState(new CameraState((int)CameraStates.UIHidden));
        }

        private void ConnectToIracing()
        {
            var connectionInfo = Simulator.Connect();
            _sdk = connectionInfo.Sdk;
            _sessionInfo = connectionInfo.SessionInfo;
        }
    }
}
