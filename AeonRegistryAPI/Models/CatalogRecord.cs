namespace AeonRegistryAPI.Models
{
    public class CatalogRecord
    {
        public int Id { get; set; }

        [Required]
        public int ArtifactId { get; set; }

        public Artifact? Artifact { get; set; } = null!;

        [Required]
        public string SubmittedById { get; set; } = string.Empty; //fk to ApplicationUser

        public ApplicationUser SubmittedBy { get; set; } = null!;

        public string? VerifiedById { get; set; } //fk to ApplicationUser

        public ApplicationUser? VerifiedBy { get; set; } = null!;

        [Required]
        public string Status { get; set; } = Enums.CatalogStatus.Draft.ToString();

        [Required]
        public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;

        public ICollection<CatalogNote> Notes { get; set; } = [];


    }
}