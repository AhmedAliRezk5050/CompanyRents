using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Repository;

public class LessorRepository : BaseRepository<Lessor>, ILessorRepository
{
    public LessorRepository(AppDbContext context) : base(context)
    {
    }
}