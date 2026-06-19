using System.Collections.Generic;
using System.Drawing;

namespace BackgammonAPI.Domain.Entities
{
    public class TrianglePoint
    {
        public List<CheckerPoint> checkersOnPoint = new List<CheckerPoint>();
        public int trianglePointIndex;
        public bool triangleIsUpward;
        public static TrianglePoint aiTrianglePoint;
       
        public void SetTriangle(TrianglePoint point)
        {
            aiTrianglePoint = point;
        }
    }
}