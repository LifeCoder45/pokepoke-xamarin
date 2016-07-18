using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Views;
using Android.Widget;
using PokePoke.Services;

namespace PokePoke.Widgets
{
    [BroadcastReceiver(Label = "@string/app_name", Exported = true)]
    [MetaData("android.appwidget.provider", Resource = "@xml/poke_control_widget")]
    [IntentFilter(new[] {"android.appwidget.action.APPWIDGET_UPDATE", "me.pokepoke.action.TOGGLE_DETECTION_SERVICE"})]
    public class PokeControlWidget : AppWidgetProvider
    {
        public const string IntentActionToggleDetectionService = "me.pokepoke.action.TOGGLE_DETECTION_SERVICE";

        private static Intent CreateIntent(Context context)
        {
            var intent = new Intent(context, typeof (PokeControlWidget));

            intent.SetAction(IntentActionToggleDetectionService);

            return intent;
        }

        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            foreach (var id in appWidgetIds)
            {
                var remoteViews = new RemoteViews(context.PackageName, Resource.Layout.PokeControlLayout);

                var pendingIntent = PendingIntent.GetBroadcast(context, 0, CreateIntent(context), 0);

                remoteViews.SetOnClickPendingIntent(Resource.Id.button_is_off, pendingIntent);
                remoteViews.SetOnClickPendingIntent(Resource.Id.button_is_on, pendingIntent);

                appWidgetManager.UpdateAppWidget(id, remoteViews);
            }
        }

        public override void OnReceive(Context context, Intent i)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (i.Action)
            {
                case "android.appwidget.action.APPWIDGET_DELETED":
                    context.StopService(CreateIntent(context));
                    break;
                case IntentActionToggleDetectionService:
                    HandleToggleIntent(context, i);

                    break;
            }

            base.OnReceive(context, i);
        }

        private void HandleToggleIntent(Context context, Intent i)
        {
            var remoteViews = new RemoteViews(context.PackageName, Resource.Layout.PokeControlLayout);

            var intent = new Intent(context, typeof (VibrationDetectionService));

            if (VibrationDetectionService.IsRunning)
            {
                context.StopService(intent);
                remoteViews.SetViewVisibility(Resource.Id.button_is_off, ViewStates.Visible);
                remoteViews.SetViewVisibility(Resource.Id.button_is_on, ViewStates.Gone);
            }
            else
            {
                context.StartService(intent);
                remoteViews.SetViewVisibility(Resource.Id.button_is_off, ViewStates.Gone);
                remoteViews.SetViewVisibility(Resource.Id.button_is_on, ViewStates.Visible);
            }

            AppWidgetManager
                .GetInstance(context)
                .UpdateAppWidget(new ComponentName(context, Class), remoteViews);
        }
    }
}