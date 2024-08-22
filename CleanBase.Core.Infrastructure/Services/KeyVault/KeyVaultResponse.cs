using CleanBase.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.Services.KeyVault
{
	public class KeyVaultResponse
	{
		public string AppId { get; set; }
		public AppSettings config { get; set; }
	}
}
