using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BottomSheetView
{
    public class InitialTouchPanGestureRecognizer : UIPanGestureRecognizer
    {
        public CGPoint initialTouchLocation;

        public InitialTouchPanGestureRecognizer(Action<UIPanGestureRecognizer> action)
        : base(action) { }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            UITouch touch = touches.AnyObject as UITouch;
            initialTouchLocation = touch.LocationInView(View);
        }
    }
}
