using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Transactions;

public enum GraphDbTransactionStatus
{
	Active,
	Committed,
	RolledBack,
	Failed
}
