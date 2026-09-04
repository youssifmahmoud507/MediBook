namespace MediBook.Application.DTOs.Specialities
{
    public class SpecialityResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
