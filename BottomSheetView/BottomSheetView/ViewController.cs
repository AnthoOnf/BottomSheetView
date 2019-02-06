using System;
using BottomSheetView.Sources;
using UIKit;

namespace BottomSheetView
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        partial void DynamicHeight_TouchUpInside(UIButton sender)
        {
            var controller = UIStoryboard.FromName("Main", null).InstantiateViewController("DynamicHeightController");

            var bottomSheetViewController = new BottomSheetViewController(controller);

            this.PresentModalViewController(bottomSheetViewController, false);
        }

        partial void FixedHeight_TouchUpInside(UIButton sender)
        {
            var controller = UIStoryboard.FromName("Main", null).InstantiateViewController("DynamicHeightController");

            SheetSize[] sheetSizes = { SheetSize.Fixed(300f) };
            var bottomSheetViewController = new BottomSheetViewController(controller, sheetSizes);

            this.PresentModalViewController(bottomSheetViewController, false);
        }

        partial void NavigationController_TouchUpInside(UIButton sender)
        {
            var controller = UIStoryboard.FromName("Main", null).InstantiateViewController("BottomSheetNavigationController");

            SheetSize[] sheetSizes = { SheetSize.Fixed(300f) };
            var bottomSheetViewController = new BottomSheetViewController(controller, sheetSizes);

            this.PresentModalViewController(bottomSheetViewController, false);
        }
    }
}
