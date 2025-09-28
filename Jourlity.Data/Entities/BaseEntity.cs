using System.ComponentModel.DataAnnotations.Schema;

namespace Jourlity.Data.Entities;

public class BaseEntity
{
    public Guid Id { get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public DateTime CreatedAt { get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public DateTime UpdatedAt { get; set; }
}