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

namespace HBR
{
    public class ActivityMediator
    {
        public delegate void MessageReceivedEventHandler(string message);
        public event MessageReceivedEventHandler ActivityMessageReceived;

        public void Send(string response)
        {
            ActivityMessageReceived?.Invoke(response);
        }
    }
}