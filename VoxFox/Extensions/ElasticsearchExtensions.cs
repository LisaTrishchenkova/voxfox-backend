using System.Text;
using Elastic.Clients.Elasticsearch;

namespace VoxFox.Extensions;

public static class ElasticsearchExtensions
{
	public static IServiceCollection AddElasticsearch(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var uri = configuration["Elasticsearch:Uri"]!;
		var indexName = configuration["Elasticsearch:IndexName"]!;

		var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
			.EnableDebugMode()
			.OnRequestCompleted(details =>
			{
				if (details.RequestBodyInBytes != null)
					Console.WriteLine($"Request: {Encoding.UTF8.GetString(details.RequestBodyInBytes)}");
			});

		services.AddSingleton(new ElasticsearchClient(settings));
		return services;
	}
}
