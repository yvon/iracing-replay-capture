using System;
using System.Threading;
using System.Threading.Tasks;
using iRacingSdkWrapper.Bitfields;

namespace IracingReplayCapture
{
	public class UiController
	{
		Player _player;
		uint _delay;
		CancellationTokenSource _cancellationTokenSource;

		public UiController(Player player, uint delay)
		{
			_player = player;
			_delay = delay;
			player.NewCamera += OnNewCamera;

			Console.CancelKeyPress += OnExit;
			AppDomain.CurrentDomain.ProcessExit += OnExit;
		}

		private void OnExit(object sender, EventArgs e)
		{
			if (_player.Sdk != null)
			{
				RestoreInterface();
			}
		}

		private void OnNewCamera(object sender, Player.NewCameraArgs e)
		{
			if (_delay == 0)
			{
				HideInterface();
			}
			else
			{
				HideInterfaceWithDelay();
			}
		}

		private void HideInterfaceWithDelay()
		{
			RestoreInterface();

			// Cancel previous task, if any
			if (_cancellationTokenSource != null)
			{
				_cancellationTokenSource.Cancel();
			}

			_cancellationTokenSource = new CancellationTokenSource();

			Task.Run(async delegate
			{
				await Task.Delay(TimeSpan.FromMilliseconds(_delay), _cancellationTokenSource.Token);
				HideInterface();
			});
		}


		private void HideInterface()
		{
			Console.WriteLine("Hiding interface");
			_player.Sdk.Camera.SetCameraState(new CameraState((int)CameraStates.UIHidden));
		}

		private void RestoreInterface()
		{
			Console.WriteLine("Bringing back the interface");
			_player.Sdk.Camera.SetCameraState(new CameraState());
		}
	}
}
