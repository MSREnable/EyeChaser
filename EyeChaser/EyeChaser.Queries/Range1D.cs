namespace EyeChaser.Queries
{
    public class Range1D
    {
        public Range1D(double lowerBound, double upperBound)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        public double LowerBound { get; }

        public double UpperBound { get; }
    }
}
