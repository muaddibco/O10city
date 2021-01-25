using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using O10.Client.Mobile.Base.Interfaces;
using O10Wallet.Droid.Services;

[assembly: Xamarin.Forms.Dependency(typeof(ToastAndroid))]
namespace O10Wallet.Droid.Services
{
    public class ToastAndroid : IToast
    {
        public void LongMessage(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
        }

        public void ShortMessage(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
        }
    }
}