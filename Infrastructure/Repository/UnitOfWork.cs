using Core.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    
    public ILessorRepository LessorRepository { get; set; }
    public IRenewalRepository RenewalRepository { get; set; }
    public IPaymentRepository PaymentRepository { get; set; }
    public IInvoiceRepository InvoiceRepository { get; set; }
    
    
    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        LessorRepository = new LessorRepository(dbContext);
        RenewalRepository = new RenewalRepository(dbContext);
        PaymentRepository = new PaymentRepository(dbContext);
        InvoiceRepository = new InvoiceRepository(dbContext);
    }
    
    public async Task<bool> SaveAsync()
    {
        return await _dbContext.SaveChangesAsync() > 0;
    }
}