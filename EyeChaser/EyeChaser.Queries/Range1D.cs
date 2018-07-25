namespace EyeChaser.Queries
{
    public class Range1D
    {
        public Range1D(double lowerBound, double upperBound)
        {
            Item1 = lowerBound;
            Item2 = upperBound;
        }

        public double Item1 { get; set; }

        public double Item2 { get; set; }
    }
}
