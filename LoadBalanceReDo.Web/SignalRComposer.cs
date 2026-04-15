using Umbraco.Cms.Core.Composing;

namespace LoadBalanceReDo.Web
{
	public class SignalRComposer : IComposer
	{
		public void Compose(IUmbracoBuilder builder)
		{
			var connectionString = builder.Config["Azure:SignalR:ConnectionString"];
			if (!string.IsNullOrEmpty(connectionString))
			{
				builder.Services.AddSignalR().AddAzureSignalR();
			}
			else
			{
				builder.Services.AddSignalR();
			}
		}
	}
}
