using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Widget;
using Java.Lang;
using Debug = System.Diagnostics.Debug;
using Thread = System.Threading.Thread;

namespace PokePoke.Services
{
    [Service(Label = "Vibration Detection Service")]
    public class VibrationDetectionService : Service
    {
        public static bool IsRunning
            => _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested;
        
        private const int NotificationId = 45;

        private const int ThrottleMillis = 500;

		private static int _debugTickCounter;

        private const int PostNotificationSleepMillis = 4000;

        private static readonly long[] NotificationVibrationPattern = {0, 500, 50, 500, 50, 750};

        private static CancellationTokenSource _cancellationTokenSource;
        
        private bool _isVibrating;
        private Toast _toast;

        public override IBinder OnBind(Intent intent) => null;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            Alert();

            DetectVibration();

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            _cancellationTokenSource.Cancel();

            Alert();
        }

        private void DetectVibration()
        {
            Task.Factory.StartNew(() =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var sleepMillis = ThrottleMillis;

                    Debug.WriteLine($"Tick: {++_debugTickCounter}");

                    try
                    {
                        var process = Runtime.GetRuntime().Exec("head /sys/class/timed_output/vibrator/enable");

                        using (var stream = new StreamReader(process.InputStream))
                        {
                            int enabled;

                            int.TryParse(stream.ReadToEnd(), out enabled);

                            // vibration is complete
                            if (_isVibrating && enabled == 0)
                                _isVibrating = false;

                            // only send for first poll during current vibration
                            if (enabled > 0 && !_isVibrating)
                            {
                                _isVibrating = true;

                                // vibrate
                                SendVibrationNotification();

                                // don't want to check while our notification is firing
                                sleepMillis = PostNotificationSleepMillis;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to check /enable: {ex.Message}");

                        _cancellationTokenSource.Cancel();
                    }

                    Thread.Sleep(sleepMillis);
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private Notification CreateNotification(int titleResource, int textResource)
        {
            var wearExtender = new NotificationCompat.WearableExtender()
                .SetHintHideIcon(true)
                .SetContentIcon(Resource.Drawable.tuft)
                .SetBackground(BitmapFactory.DecodeResource(Resources, Resource.Drawable.sky));

            return new NotificationCompat.Builder(this)
                .SetAutoCancel(true)
                .SetContentTitle(Resources.GetString(titleResource))
                .SetContentText(Resources.GetString(textResource))
                .SetSmallIcon(Resource.Drawable.tuft)
                .SetVibrate(NotificationVibrationPattern)
                .Extend(wearExtender)
                .Build();
        }
        
        private void SendVibrationNotification()
        {
            var notification = CreateNotification(
                Resource.String.notification_title,
                Resource.String.notification_text);

            (GetSystemService(NotificationService) as NotificationManager)?.Notify(NotificationId, notification);
        }
        
        private void Alert()
        {
            _toast?.Cancel();
            
            _toast = Toast.MakeText(
                this,
                _cancellationTokenSource.Token.IsCancellationRequested ? Resource.String.alert_service_disabled : Resource.String.alert_service_enabled,
                ToastLength.Long);

            _toast.Show();
        }
    }
}