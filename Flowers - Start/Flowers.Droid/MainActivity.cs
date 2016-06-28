using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Flowers.Data.ViewModel;
using Flowers.Helpers;
using GalaSoft.MvvmLight.Helpers;

namespace Flowers
{
    [Activity(Label = "Flowers", MainLauncher = true, Icon = "@drawable/icon")]
    public partial class MainActivity : AdapterView.IOnItemClickListener
    {
        private Binding _lastLoadedBinding;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

        }

    }
}