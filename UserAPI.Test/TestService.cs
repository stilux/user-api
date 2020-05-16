using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace UserAPI.Test
{
    public class TestService : ITestService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TestService> _logger;

        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
            {PropertyNameCaseInsensitive = true};

        public TestService(IHttpClientFactory httpClientFactory, ILogger<TestService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task RunAsync(Uri serviceUrl, string host)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Host = host ?? serviceUrl.Host;
            client.Timeout = TimeSpan.FromSeconds(10);

            var userId = (await TestCreateUserAsync(client, serviceUrl))?.Id;
            if (!userId.HasValue) return;

            await TestGetUserAsync(client, serviceUrl, userId.Value);
            await TestUpdateUserAsync(client, serviceUrl, userId.Value);
            await TestDeleteUserAsync(client, serviceUrl, userId.Value);
        }

        private async Task<User> TestCreateUserAsync(HttpClient client, Uri serviceUrl)
        {
            var newUser = CreateRandomUser();

            using var content = new StringContent(
                JsonSerializer.Serialize(newUser), Encoding.UTF8, MediaTypeNames.Application.Json);

            var result = await client.PostAsync(serviceUrl, content);
            LogRequest(result, HttpStatusCode.Created);
            
            if (result.StatusCode == HttpStatusCode.Created)
            {
                var json = await result.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<User>(json, JsonSerializerOptions).AsTask();
            }
            return null;
        }

        private async Task<User> TestGetUserAsync(HttpClient client, Uri serviceUrl, int userId)
        {
            var result = await client.GetAsync($"{serviceUrl}/{userId}");
            
            LogRequest(result, HttpStatusCode.OK);
            
            var json = await result.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<User>(json, JsonSerializerOptions).AsTask(); 
        }

        private async Task TestUpdateUserAsync(HttpClient client, Uri serviceUrl, int userId)
        {
            var updatingUser = CreateRandomUser();
            updatingUser.Id = userId;

            using var content = new StringContent(
                JsonSerializer.Serialize(updatingUser), Encoding.UTF8, MediaTypeNames.Application.Json);

            var result = await client.PutAsync($"{serviceUrl}/{userId}", content);
            LogRequest(result, HttpStatusCode.NoContent);
        }

        private async Task TestDeleteUserAsync(HttpClient client, Uri serviceUrl, int userId)
        {
            var result = await client.DeleteAsync($"{serviceUrl}/{userId}");
            LogRequest(result, HttpStatusCode.NoContent);
        }

        private void LogRequest(HttpResponseMessage response, HttpStatusCode expected)
        {
            if (response.StatusCode != expected)
            {
                _logger.LogError(
                    $"Request {response.RequestMessage.Method} {response.RequestMessage.RequestUri} returned {response.StatusCode}, but expected {expected}");
            }
        }

        private static User CreateRandomUser()
        {
            return new User
            {
                UserName = Faker.Internet.UserName(),
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                Email = Faker.Internet.Email(),
                Phone = Faker.Phone.Number()
            };
        }
    }
}