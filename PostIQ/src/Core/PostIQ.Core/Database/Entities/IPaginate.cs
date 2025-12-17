using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostIQ.Core.Database.Entities
{
    public interface IPaginate<T>
    {
        int From { get; }

        int Index { get; }

        int Size { get; }

        int Count { get; }

        int Pages { get; }

        IList<T> Data { get; }

        bool HasPrevious { get; }

        bool HasNext { get; }
    }
}
