using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using System;
using System.IO;
using System.Threading;

namespace IracingReplayCapture
{
    public class Recorder
    {
        private string _output;
        private string _initialRecordingPath;
        private string _initialFilenameFormatting;
        private string _initialScene;
        private string _obsAddress;
        private string _obsPassword;
        private string[] _cameras;
        private FrameRange[] _frameRanges;
        private OBSWebsocket _obs = new OBSWebsocket();
        private AutoResetEvent _recordingStarted = new AutoResetEvent(false);
        private AutoResetEvent _recordingStopped = new AutoResetEvent(false);

        public static void Record(
            string obsPassword,
            string[] cameras,
            FrameRange[] ranges,
            string output = null,
            string obsAddress = "ws://localhost:4444")
        {
            new Recorder(output, obsAddress, obsPassword, cameras, ranges).Record();
        }

        public Recorder(string output, string obsAddress, string obsPassword, string[] cameras, FrameRange[] frameRanges)

        {
            _output = output;
            _obsAddress = obsAddress;
            _obsPassword = obsPassword;
            _cameras = cameras;
            _frameRanges = frameRanges;
            _obs.RecordingStateChanged += OnRecordingStateChanged;
        }

        private void OnRecordingStateChanged(OBSWebsocket sender, OutputState state)
        {
            switch (state)
            {
                case OutputState.Started:
                    _recordingStarted.Set();
                    break;
                case OutputState.Stopped:
                    _recordingStopped.Set();
                    break;
            }
        }

        public void Record()
        {
            if (Connect())
            {
                // A clean house
                SaveInitialRecordingFolder();
                SaveInitialFilenameFormatting();
                SaveInitialScene();
                Console.CancelKeyPress += OnExit;
                AppDomain.CurrentDomain.ProcessExit += OnExit;

                var player = new Player(_cameras, _frameRanges);
                player.PlaybackStarting += OnPlaybackStarting;
                player.PlaybackEnding += OnPlaybackEnding;
                player.NewFrameRange += OnNewFrameRange;
                player.NewCamera += OnNewCamera;
                player.Play();
            }
        }

        private void SaveInitialScene()
        {
            _initialScene = _obs.GetCurrentScene().Name;
        }

        private void OnNewCamera(object sender, Player.NewCameraArgs e)
        {
            if (_output != null)
            {
                Console.WriteLine($"OBS filename: {e.Camera}");
                _obs.SetFilenameFormatting(e.Camera);
            }

            ChangeScene(e.Camera);
        }

        private void ChangeScene(string camera)
        {
            string sceneName = char.ToUpper(camera[0]) + camera.Substring(1);

            try
            {
                _obs.SetCurrentScene(sceneName);
                Console.WriteLine($"OBS scene: {sceneName}");
            }
            catch (OBSWebsocketDotNet.ErrorResponseException)
            {
                Console.WriteLine($"OBS scene {sceneName} does not exist, reverting to {_initialScene}");
                RestoreInitialScene();
            }
        }

        private void RestoreInitialScene()
        {
            _obs.SetCurrentScene(_initialScene);
        }

        private void SaveInitialFilenameFormatting()
        {
            if (_output != null)
            {
                _initialFilenameFormatting = _obs.GetFilenameFormatting();
                Console.WriteLine($"Initial OBS filename formatting: {_initialFilenameFormatting}");
            }
        }

        private void OnNewFrameRange(object sender, Player.NewFrameRangeArgs e)
        {
            if (_output != null)
            {
                string to = e.FrameRange.to == null ? "end" : e.FrameRange.to.ToString();
                string subfolder = $"{e.FrameRange.from}-{to}";
                string folder = Path.Combine(_output, subfolder);
                Console.WriteLine($"OBS recording folder: {folder}");
                _obs.SetRecordingFolder(Path.GetFullPath(folder));
            }
        }

        private void SaveInitialRecordingFolder()
        {
            if (_output != null)
            {
                _initialRecordingPath = _obs.GetRecordingFolder();
                Console.WriteLine($"Initial OBS recording folder: {_initialRecordingPath}");
            }
        }

        private void RestoreRecordingFolder()
        {
            if (_initialRecordingPath != null)
            {
                _obs.SetRecordingFolder(_initialRecordingPath);
                Console.WriteLine("Initial OBS recording folder restored");
            }
        }

        private void OnPlaybackEnding(object sender, EventArgs e)
        {
            StopRecording();
        }

        private void OnExit(object sender, EventArgs e)
        {
            Console.WriteLine("Exiting recorder");
            if (_obs.GetRecordingStatus().IsRecording)
            {
                StopRecording();
            }
            RestoreRecordingFolder();
            RestoreFilenameFormatting();
            RestoreInitialScene();
            _obs.Disconnect();
        }

        private void RestoreFilenameFormatting()
        {
            if (_initialFilenameFormatting != null)
            {
                _obs.SetFilenameFormatting(_initialFilenameFormatting);
                Console.WriteLine("Initial OBS filename formatting restored");
            }
        }

        private void OnPlaybackStarting(object sender, EventArgs e)
        {
            StartRecording();
        }

        private void StartRecording()
        {
            WithRetry("Start recording", _obs.StartRecording, _recordingStarted);
        }

        private void StopRecording(int attempt = 0)
        {
            WithRetry("Stop recording", _obs.StopRecording, _recordingStopped);
        }

        private void WithRetry(string desc, Action action, WaitHandle waitHandle, int retry = 0)
        {
            Console.WriteLine($"{desc} (attempt {retry})");
            action();

            if (!waitHandle.WaitOne(500))
            {
                WithRetry(desc, action, waitHandle, retry += 1);
            }
        }

        private bool Connect()
        {
            Console.WriteLine($"Connecting to OBS ({_obsAddress})");

            try
            {
                _obs.Connect(_obsAddress, _obsPassword);
            }
            catch (System.ArgumentException e) when (e.ParamName == "url")
            {
                Console.Error.WriteLine("Invalid --obs-address parameter. Expected format: ws://HOST:PORT");
                return false;
            }
            catch (OBSWebsocketDotNet.AuthFailureException)
            {
                Console.Error.WriteLine("OBS websocket authentication failed. Set the password with --obs-password parameter.");
                return false;
            }

            if (!_obs.IsConnected)
            {
                Console.Error.WriteLine("Cannot connect to OBS.\nMake sure it is started and that obs-websocket is installed (https://github.com/Palakis/obs-websocket/releases).");
                return false;
            }

            Console.WriteLine("Connnected to OBS");
            return true;
        }
    }
}
