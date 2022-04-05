using System;
using System.Collections.Generic;
using System.Text;

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
