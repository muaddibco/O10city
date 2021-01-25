using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace O10Wallet.Droid.Views
{
    public class NotificationAlertView : FrameLayout
    {
        public NotificationAlertView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize(context);
        }

        public NotificationAlertView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize(context);
        }

        private void Initialize(Context context)
        {
            Inflate(context, Resource.Layout.NotificationAlert, this);
        }

        public void SetMessage(string msg)
        {
            TextView textView = (TextView)FindViewById(Resource.Id.notification_alert_msg);
            if(textView != null)
            {
                textView.Text = msg;
            }
        }
    }
}