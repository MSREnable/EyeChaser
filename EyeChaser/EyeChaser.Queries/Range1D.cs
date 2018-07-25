using System;

namespace EyeChaser.Queries
{
    public class Range1D
    {
        public Range1D(double lowerBound, double upperBound)
        {
            if (!(lowerBound <= upperBound))
            {
                throw new Exception();
            }

            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        public double LowerBound { get; }

        public double UpperBound { get; }

        public double BoundSize => UpperBound - LowerBound;

        public override string ToString()
        {
            return $"{LowerBound}..{UpperBound}/{BoundSize}";
        }
    }
}
