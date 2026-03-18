using Umbraco.Cms.Core.Composing;

namespace LoadBalanceReDo.Web
{
	public class SignalRComposer : IComposer
	{
		public void Compose(IUmbracoBuilder builder) =>
			builder.Services.AddSignalR().AddAzureSignalR();
	}
}
