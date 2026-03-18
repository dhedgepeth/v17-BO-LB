using StackExchange.Redis;

namespace LoadBalanceReDo.Web
{
	public static partial class IUmbracoBuilderExtensions
	{
		public static IUmbracoBuilder AddRedisCache(this IUmbracoBuilder builder)
		{
			builder.Services.AddStackExchangeRedisCache(options =>
			{
				var redisConnectionString = builder.Config.GetConnectionString("RedisCache");
				var config = ConfigurationOptions.Parse(redisConnectionString);
				config.Ssl = false;
				config.AbortOnConnectFail = false;

				options.ConfigurationOptions = config;
				options.InstanceName = "UmbracoSession_";
			});

			return builder;
		}
	}
}