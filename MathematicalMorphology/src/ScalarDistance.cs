
namespace MathematicalMorphology.src
{
    public class ScalarDistance
    {
        public ScalarDistance(double xDistance, double yDistance)
        {
            this.XDistance = xDistance;
            this.YDistance = yDistance;
        }

        public double XDistance { get; private set; }
        public double YDistance { get; private set; }
    }
}
