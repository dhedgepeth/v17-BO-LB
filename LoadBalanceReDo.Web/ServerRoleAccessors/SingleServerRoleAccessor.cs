using Umbraco.Cms.Core.Sync;

namespace LoadBalanceReDo.Web.ServerRoleAccessors
{
	public sealed class SingleServerRoleAccessor : IServerRoleAccessor
	{
		public ServerRole CurrentServerRole => ServerRole.Single;
	}
}
