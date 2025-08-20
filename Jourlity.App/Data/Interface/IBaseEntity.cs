namespace Jourlity.App.Data.Interface;

public interface IBaseEntity
{
    public Guid Id { get; set; }

    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }

    public DateTime Deleted { get; set; }
}