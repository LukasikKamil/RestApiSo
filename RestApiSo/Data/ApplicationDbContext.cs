using Microsoft.EntityFrameworkCore;
using RestApiSo.Models;

namespace RestApiSo.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Tag> Tags { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        /// <summary>
        /// Configures the model for the <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/> to use.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the Tags entity.
            modelBuilder.Entity<Tag>(entity =>
            {
                // Configure the Percentage property to have a precision of 5 digits and 2 decimal places.
                entity.Property(t => t.Percentage)
                   .HasPrecision(5, 2);
            });

            // Call the base method to continue configuring the model.
            base.OnModelCreating(modelBuilder);
        }
    }
}
