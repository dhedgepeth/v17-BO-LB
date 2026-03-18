using Umbraco.Cms.Core.Sync;

namespace LoadBalanceReDo.Web.ServerRoleAccessors
{
	public sealed class SchedulingPublisherServerRoleAccessor : IServerRoleAccessor
	{
		public ServerRole CurrentServerRole => ServerRole.SchedulingPublisher;
	}
}
