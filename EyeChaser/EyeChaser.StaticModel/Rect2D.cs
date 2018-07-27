using System;

namespace EyeChaser.StaticModel
{
    public sealed class Rect2D
    {
        public double Left { get; }
        public double Right { get; }
        public double Top { get; }
        public double Bottom { get; }

        public Rect2D(double left, double right, double top, double bottom)
        {
            if (right < left || bottom < top)
            {
                throw new ArgumentException(String.Format("Left {0} < Right {1}, Top {2} < Bottom {3} ??", left, right, top, bottom));
            }
            this.Left = left; this.Right = right; this.Top = top; this.Bottom = bottom;
        }

        public double Width => (Right - Left);
        public double Height => (Bottom - Top);
    }
}
