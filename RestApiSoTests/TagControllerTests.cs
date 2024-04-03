using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestApiSoTests
{
    public class TagControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public TagControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        /// <summary>
        /// Tests that the GET request to the /api/tags endpoint returns a successful status code
        /// </summary>
        public async Task GetTags_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/tags");

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}
