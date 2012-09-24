using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeekStream.Core.Domain;
using LiveDomain.Core;

namespace GeekStream.Core.Commands
{
	public class RemoveSearchkeyCommand : Command<GeekStreamModel>
	{
		string _key;

		public RemoveSearchkeyCommand(string key)
		{
			_key = key;
		}

		#region Overrides of Command<GeekStreamModel>

		protected override void Execute(GeekStreamModel model)
		{
			
		}

		#endregion
	}
}
