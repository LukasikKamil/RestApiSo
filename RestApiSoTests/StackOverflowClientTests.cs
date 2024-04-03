using Moq;
using Moq.Protected;
using RestApiSo.Services;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RestApiSoTests
{
    public class StackOverflowClientTests
    {
        [Fact]
        public async Task GetTags_ReturnsListOfTags()
        {
            // Arrange
            // Create a mock HttpMessageHandler
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            // Setup a protected method of HttpMessageHandler to send a HTTP response asynchronously
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
               .ReturnsAsync(() =>
               {
                   // Create a JSON string that represents a list of tags
                   var content = @"{""items"":[{""name"":""tag1""},{""name"":""tag2""}],""has_more"":false}";

                   // Convert the JSON string to a byte array
                   var byteArray = Encoding.ASCII.GetBytes(content);

                   // Create a memory stream and write the byte array to it using a GZipStream for compression
                   var stream = new MemoryStream();
                   using(var gzipStream = new GZipStream(stream, CompressionMode.Compress, true))
                   {
                       gzipStream.Write(byteArray, 0, byteArray.Length);
                   }

                   // Reset the position of the memory stream
                   stream.Position = 0;

                   // Return a HTTP response message with the memory stream as its content
                   return new HttpResponseMessage()
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StreamContent(stream),
                   };
               })
               .Verifiable();

            // Create a HttpClient with the mock HttpMessageHandler
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.stackexchange.com/2.2/"),
            };

            // Create a StackOverflowClient with the HttpClient
            var stackOverflowClient = new StackOverflowClient(httpClient);

            // Act
            // Call the GetTags method of the StackOverflowClient and get the result
            var result = await stackOverflowClient.GetTags();

            // Assert
            // Check if the count of the result is 2
            Assert.Equal(2, result.Count);

            // Check if the name of the first tag in the result is "tag1"
            Assert.Equal("tag1", result[0].Name);

            // Check if the name of the second tag in the result is "tag2"
            Assert.Equal("tag2", result[1].Name);
        }


        [Fact]
        public async Task GetTags_ReturnsEmptyList_WhenApiReturnsNoTags()
        {
            // Arrange
            // Create a mock HttpMessageHandler
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            // Setup a protected method of HttpMessageHandler to send a HTTP response asynchronously
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(() =>
               {
                   // Create a JSON string that represents an empty list of tags
                   var content = "{\"items\":[],\"has_more\":false}";

                   // Convert the JSON string to a byte array
                   var byteArray = Encoding.ASCII.GetBytes(content);

                   // Create a memory stream and write the byte array to it using a GZipStream for compression
                   var stream = new MemoryStream();
                   using(var gzipStream = new GZipStream(stream, CompressionMode.Compress, true))
                   {
                       gzipStream.Write(byteArray, 0, byteArray.Length);
                   }

                   // Reset the position of the memory stream
                   stream.Position = 0;

                   // Return a HTTP response message with the memory stream as its content
                   return new HttpResponseMessage()
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StreamContent(stream),
                   };
               })
               .Verifiable();

            // Create a HttpClient with the mock HttpMessageHandler
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.stackexchange.com/2.2/"),
            };

            // Create a StackOverflowClient with the HttpClient
            var stackOverflowClient = new StackOverflowClient(httpClient);

            // Act
            // Call the GetTags method of the StackOverflowClient and get the result
            var result = await stackOverflowClient.GetTags();

            // Assert
            // Check if the result is an empty list
            Assert.Empty(result);
        }


        [Fact]
        public async Task GetTags_ThrowsException_WhenApiReturnsError()
        {
            // Arrange
            // Create a mock HttpMessageHandler
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            // Setup a protected method of HttpMessageHandler to send a HTTP response asynchronously
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.InternalServerError,
               })
               .Verifiable();

            // Create a HttpClient with the mock HttpMessageHandler
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.stackexchange.com/2.2/"),
            };

            // Create a StackOverflowClient with the HttpClient
            var stackOverflowClient = new StackOverflowClient(httpClient);

            // Act & Assert
            // Call the GetTags method of the StackOverflowClient and assert that it throws a HttpRequestException
            await Assert.ThrowsAsync<HttpRequestException>(() => stackOverflowClient.GetTags());
        }

    }
}
