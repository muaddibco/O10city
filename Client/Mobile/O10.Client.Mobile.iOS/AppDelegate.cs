using Foundation;
using System;
using System.Threading.Tasks.Dataflow;
using UIKit;
using O10Wallet.Base.Mobile;
using O10Wallet.Base.Mobile.Interfaces;

namespace O10Wallet.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        private TransformBlock<ISynchronizerServiceBinder, ISynchronizerServiceBinder> _synchronizerConnected;

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Rg.Plugins.Popup.Popup.Init();
            global::Xamarin.Forms.Forms.Init();
            global::Xamarin.Forms.FormsMaterial.Init();
            _synchronizerConnected = new TransformBlock<ISynchronizerServiceBinder, ISynchronizerServiceBinder>(s => s);
            LoadApplication(new App(_synchronizerConnected));

            ZXing.Net.Mobile.Forms.iOS.Platform.Init();
            return base.FinishedLaunching(app, options);
        }
    }
}
