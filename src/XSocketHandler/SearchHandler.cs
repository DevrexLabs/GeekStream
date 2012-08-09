using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Text;
using GeekStream.Core.Domain;
using GeekStream.Core.Queries;
using LiveDomain.Core;
using LiveDomain.Enterprise;
using XSockets.Core.Globals;
using XSockets.Core.XSocket;
using XSockets.Core.XSocket.Event.Attributes;
using XSockets.Core.XSocket.Event.Interface;
using XSockets.Core.XSocket.Interface;
using XSockets.Core.XSocket.Helpers;

namespace XSocketHandler
{
	//Export the IXBaseSocket interface so that the server can fins the plugin
	[Export(typeof(IXBaseSocket))]
	//MetaData for knowing the unique alias and the buffersize
	[XBaseSocketMetadata("SearchController", Constants.GenericTextBufferSize)]
	public class SearchHandler : XBaseSocket
	{
		static LiveDbConnectionSettings _liveDbConnectionSettings;

		public static ITransactionHandler<GeekStreamModel> Db
		{
			get
			{
				if (_liveDbConnectionSettings == null)
				{
					string liveDbConnectionString = ConfigurationManager.ConnectionStrings["geekstream"].ConnectionString;
					_liveDbConnectionSettings = LiveDbConnectionSettings.Parse(liveDbConnectionString);

				}
				return _liveDbConnectionSettings.GetClient<GeekStreamModel>();
			}
		}


		#region Overrides of XBaseSocket

		public override IXBaseSocket NewInstance()
		{
			return new SearchHandler();
		}

		#endregion

		[HandlerEvent("SearchQuery")]
		public void Search(SearchRequest search)
		{
			if(search.Query == null || search.Query.Length < 3) return;

			var query = new SearchQuery(search.Query);
			var result = Db.Execute(query);

			XNode.XSocket.Send(result, "OnSearchCompleted");
		}
	}

	public class SearchRequest
	{
		public string Query { get; set; }
	}

}
