using CoreGraphics;
using UIKit;

namespace Plugin.Toasts
{
    public class MessageWindow : UIWindow
    {
        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            var hitView = base.HitTest(point, uievent);
            if (hitView == RootViewController.View)
                hitView = null;
            return hitView;
        }
    }
}

