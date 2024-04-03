using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestApiSo.Data;
using RestApiSo.Models;
using RestApiSo.Services;

namespace RestApiSo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly StackOverflowClient _client;
        private readonly ILogger<TagController> _logger;

        public TagController(ApplicationDbContext db, StackOverflowClient client, ILogger<TagController> logger)
        {
            _db = db;
            _client = client;
            _logger = logger;
        }


        /// <summary>
        /// Gets all tags from the Stack Overflow API and compares them to the local database. If a tag does not exist in the database, it is added. The percentage property of each tag is calculated based on the number of questions that use the tag. The tags are then sorted based on the percentage property.
        /// </summary>
        /// <returns>A list of tags, sorted by percentage.</returns>
        [HttpGet]
        public async Task<IEnumerable<Tag>> Get()
        {
            IEnumerable<Tag>? tags = null;

            try
            {

                tags = await _client.GetTags();

                // Loop through each tag from the Stack Overflow API
                foreach(var tag in tags)
                {
                    // Check if the tag already exists in the database
                    var existingTag = await _db.Tags.FirstOrDefaultAsync(t => t.Name == tag.Name);

                    // If the tag does not exist, add it
                    if(existingTag == null)
                    {
                        _db.Tags.Add(tag);
                    }
                }

                // Save the changes to the database
                await _db.SaveChangesAsync();

                // Calculate the total count of questions that use each tag
                var totalCount = _db.Tags.Sum(t => t.Count);

                // Loop through each tag again and calculate the percentage
                foreach(var tag in _db.Tags)
                {
                    tag.Percentage = Math.Round((decimal) tag.Count / totalCount * 100, 2);
                }

                // Save the changes to the database
                await _db.SaveChangesAsync();

                _logger.LogInformation("Tags has been updated successfully.");
            }
            catch (Exception e) 
            {
                _logger.LogError(e, "An error occured while updating tags.");
            }
            // Return the sorted list of tags
            return tags?.OrderBy(t => t.Name)?? Enumerable.Empty<Tag>();
        }



        /// <summary>
        /// Gets a page of tags, sorted by the specified sort criteria.
        /// </summary>
        /// <param name="pageNumber">The page number, starting from 1.</param>
        /// <param name="sortField">The field to sort by. Can be "name" or "percentage".</param>
        /// <param name="sortOrder">The sort order. Can be "asc" or "desc".</param>
        /// <returns>A page of tags, sorted by the specified criteria.</returns>
        [HttpGet("page/{pageNumber}")]
        public async Task<IEnumerable<Tag>> GetPage(int pageNumber, string sortField, string sortOrder)
        {
            int pageSize = 50;
            IEnumerable<Tag> pagedTags = Enumerable.Empty<Tag>();

            try
            {
                var tags = _db.Tags.AsQueryable();

                // Apply sorting based on the provided sortField and sortOrder
                switch(sortField)
                {
                    case "name":
                    tags = sortOrder == "asc" ? tags.OrderBy(t => t.Name) : tags.OrderByDescending(t => t.Name);
                    break;
                    case "percentage":
                    tags = sortOrder == "asc" ? tags.OrderBy(t => t.Percentage) : tags.OrderByDescending(t => t.Percentage);
                    break;
                    default:
                    tags = tags.OrderBy(t => t.Name); // Default sorting by name
                    break;
                }

                // Retrieve the requested page of tags
                pagedTags = await tags.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

                _logger.LogInformation("Retrieved page {PageNumber} of tags sorted by {SortField} in {SortOrder} order.", pageNumber, sortField, sortOrder);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "An error occurred while retrieving page {PageNumber} of tags.", pageNumber);
                // Depending on your error handling policy, you might want to handle the exception differently
                // For example, you could return a custom error response or rethrow the exception
            }

            return pagedTags;
        }




        [HttpPost("refresh")]
        /// <summary>
        /// Refreshes the list of tags by re-fetching them from the Stack Overflow API and recalculating the percentages.
        /// </summary>
        /// <returns>A list of tags.</returns>
        public async Task<IEnumerable<Tag>> Refresh()
        {
            IEnumerable<Tag>? tags = null;

            try
            {
                tags = await _client.GetTags();

                // Remove all existing tags
                _db.Tags.RemoveRange(_db.Tags);
                await _db.SaveChangesAsync(); // Save immediately after removing to avoid conflicts

                // Add the new tags
                foreach(var tag in tags)
                {
                    _db.Tags.Add(tag);
                }

                // Save the changes to the database
                await _db.SaveChangesAsync();

                // Calculate the total count of questions that use each tag
                var totalCount = _db.Tags.Sum(t => t.Count);

                // Loop through each tag again and calculate the percentage
                foreach(var tag in _db.Tags)
                {
                    tag.Percentage = Math.Round((decimal) tag.Count / totalCount * 100, 2);
                }

                // Save the changes to the database
                await _db.SaveChangesAsync();

                _logger.LogInformation("Tags have been refreshed successfully.");
            }
            catch(Exception e)
            {
                _logger.LogError(e, "An error occurred while refreshing tags.");
                // Depending on your error handling policy, you might want to handle the exception differently
                // For example, you could return a custom error response or rethrow the exception
            }

            // Assuming you want to return the tags even if there was an error during the refresh process
            // If not, you can move this inside the try block and handle accordingly
            return tags?.OrderBy(t => t.Name) ?? Enumerable.Empty<Tag>();
        }

    }
}