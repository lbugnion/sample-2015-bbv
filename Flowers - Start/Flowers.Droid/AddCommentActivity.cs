using Android.App;
using Android.OS;
using Flowers.Data.ViewModel;
using GalaSoft.MvvmLight.Helpers;
using GalaSoft.MvvmLight.Views;

namespace Flowers
{
    [Activity(Label = "Add Comment")]
    public partial class AddCommentActivity
    {
        private Binding<string, string> _saveBinding;

        private FlowerViewModel Vm
        {
            get;
            set;
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.AddComment);

        }
    }
}