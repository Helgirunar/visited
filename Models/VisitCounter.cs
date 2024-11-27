namespace backend.Models
{
    public class VisitCounter
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int Count { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
    }
}
