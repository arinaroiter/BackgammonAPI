using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgammonAPI.Domain.Entities
{
    public class BarPointWhite : BarPointBase
    {
        private static List<CheckerPoint> checkersOnBarPoint = new List<CheckerPoint>();
        public override List<CheckerPoint> GetGameObjects()
        {
            return checkersOnBarPoint;
        }

        public override void AddGameObject(CheckerPoint gameObject)
        {
            checkersOnBarPoint.Add(gameObject);
        }

        public override void RemoveGameObject(CheckerPoint gameObject)
        {
            checkersOnBarPoint.Remove(gameObject);

        }
    }
}