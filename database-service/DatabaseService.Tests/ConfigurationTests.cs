using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DatabaseService.Tests
{
    public class StartupTests
    {
        [Fact]
        public void TestElasticSearchUriIsConfigured()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                [
                new KeyValuePair<string, string>("ConnectionStrings:ElasticSearchUri", "http://localhost:9200")
                ])
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var elasticUri = new Uri(serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("ElasticSearchUri"));

            // Assert
            Xunit.Assert.NotNull(elasticUri);
            Xunit.Assert.Equal("http://localhost:9200/", elasticUri.ToString());
        }

        [Fact]
        public void TestRabbitMqUriIsConfigured()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                [
                new KeyValuePair<string, string>("ConnectionStrings:RabbitMqUri", "amqp://localhost"),
                new KeyValuePair<string, string>("ConnectionStrings:RabbitMqQueueName", "testQueue")
                ])
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var rabbitUri = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("RabbitMqUri");
            var queueName = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("RabbitMqQueueName");

            // Assert
            Xunit.Assert.NotNull(rabbitUri);
            Xunit.Assert.Equal("amqp://localhost", rabbitUri);
            Xunit.Assert.NotNull(queueName);
            Xunit.Assert.Equal("testQueue", queueName);
        }   

    }
}