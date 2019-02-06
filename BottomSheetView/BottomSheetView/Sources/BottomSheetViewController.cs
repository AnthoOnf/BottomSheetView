using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BottomSheetView.Sources
{
    public class BottomSheetViewController : UIViewController, IUIGestureRecognizerDelegate
    {
        public UIView ContainerView = new UIView();
        public UIView PullBarView = new UIView();
        public bool DismissOnBackgroundTap = true;
        public UIViewController ChildViewController { get; private set; }

        private SheetSize _containerSize = SheetSize.Fixed(520f);
        private SheetSize _actualContainerSize = SheetSize.Fixed(520f);
        private SheetSize[] _orderedSheetSize = { SheetSize.Fixed(520f), SheetSize.FullScreen };

        private CGPoint _firstPanPoint = CGPoint.Empty;
        private UIColor _overlayColor => UIColor.FromRGBA(0, 0, 0, (int)70 * 255);
        private NSLayoutConstraint _containerHeightConstraint;
        private InitialTouchPanGestureRecognizer _panGestureRecognizer;
        private UIScrollView _childScrollView { get; set; }

        public BottomSheetViewController() : base()
        {
            this.ModalPresentationStyle = UIModalPresentationStyle.OverFullScreen;
        }

        public BottomSheetViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.View.BackgroundColor = UIColor.Clear;
            SetUpContainerView();
            SetUpDismissView();

            var panGestureRecognizer = new InitialTouchPanGestureRecognizer(Panned);

            this.View.AddGestureRecognizer(panGestureRecognizer);
            panGestureRecognizer.Delegate = this;
            _panGestureRecognizer = panGestureRecognizer;

            SetUpChildViewController();
            SetUpPullBarView();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            UIView.Animate(0.3, 0, UIViewAnimationOptions.CurveEaseOut, () =>
            {
                this.View.BackgroundColor = _overlayColor;
                ContainerView.Transform = CGAffineTransform.MakeIdentity();
                _actualContainerSize = SheetSize.Fixed((float)ContainerView.Frame.Height);
            }, null);
        }

        private UIEdgeInsets safeAreaInsets()
        {
            var insets = UIEdgeInsets.Zero;
            insets = UIApplication.SharedApplication.KeyWindow?.SafeAreaInsets ?? insets;

            insets.Top = (float)Math.Max(insets.Top, 20);

            return insets;
        }

        public void Init(UIViewController controller, SheetSize[] sizes)
        {
            ChildViewController = controller;
            _containerHeightConstraint = new NSLayoutConstraint();
            if (sizes.Count() > 0)
                SetSizes(sizes, false);
            this.ModalPresentationStyle = UIModalPresentationStyle.OverFullScreen;
        }

        public void SetSizes(SheetSize[] sizes, bool animated = true)
        {
            if (sizes.Length < 0)
                return;
            _orderedSheetSize = sizes;
            _orderedSheetSize.OrderBy(ss => ss.Value);
            Resize(sizes[0]);
        }

        public void Resize(SheetSize toSize, bool animated = true)
        {
            if (animated)
            {
                UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                {
                    _containerHeightConstraint.Constant = HeightFor(toSize);
                    this.View.LayoutIfNeeded();
                }, null);
            }
            else
            {
                _containerHeightConstraint.Constant = HeightFor(toSize);
            }
            _containerSize = toSize;
            _actualContainerSize = toSize;
        }

        private void SetUpContainerView()
        {
            ContainerView.BackgroundColor = UIColor.Clear;
            ContainerView.TranslatesAutoresizingMaskIntoConstraints = false;

            this.View.AddSubview(ContainerView);
            this.View.AddConstraint(NSLayoutConstraint.Create(this.View, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1, 0));
            this.View.AddConstraint(NSLayoutConstraint.Create(this.View, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Leading, 1, 0));
            this.View.AddConstraint(NSLayoutConstraint.Create(this.View, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Trailing, 1, 0));
            _containerHeightConstraint = NSLayoutConstraint.Create(ContainerView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, HeightFor(_containerSize));
            _containerHeightConstraint.Priority = 900;

            ContainerView.AddConstraint(_containerHeightConstraint);
            ContainerView.Transform = CGAffineTransform.MakeTranslation(0, UIScreen.MainScreen.Bounds.Height);
        }

        private void SetUpChildViewController()
        {
            ChildViewController.WillMoveToParentViewController(this);
            AddChildViewController(ChildViewController);
            ContainerView.TranslatesAutoresizingMaskIntoConstraints = false;
            ChildViewController.View.TranslatesAutoresizingMaskIntoConstraints = false;
            ContainerView.AddSubview(ChildViewController.View);

            var bottomInset = this.AdditionalSafeAreaInsets.Bottom;

            ContainerView.AddConstraint(NSLayoutConstraint.Create(ContainerView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ChildViewController.View, NSLayoutAttribute.Top, 1, 0));
            ContainerView.AddConstraint(NSLayoutConstraint.Create(ContainerView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ChildViewController.View, NSLayoutAttribute.Bottom, 1, bottomInset));
            ContainerView.AddConstraint(NSLayoutConstraint.Create(ContainerView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, ChildViewController.View, NSLayoutAttribute.Leading, 1, 0));
            ContainerView.AddConstraint(NSLayoutConstraint.Create(ContainerView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, ChildViewController.View, NSLayoutAttribute.Trailing, 1, 0));

            ChildViewController.View.Layer.MaskedCorners = CoreAnimation.CACornerMask.MaxXMinYCorner | CoreAnimation.CACornerMask.MinXMinYCorner;
            ChildViewController.View.Layer.CornerRadius = 10;
            ChildViewController.View.Layer.MasksToBounds = true;

            ChildViewController.DidMoveToParentViewController(this);
        }

        private void SetUpDismissView()
        {
            UIView DismissAreaView = new UIView(CGRect.Empty);
            DismissAreaView.BackgroundColor = UIColor.Clear;
            DismissAreaView.TranslatesAutoresizingMaskIntoConstraints = false;
            this.View.AddSubview(DismissAreaView);

            this.View.AddConstraint(NSLayoutConstraint.Create(this.View, NSLayoutAttribute.Top, NSLayoutRelation.Equal, DismissAreaView, NSLayoutAttribute.Top, 1, 0));
            this.View.AddConstraint(NSLayoutConstraint.Create(this.View, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, DismissAreaView, NSLayoutAttribute.Leading, 1, 0));
            this.View.AddConstraint(NSLayoutConstraint.Create(this.View, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, DismissAreaView, NSLayoutAttribute.Trailing, 1, 0));
            this.View.AddConstraint(NSLayoutConstraint.Create(ContainerView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, DismissAreaView, NSLayoutAttribute.Bottom, 1, 0));

            DismissAreaView.UserInteractionEnabled = true;

            DismissAreaView.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                DismissTapped();
            }));
        }

        private void SetUpPullBarView()
        {
            ContainerView.AddSubview(PullBarView);
            PullBarView.TranslatesAutoresizingMaskIntoConstraints = false;
            PullBarView.AddConstraint(NSLayoutConstraint.Create(PullBarView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 24));

            ContainerView.AddConstraint(NSLayoutConstraint.Create(ContainerView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, PullBarView, NSLayoutAttribute.Top, 1, 0));
            ContainerView.AddConstraint(NSLayoutConstraint.Create(ContainerView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, PullBarView, NSLayoutAttribute.Leading, 1, 0));
            ContainerView.AddConstraint(NSLayoutConstraint.Create(ContainerView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, PullBarView, NSLayoutAttribute.Trailing, 1, 0));

            var grabView = new UIView(CGRect.Empty);
            grabView.TranslatesAutoresizingMaskIntoConstraints = false;
            PullBarView.AddSubview(grabView);
            PullBarView.AddConstraint(NSLayoutConstraint.Create(grabView, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, PullBarView, NSLayoutAttribute.CenterX, 1, 0));
            PullBarView.AddConstraint(NSLayoutConstraint.Create(grabView, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, PullBarView, NSLayoutAttribute.CenterY, 1, 0));
            grabView.AddConstraint(NSLayoutConstraint.Create(grabView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 4));
            grabView.AddConstraint(NSLayoutConstraint.Create(grabView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1, 36));

            grabView.Layer.CornerRadius = 3;
            grabView.Layer.MasksToBounds = true;
            grabView.BackgroundColor = UIColor.FromWhiteAlpha(0.868f, 1);
        }

        void DismissTapped()
        {
            UIView.Animate(0.3, 0, UIViewAnimationOptions.CurveEaseInOut, () =>
            {
                this.View.BackgroundColor = UIColor.Clear;
            }, () => { this.DismissViewController(true, null); });
        }

        public void HandleScrollView(UIScrollView scrollView)
        {
            scrollView.PanGestureRecognizer.RequireGestureRecognizerToFail(_panGestureRecognizer);
            _childScrollView = scrollView;
        }

        private float HeightFor(SheetSize type)
        {
            if (type == SheetSize.HalfScreen)
                return (float)(UIScreen.MainScreen.Bounds.Height / 2 + 24);
            else if (type == SheetSize.FullScreen)
            {
                var insets = safeAreaInsets();
                return (float)(UIScreen.MainScreen.Bounds.Height - insets.Top - 20);
            }
            return type.Value;
        }


        void Panned(UIPanGestureRecognizer gesture)
        {
            var point = gesture.TranslationInView(gesture.View?.Superview);
            if (gesture.State == UIGestureRecognizerState.Began)
            {
                _firstPanPoint = point;
                _actualContainerSize = SheetSize.Fixed((float)ContainerView.Frame.Height);
            }

            var minHeight = Math.Min(HeightFor(_actualContainerSize), HeightFor(_orderedSheetSize.First()));
            var maxHeight = Math.Max(HeightFor(_actualContainerSize), HeightFor(_orderedSheetSize.Last()));
            var newHeight = Math.Max(0, HeightFor(_actualContainerSize) + _firstPanPoint.Y - point.Y);
            float offset = 0;

            if (newHeight < minHeight)
            {
                offset = minHeight - (float)newHeight;
                newHeight = minHeight;
            }
            if (newHeight > maxHeight)
                newHeight = maxHeight;

            if (gesture.State == UIGestureRecognizerState.Cancelled || gesture.State == UIGestureRecognizerState.Failed)
            {
                UIView.Animate(0.3, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                {
                    ContainerView.Transform = CGAffineTransform.MakeIdentity();
                    _containerHeightConstraint.Constant = HeightFor(_containerSize);
                }, null);
            }
            else if (gesture.State == UIGestureRecognizerState.Ended)
            {
                var velocity = (0.2 * gesture.VelocityInView(this.View).Y);
                var finalHeight = newHeight - offset - velocity;
                if (velocity > 500)
                    finalHeight = -1;

                var animationDuration = (Math.Abs(velocity * 0.0002) + 0.2);

                if (finalHeight <= (minHeight / 2))
                {
                    UIView.Animate(animationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                    {
                        this.View.BackgroundColor = UIColor.Clear;
                        ContainerView.Transform = CGAffineTransform.MakeTranslation(0, ContainerView.Frame.Height);
                    }, () => { this.DismissViewController(false, null); });
                    return;
                }

                var newSize = _containerSize;
                if (point.Y < 0)
                {
                    newSize = _orderedSheetSize.Last() ?? _containerSize;
                    foreach (var size in _orderedSheetSize.Reverse())
                    {
                        if (finalHeight < HeightFor(size))
                            newSize = size;
                        else
                            break;
                    }
                }
                else
                {
                    newSize = _orderedSheetSize.First() ?? _containerSize;
                    foreach (var size in _orderedSheetSize)
                    {
                        if (finalHeight > HeightFor(size))
                            newSize = size;
                        else
                            break;

                    }
                }
                _containerSize = newSize;

                UIView.Animate(animationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                {
                    ContainerView.Transform = CGAffineTransform.MakeIdentity();
                    _containerHeightConstraint.Constant = HeightFor(newSize);
                    this.View.LayoutIfNeeded();
                }, () => { _actualContainerSize = SheetSize.Fixed((float)ContainerView.Frame.Height); });
            }
            else
            {
                _containerHeightConstraint.Constant = (float)newHeight;
                if (offset > 0)
                    ContainerView.Transform = CGAffineTransform.MakeTranslation(0, offset);
                else
                    ContainerView.Transform = CGAffineTransform.MakeIdentity();
            }
        }

        [Export("gestureRecognizerShouldBegin:")]
        public bool ShouldBegin(UIGestureRecognizer recognizer)
        {
            var panGestureRecognizer = recognizer as InitialTouchPanGestureRecognizer;
            var childScrollView = _childScrollView;
            var point = panGestureRecognizer.initialTouchLocation;
            if (panGestureRecognizer == null || childScrollView == null || point == null)
            {
                return true;
            }

            var pointInChildScrollView = this.View.ConvertPointToView(point, _childScrollView).Y - _childScrollView.ContentOffset.Y;
            var velocity = panGestureRecognizer.VelocityInView(panGestureRecognizer.View.Superview);

            if (Math.Abs(velocity.Y) > Math.Abs(velocity.X) || childScrollView.ContentOffset.Y == 0)
                return false;

            if (velocity.Y < 0)
            {
                var containerHeight = HeightFor(_containerSize);
                return HeightFor(_orderedSheetSize.Last()) > containerHeight && containerHeight < HeightFor(SheetSize.FullScreen);
            }
            else
                return true;
        }
    }
}
