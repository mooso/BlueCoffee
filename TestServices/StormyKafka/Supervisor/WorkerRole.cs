using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Experimental.Azure.Storm;

namespace Supervisor
{
	public class WorkerRole : StormNodeBase
	{
		protected override StormNodeType NodeType
		{
			get { return StormNodeType.SupervisorWithDrpc; }
		}
	}
}
