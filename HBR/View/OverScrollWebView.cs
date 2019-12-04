using System;
using Android.Content;
using Android.Util;
using Android.Webkit;

namespace HBR.View
{
    public class OverScrollWebView : WebView
    {
        public Action OnOverScrollX { get; set; }
        public Action OnOverScrollY { get; set; }
        public int PageHeight { get; private set; }

        public OverScrollWebView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            VerticalScrollBarEnabled = false;
            HorizontalScrollBarEnabled = false;
            Settings.JavaScriptEnabled = true;
        }

        protected override void OnOverScrolled(int scrollX, int scrollY, bool clampedX, bool clampedY)
        {
            var ch = ContentHeight;
            var h = Height;

            if (clampedX)
            {
                OnOverScrollX?.Invoke();
            }

            if (clampedY)
            {
                //OnOverScrollY?.Invoke();
            }
            base.OnOverScrolled(scrollX, scrollY, clampedX, clampedY);
        }

        public override void LoadUrl(string url)
        {
            base.LoadUrl(url);
        }

        public void ScrollToTop()
        {
            EvaluateJavascript("document.documentElement.scrollTop = 0;", null);
        }

        public void ScrollToAnchor(string anchor)
        {
            EvaluateJavascript($"document.getElementById(\"{ anchor }\").scrollIntoView(true);", null);
        }
    }
}