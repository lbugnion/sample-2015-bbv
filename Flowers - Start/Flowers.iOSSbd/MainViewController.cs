using System;
using Flowers.Data.ViewModel;
using Foundation;
using GalaSoft.MvvmLight.Helpers;
using SDWebImage;
using UIKit;

namespace Flowers.iOSSbd
{
    partial class MainViewController : UIViewController
    {
        private Binding<string, string> _lastLoadedBinding;
        private ObservableTableViewController<FlowerViewModel> _tableViewController;

        private MainViewModel Vm
        {
            get
            {
                return Application.Locator.Main;
            }
        }

        public MainViewController(IntPtr handle)
            : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

    }
}