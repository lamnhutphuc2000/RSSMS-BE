using System.Collections.Generic;

namespace _3DBinPacking.Model
{
    public class BinPackResult
    {
        public IList<IList<Cuboid>> BestResult { get; private set; }

        public BinPackResult(IList<IList<Cuboid>> bestResult)
        {
            BestResult = bestResult;
        }
    }
}
