using CoreGraphics;
using UIKit;

namespace TestApp.iOS.Controls
{
    public class InvertedLabel : UILabel
    {
        private UIEdgeInsets _insets;
        
        
        public InvertedLabel(UIEdgeInsets insets) : base()
        {
            _insets = insets;
        }
        
        public override void DrawText(CGRect rect)
        {
            base.DrawText(_insets.InsetRect(rect));
        }

        public override void Draw(CGRect rect)
        {
            var gc = UIGraphics.GetCurrentContext();
            gc.SaveState();
            UIColor.White.SetFill();
            UIGraphics.RectFill(rect);
            gc.SetBlendMode(CGBlendMode.Clear);
            base.Draw(rect);
            gc.RestoreState();
        }
    }
}