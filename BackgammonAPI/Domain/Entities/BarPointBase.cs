using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgammonAPI.Domain.Entities
{
    public abstract class BarPointBase
    {
        public abstract List<CheckerPoint> GetGameObjects();
        public abstract void AddGameObject(CheckerPoint gameObject);
        public abstract void RemoveGameObject(CheckerPoint gameObject);

    }
}