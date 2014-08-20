using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System.IO.Compression;
using System.Threading.Tasks;
using System.IO;

namespace SharkFrontEnd
{
	public class WebRole : RoleEntryPoint
	{
		public override bool OnStart()
		{
			return base.OnStart();
		}
	}
}
