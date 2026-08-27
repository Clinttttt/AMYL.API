namespace AMYL.Api.Shared.Domain
{
    public class AuditableEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime DeletedAt { get; set; }
        public string? DeletedBy { get; set; } 
        public bool IsDeleted { get; set; } 

        public void SoftDelete(string DeletedBy)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            this.DeletedBy = DeletedBy;
        }
    }
}
