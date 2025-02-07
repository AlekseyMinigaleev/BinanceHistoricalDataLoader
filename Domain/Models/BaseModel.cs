namespace Domain.Models
{
    public abstract class BaseModel
    {
        protected BaseModel()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
    }
}