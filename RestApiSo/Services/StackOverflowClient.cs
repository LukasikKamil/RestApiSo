using RestApiSo.Models;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestApiSo.Services;

public class StackOverflowClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StackOverflowClient> _logger;

    public StackOverflowClient(ILogger<StackOverflowClient> logger)
    {
        // Initialize a new HttpClient
        _httpClient = new HttpClient
        {
            // Set the base address of the HttpClient to the Stack Overflow API
            BaseAddress = new Uri("https://api.stackexchange.com/2.2/")
        };
        _logger = logger;
    }


    public StackOverflowClient(HttpClient httpClient, ILogger<StackOverflowClient> logger )
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Tag>> GetTags()
    {
        // Initialize a list to store the tags and a variable to keep track of the page number
        var tags = new List<Tag>();
        var page = 1;

        // Keep fetching tags from the API until we have 1000 tags
        while(tags.Count < 1000)
        {
            try
            {

                // Send a GET request to the Stack Overflow API to fetch the tags
                var response = await _httpClient.GetAsync($"tags?page={page}&pagesize=100&order=desc&sort=popular&site=stackoverflow");

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response stream
                using var responseStream = await response.Content.ReadAsStreamAsync();

                // Decompress the response stream
                using var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress);

                // Read the decompressed stream
                using var streamReader = new StreamReader(decompressedStream);
                var content = await streamReader.ReadToEndAsync();

                // Deserialize the JSON content into a StackOverflowResponse object
                var data = JsonSerializer.Deserialize<StackOverflowResponse>(content);

                // If the response contains any tags, add them to our list
                if(data?.Items != null)
                {
                    tags.AddRange(data.Items);
                }

                // If the response indicates there are no more tags, break the loop
                if(data?.HasMore == false)
                {
                    break;
                }

                // Increment the page number for the next request
                page++;

                _logger.LogInformation("Fetching tags from page {PageNumber}", page);
            }
            catch(HttpRequestException e)
            {
                // Log network-related exceptions
                _logger.LogError(e, "An error occurred connecting to Stack Overflow API");
            }
            catch(JsonException e)
            {
                // Log JSON serialization/deserialization exceptions
                _logger.LogError(e, "An error occurred while deserializing the response");
            }
            catch(Exception e)
            {
                // Log any other exceptions that were not accounted for
                _logger.LogError(e, "An unexpected error occurred");
            }
        }

        // Return the list of tags
        return tags;
    }
}