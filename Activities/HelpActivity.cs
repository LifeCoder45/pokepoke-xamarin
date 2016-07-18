using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;

namespace PokePoke.Activities
{
    [Activity(
        Label = "@string/help_title",
        Icon = "@mipmap/ic_launcher",
        Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class HelpActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Help);

            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
        }
    }
}