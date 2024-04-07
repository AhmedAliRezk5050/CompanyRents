namespace Core.Entities;

public class ParticipationRatio
{
    public int Id { get; set; }
    public int YearNumber { get; set; }
    public double Ratio { get; set; }

    public int LessorId { get; set; }
    public Lessor Lessor { get; set; } = null!;
}