using CoreGraphics;
using UIKit;

namespace Toasts.Forms.Plugin.iOS
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

