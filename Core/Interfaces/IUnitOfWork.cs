namespace Core.Interfaces;

public interface IUnitOfWork
{
    public ILessorRepository LessorRepository { get; set; }
    public IRenewalRepository RenewalRepository  { get; set; }
    public IPaymentRepository PaymentRepository  { get; set; }
    public IInvoiceRepository InvoiceRepository  { get; set; }
 
    Task<bool> SaveAsync();
}