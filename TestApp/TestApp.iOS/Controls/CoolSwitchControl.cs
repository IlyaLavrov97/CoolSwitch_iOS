using System;
using CoreAnimation;
using CoreGraphics;
using CoreImage;
using Foundation;
using OpenTK;
using UIKit;

namespace TestApp.iOS.Controls
{
    public class CoolSwitchControl : UIView
    {
        private const float StrokeWidth = 3;

        private CGSize _switchSize;

        private CAShapeLayer _offSwitchMask;
        private CAShapeLayer _onSwitchMask;
        private CAShapeLayer _loadingSwitchMask;

        private CAGradientLayer _borderLayer; 
        private CAGradientLayer _switchLayer;
        private CAGradientLayer _offLayer;
        private CAGradientLayer _borderInsideLayer;

        public SwitchState CurrentSwitchState { get; set; }
        
        public CoolSwitchControl(CGRect frame) : base(frame)
        {
            InitializeMasks();
            InitializeLayers(frame);
        }

        private CAGradientLayer GetColoredLayerWithMask(CALayer mask)
        {
            return new CAGradientLayer
            {
                Colors = new[] {UIColor.Blue.CGColor, UIColor.Green.CGColor},
                Frame = new CGRect(0, 0, Frame.Width, Frame.Height),
                Mask = mask,
            };
        }

        private void InitializeMasks()
        {
            var switchRect = new CGRect(new CGPoint(Frame.Height / 4, Frame.Height / 4), _switchSize);
            var onSwitchPath = UIBezierPath.FromOval(switchRect);
            _onSwitchMask = InvertMask(onSwitchPath);
            
            switchRect.X = Frame.Width - _switchSize.Width - _switchSize.Width / 2;
            var offSwitchPath = UIBezierPath.FromOval(switchRect);
            _offSwitchMask = InvertMask(offSwitchPath);
            
            switchRect.X = (Frame.Width - _switchSize.Width) / 2;
            var loadingSwitchPath = UIBezierPath.FromOval(switchRect);
            _loadingSwitchMask = InvertMask(loadingSwitchPath);
        }

        private void InitializeLayers(CGRect frame)
        {
            Frame = new CGRect(Frame.X, Frame.Y, Frame.Width + 2 * StrokeWidth, Frame.Height + 2 * StrokeWidth);
            _switchSize = new CGSize(Frame.Height / 2, Frame.Height / 2);

            var mainPath = UIBezierPath.FromRoundedRect(new CGRect(0, 0, Frame.Width, Frame.Height), Frame.Height / 2);
            
            var mainMask = new CAShapeLayer
            {
                Path = mainPath.CGPath,
            };

            Layer.Mask = mainMask;
            Layer.MasksToBounds = true;
            
            //
            
            var path = UIBezierPath.FromRoundedRect(
                new CGRect(StrokeWidth / 2, StrokeWidth / 2, frame.Width + StrokeWidth, frame.Height + StrokeWidth),
                (frame.Height  + StrokeWidth) / 2);

            var borderMask = new CAShapeLayer
            {
                Path = path.CGPath,
                FillColor = UIColor.Clear.CGColor,
                LineWidth = StrokeWidth,
                StrokeColor = UIColor.Red.CGColor
            };

            _borderLayer = GetColoredLayerWithMask(borderMask);
            _borderLayer.MasksToBounds = true;
            
            Layer.AddSublayer(_borderLayer);
            
            //

            _borderInsideLayer = GetColoredLayerWithMask(_onSwitchMask);

            Layer.AddSublayer(_borderInsideLayer);
            
            var pathAnimation = CABasicAnimation.FromKeyPath(@"path");
            
            pathAnimation.Duration = 5;
            pathAnimation.SetFrom(_onSwitchMask.Path);
            pathAnimation.SetTo(_offSwitchMask.Path);
            
            pathAnimation.AnimationStopped += (sender, args) => _borderInsideLayer.Mask = _offSwitchMask;

            _borderInsideLayer.Mask.AddAnimation(pathAnimation, @"path");
            
            // 
            
            var powerImage = UIImage.FromBundle("power_filled.png");
            
            var switchMask = new CAShapeLayer
            {
                Contents = powerImage.CGImage,
                Frame = new CGRect(_borderLayer.Frame.Height / 4, _borderLayer.Frame.Height / 4, _borderLayer.Frame.Height / 2, _borderLayer.Frame.Height / 2)
            };

            _switchLayer = GetColoredLayerWithMask(switchMask);
            _switchLayer.CornerRadius = switchMask.Frame.Height / 2;
            _switchLayer.MasksToBounds = true;

            //Layer.AddSublayer(_switchLayer);
            
            //
            
            var offFrame = new NSString("Off").GetBoundingRect(new CGSize(frame.Width, frame.Height), 
                NSStringDrawingOptions.UsesLineFragmentOrigin,
                new UIStringAttributes { Font = UIFont.SystemFontOfSize(20) }, null);
            
            var offMask = new CATextLayer
            {
                String = "Off",
                FontSize = 20,
                ForegroundColor = UIColor.Black.CGColor,
                Frame = new CGRect(new CGPoint(
                        _borderLayer.Frame.Right - (offFrame.Width + _borderLayer.Frame.Width / 2) / 2 - 8,
                        (_borderLayer.Frame.Height - offFrame.Height) / 2),  offFrame.Size),
            };
            
            offMask.SetFont(UIFont.SystemFontOfSize(20).Name);

            _offLayer = GetColoredLayerWithMask(offMask);

            //Layer.AddSublayer(_offLayer);
            
            //
            
            var onMaskView = new InvertedLabel(new UIEdgeInsets(0, 25, 0,0))
            {
                Text = "On",
                Font = UIFont.SystemFontOfSize(20),
                Frame = new CGRect(CGPoint.Empty, Frame.Size),
            };

            var gradientView = new UIView(new CGRect(CGPoint.Empty, Frame.Size))
            {
                MaskView = onMaskView
            };

            gradientView.Layer.InsertSublayer(_borderInsideLayer, 0);
            
            AddSubview(gradientView);
        }

        private CAShapeLayer InvertMask(UIBezierPath path)
        {
            var invertedMask = new CAShapeLayer();
            
            var back = UIBezierPath.FromRect(new CGRect(0, 0, Frame.Width, Frame.Height));
            path.AppendPath(back);
            invertedMask.FillRule = CAShapeLayer.FillRuleEvenOdd;

            invertedMask.Path = path.CGPath;
            
            return invertedMask;
        }
    }

    public enum SwitchState : int
    {
        None = 0,
        On = 1,
        Custom = 2,
        Off = 3,
    }
}