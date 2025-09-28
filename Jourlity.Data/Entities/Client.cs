namespace Jourlity.Data.Entities
{
    public class Client : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string DbPath { get; set; }
    }
}