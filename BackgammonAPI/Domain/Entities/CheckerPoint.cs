using System.Drawing;
using static BackgammonAPI.Domain.ValueObjects.Constants;


namespace BackgammonAPI.Domain.Entities
{

    public class CheckerPoint
    {
        public int checkerPointIndex; // Set this when you instantiate the checker
        public bool checkerIsUpward;
        public TrianglePoint parentTriangle;
        public CheckerColor color;
        public static CheckerPoint aiCheckerPoint;
        public string tagName;

        public void SetChecker(CheckerPoint point)
        {
            aiCheckerPoint = point;
        }
    }

}