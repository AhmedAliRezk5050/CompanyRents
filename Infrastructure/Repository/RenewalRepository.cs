using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Repository;

public class RenewalRepository : BaseRepository<Renewal>, IRenewalRepository
{
    public RenewalRepository(AppDbContext context) : base(context)
    {
    }
}