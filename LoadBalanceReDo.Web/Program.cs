using LoadBalanceReDo.Web;
using LoadBalanceReDo.Web.ServerRoleAccessors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Umbraco.Cms.Infrastructure.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var umbracoBuilder = builder.CreateUmbracoBuilder()
	.AddBackOffice()
	.AddWebsite()
	.AddDeliveryApi()
	.AddComposers();

if (builder.Environment.EnvironmentName.Equals("Subscriber"))
{
	umbracoBuilder.SetServerRegistrar<SchedulingPublisherServerRoleAccessor>()
		.LoadBalanceIsolatedCaches()
		.AddAzureBlobMediaFileSystem()
		.AddAzureBlobImageSharpCache()
		.AddRedisCache();
}
else if (builder.Environment.IsProduction())
{
	umbracoBuilder.SetServerRegistrar<SchedulingPublisherServerRoleAccessor>()
		.LoadBalanceIsolatedCaches();
}
else if (builder.Environment.EnvironmentName.Equals("Local"))
{
	umbracoBuilder.SetServerRegistrar<SingleServerRoleAccessor>();
}
else
{
	umbracoBuilder.SetServerRegistrar<SingleServerRoleAccessor>()
		.AddRedisCache();
}

var tempFileBlobConnectionString = builder.Configuration["TEMP_FILE_BLOB_CONNECTION_STRING"];
var tempFileBlobContainerName = builder.Configuration["TEMP_FILE_BLOB_CONTAINER_NAME"] ?? "umbraco-temp-uploads";
if (!string.IsNullOrEmpty(tempFileBlobConnectionString)
	&& string.IsNullOrEmpty(builder.Configuration["Umbraco:Storage:AzureBlob:TemporaryFile:ConnectionString"]))
{
	builder.Configuration["Umbraco:Storage:AzureBlob:TemporaryFile:ConnectionString"] = tempFileBlobConnectionString;
	builder.Configuration["Umbraco:Storage:AzureBlob:TemporaryFile:ContainerName"] = tempFileBlobContainerName;
}

var dpConnectionString = builder.Configuration["Umbraco:Storage:AzureBlob:Media:ConnectionString"];
if (!string.IsNullOrEmpty(dpConnectionString))
{
	builder.Services.AddDataProtection()
		.SetApplicationName("LoadBalanceReDo")
		.PersistKeysToAzureBlobStorage(dpConnectionString, "data-protection", "keys.xml");
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
	options.KnownNetworks.Clear();
	options.KnownProxies.Clear();
});

umbracoBuilder.Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseUmbraco()
	.WithMiddleware(u =>
	{
		u.UseBackOffice();
		u.UseWebsite();
	})
	.WithEndpoints(u =>
	{
		u.UseBackOfficeEndpoints();
		u.UseWebsiteEndpoints();
	});

await app.RunAsync();