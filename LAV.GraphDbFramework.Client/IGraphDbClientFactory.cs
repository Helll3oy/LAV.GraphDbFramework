using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAV.GraphDbFramework.Core;
using Microsoft.Extensions.Options;

namespace LAV.GraphDbFramework.Client;

public interface IGraphDbClientFactory
{
	IGraphDbClient Create();
}
