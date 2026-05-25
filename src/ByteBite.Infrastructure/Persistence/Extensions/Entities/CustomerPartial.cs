using System.ComponentModel.DataAnnotations.Schema;

namespace ByteBite.Infrastructure.Persistence.Entities;

public partial class Customer
{
    [NotMapped]
    public string? Token { get; set; }
}
