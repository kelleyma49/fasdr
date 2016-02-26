using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fasdr.Backend
{
    public interface IDatabase
    {
		Dictionary<string,Provider>  Providers { get; }
    }
}
