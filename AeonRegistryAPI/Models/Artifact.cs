namespace AeonRegistryAPI.Models
{
    public class Artifact
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string? Name { get; set; }

        [Required, MaxLength(500)]
        public string? CatalogNumber { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; } //Aeon Narrative

        [MaxLength(2000)]
        public string? PublicNarrative { get; set; } // Public Narrative

        public DateTime DateDiscovered { get; set; }

        public string? Type { get; set; } //artifact type enum as string

        [Required]

        public int SiteId { get; set; }
        public Site? Site { get; set; }

        //navigation properties
        public List<ArtifactMediaFile> MediaFiles { get; set; } = [];

        public List<CatalogRecord> CatalogRecords { get; set; } = [];
    }

}