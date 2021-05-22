namespace EOrm.Interfaces
{
    public interface IEntity
    {
        int Id { get; }
        string GetCreateCommand();
        string GetDeleteCommand();
        string GetUpdateCommand();
    }
}
