using System.Data;
using System.Linq.Expressions;
using AspNetCore.Reporting;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CompanyRents.Models;
using Serilog;

namespace CompanyRents.Controllers;

public class PaymentsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly UserManager<AppUser> _userManager;

    public PaymentsController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment,
        UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironment = webHostEnvironment;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Permissions.Payments.View)]
    public async Task<IActionResult> Index()
    {
        try
        {
            var paymentDetailsViewModels = (await _unitOfWork
                .PaymentRepository
                .GetAllAsync(include: x
                    => x.Include(e => e.User)
                        .Include(t => t.Invoice)
                )).Select(e => new PaymentDetailsViewModel()
            {
                Id = e.Id,
                Date = e.Date,
                PaymentType = e.PaymentType,
                PaidAmount = e.PaidAmount,
                RemainingAmount = e.RemainingAmount,
                InvoiceNumber = e.Invoice.Number,
                UserName = e.User.UserName,
                BankAccountNumber = e.BankAccountNumber,
                TotalInvoiceAmount = e.Invoice.Amount,
                Notes = e.Notes,
                InvoiceTotalPaidAmount = e.Invoice.Amount - e.RemainingAmount
            }).ToList();

            ViewBag.InvoicesSelectList = await GetInvoiceSelectList();
            
            return View(paymentDetailsViewModels);
        }
        catch (Exception e)
        {
            Log.Error("PaymentsController - Index[GET] - {@message}", e.Message);
            TempData["home-temp-msg"] = "unknown-error";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    [Authorize(Permissions.Payments.Add)]
    public async Task<IActionResult> Create()
    {
        try
        {
            var paymentViewModel = new PaymentViewModel();

            paymentViewModel.InvoiceSelectList = await GetFilteredInvoiceSelectList();

            return View(paymentViewModel);
        }
        catch (Exception e)
        {
            TempData["payments-index-temp-msg"] = "unknown-error";
            Log.Error("PaymentsController - Create[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [Authorize(Permissions.Payments.Add)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentViewModel model)
    {
        try
        {
            ModelState.Remove(nameof(model.InvoiceSelectList));

            if (!ModelState.IsValid)
            {
                model.InvoiceSelectList = await GetFilteredInvoiceSelectList();
                return View(model);
            }

            
            var invoice = (await _unitOfWork.InvoiceRepository
                .GetFirstOrDefaultAsync(
                    x => x.Id == model.InvoiceId,
                    include: e => e.Include(t => t.Renewal)
                        .ThenInclude(r => r.Invoices) // Include Invoices for Renewal
                        .Include(w => w.Lessor)
                        .ThenInclude(l => l.Invoices) // Include Invoices for Lessor
                ))!;


            var payment = new Payment();

            payment.PaidAmount = model.PaidAmount.Value;
            payment.RemainingAmount = model.RemainingInvoiceAmount - model.PaidAmount.Value;
            payment.Date = model.Date.Value;
            payment.PaymentType = model.PaymentType;
            payment.BankAccountNumber = model.BankAccountNumber;
            payment.Notes = model.Notes;
            payment.User = await GetAuthenticatedUser();

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
            var uniqueFileName = Guid.NewGuid() + "_" + model.PaymentDocumentFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.PaymentDocumentFile.CopyToAsync(fileStream);
            }

            payment.Document = uniqueFileName;
            payment.InvoiceId = model.InvoiceId;


            if (payment.RemainingAmount == 0)
            {
                invoice.IsPaid = true;
            }
            
            if (invoice.Renewal is not null)
            {
                if ((invoice.Renewal.Invoices.Where(e => e.IsPaid && e.Id != invoice.Id).Sum(e => e.Amount) + payment.PaidAmount + payment.RemainingAmount) == invoice.Renewal.TotalAmount)
                {
                    invoice.Renewal.IsPaid = true;
                }
            }
            else
            {
                if ((invoice.Lessor.Invoices.Where(e => e.IsPaid && e.Id != invoice.Id).Sum(e => e.Amount) + payment.PaidAmount + payment.RemainingAmount) == invoice.Lessor.TotalRentAmount)
                {
                    invoice.Lessor.IsPaid = true;
                }
            }

            _unitOfWork.PaymentRepository.Add(payment);

            var isSuccessSave = await _unitOfWork.SaveAsync();

            if (isSuccessSave)
            {
                TempData["payments-index-temp-msg"] = "create-success";
                return RedirectToAction(nameof(Index));
            }

            TempData["payments-index-temp-msg"] = "create-error";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["payments-index-temp-msg"] = "unknown-error";
            Log.Error("PaymentsController - Create[POST] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> FetchInvoice(int invoiceId)
    {
        var invoice = await _unitOfWork
            .InvoiceRepository
            .GetFirstOrDefaultAsync(x => x.Id == invoiceId,
                include: r => r.Include(t => t.Payments)
            );

        if (invoice is not null)
        {
            var remainingAmount = invoice.Amount - invoice.Payments.Sum(x => x.PaidAmount);
            var invoiceTotalPaidAmount = invoice.Amount - remainingAmount;

            return Json(new
            {
                amount = invoice.Amount,
                remainingAmount,
                invoiceTotalPaidAmount
            });
        }

        return NotFound();
    }

    [HttpGet]
    [Authorize(Permissions.Payments.View)]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var payment = await _unitOfWork
                .PaymentRepository
                .GetFirstOrDefaultAsync(x => x.Id == id,
                    include: e => e.Include(g => g.Invoice));

            if (payment is null)
            {
                return NotFound();
            }

            var paymentViewModel = new PaymentViewModel()
            {
                PaidAmount = payment.PaidAmount,
                InvoiceAmount = payment.Invoice.Amount,
                RemainingInvoiceAmount = payment.RemainingAmount,
                Date = payment.Date,
                PaymentType = payment.PaymentType,
                BankAccountNumber = payment.BankAccountNumber,
                PaymentDocumentFileName = payment.Document,
                InvoiceSelectList = await GetInvoiceSelectList(),
                InvoiceId = payment.InvoiceId,
                Notes = payment.Notes
            };

            return View(paymentViewModel);
        }
        catch (Exception e)
        {
            TempData["payments-index-temp-msg"] = "unknown-error";
            Log.Error("PaymentsController - Details[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    [Authorize(Permissions.Payments.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var payment = await _unitOfWork
                .PaymentRepository
                .GetFirstOrDefaultAsync(x => x.Id == id,
                    x => x.Include(a => a.Invoice)
                );

            var paymentViewModel = new EditPaymentViewModel()
            {
                Id = payment.Id,
                PaidAmount = payment.PaidAmount,
                InvoiceAmount = payment.Invoice.Amount,
                RemainingInvoiceAmount = payment.RemainingAmount,
                Date = payment.Date,
                Notes = payment.Notes,
                PaymentType = payment.PaymentType,
                BankAccountNumber = payment.BankAccountNumber,
                PaymentDocumentFileName = payment.Document,
                InvoiceId = payment.InvoiceId,
                InvoiceSelectList = await GetInvoiceSelectList(),
                InvoiceTotalPaidAmount = payment.Invoice.Amount - payment.RemainingAmount
            };

            return View(paymentViewModel);
        }
        catch (Exception e)
        {
            TempData["payments-index-temp-msg"] = "unknown-error";
            Log.Error("PaymentsController - Edit[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Permissions.Payments.Edit)]
    public async Task<IActionResult> Edit(EditPaymentViewModel model)
    {
        try
        {
            ModelState.Remove(nameof(model.InvoiceSelectList));

            if (!ModelState.IsValid)
            {
                model.InvoiceSelectList = await GetInvoiceSelectList();
                return View(model);
            }


            var payment = await _unitOfWork.PaymentRepository
                .GetFirstOrDefaultAsync(x => x.Id == model.Id);


            payment.Date = model.Date.Value;
            payment.PaymentType = model.PaymentType;
            payment.Notes = model.Notes;
            payment.BankAccountNumber = model.BankAccountNumber;
            payment.User = await GetAuthenticatedUser();


            if (model.PaymentDocumentFile is not null)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
                var uniqueFileName = Guid.NewGuid() + "_" + model.PaymentDocumentFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.PaymentDocumentFile.CopyToAsync(fileStream);
                }

                payment.Document = uniqueFileName;
            }

            _unitOfWork.PaymentRepository.Update(payment);

            var isSuccessSave = await _unitOfWork.SaveAsync();

            if (isSuccessSave)
            {
                TempData["payments-index-temp-msg"] = "edit-success";
                return RedirectToAction(nameof(Index));
            }

            TempData["payments-index-temp-msg"] = "edit-error";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["payments-index-temp-msg"] = "unknown-error";
            Log.Error("PaymentsController - Edit[POST] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    [Authorize(Permissions.Payments.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var payment = await _unitOfWork
                .PaymentRepository
                .GetFirstOrDefaultAsync(x => x.Id == id);

            if (payment is null)
            {
                return NotFound();
            }

            var invoice = await _unitOfWork.InvoiceRepository
                .GetFirstOrDefaultAsync(e => e.Id == payment.InvoiceId,
                    include: r => r.Include(e => e.Payments)
                        .Include(f => f.Lessor)
                        .Include(h => h.Renewal)
                    );

            var maxPaymentId = invoice.Payments.Max(p => p.Id);


            var isLastPayment = payment.Id == maxPaymentId;

            if (isLastPayment)
            {
                invoice.IsPaid = false;

                if (invoice.Renewal is not null)
                {
                    invoice.Renewal.IsPaid = false;
                }
                else
                {
                    invoice.Lessor.IsPaid = false;
                }

                _unitOfWork.PaymentRepository.Remove(payment);

                var isSuccessSave = await _unitOfWork.SaveAsync();

                if (isSuccessSave)
                {
                    TempData["payments-index-temp-msg"] = "delete-success";
                    return RedirectToAction(nameof(Index));
                }
            }


            TempData["payments-index-temp-msg"] = "delete-error";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["payments-index-temp-msg"] = "delete-error";
            Log.Error("PaymentsController - Delete[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    [Authorize(Permissions.Payments.View)]
    public async Task<ActionResult> GetPaymentsPdf(int? invoiceId, DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var paymentsDetailsViewModels =
                await _unitOfWork.PaymentRepository.GetAll(
                    include: e =>
                        e.Include(x => x.Invoice),
                    filters: new List<Expression<Func<Payment, bool>>>()
                    {
                        i => invoiceId == null || invoiceId == i.InvoiceId,
                        i => fromDate == null || i.Date >= fromDate,
                        i => toDate == null || i.Date <= toDate
                    }).Select(i => new PaymentDetailsViewModel
                {
                    TotalInvoiceAmount = i.Invoice.Amount,
                    PaidAmount = i.PaidAmount,
                    RemainingAmount = i.RemainingAmount,
                    Date = i.Date,
                    PaymentType = i.PaymentType,
                    BankAccountNumber = i.BankAccountNumber,
                    InvoiceNumber = i.Invoice.Number
                }).ToListAsync();

            if (!paymentsDetailsViewModels.Any())
            {
                TempData["payments-index-temp-msg"] = "create-success";
                return RedirectToAction(nameof(Index));
            }

            string invoiceNumber = null;

            if (invoiceId is not null)
            {
                invoiceNumber = paymentsDetailsViewModels.First().InvoiceNumber;
            }

            var paymentsInfoDataTable = new DataTable();
            paymentsInfoDataTable.Columns.Add("PrintDate");
            paymentsInfoDataTable.Columns.Add("User");
            paymentsInfoDataTable.Columns.Add("InvoiceNumber");

            var paymentsInfoDataTableRow = paymentsInfoDataTable.NewRow();
            paymentsInfoDataTableRow["PrintDate"] = DateTime.Now.ToString("yyyy-MM-dd HH:m");
            paymentsInfoDataTableRow["User"] = User.Identity.Name;
            paymentsInfoDataTableRow["InvoiceNumber"] = invoiceNumber;
            paymentsInfoDataTable.Rows.Add(paymentsInfoDataTableRow);

            var paymentsDataTable = new DataTable();
            paymentsDataTable.Columns.Add("Count");
            paymentsDataTable.Columns.Add("TotalInvoiceAmount");
            paymentsDataTable.Columns.Add("PaidAmount");
            paymentsDataTable.Columns.Add("RemainingAmount");
            paymentsDataTable.Columns.Add("Date");
            paymentsDataTable.Columns.Add("PaymentType");
            paymentsDataTable.Columns.Add("BankAccountNumber");
            paymentsDataTable.Columns.Add("InvoiceNumber");

            for (int i = 0; i < paymentsDetailsViewModels.Count; i++)
            {
                var paymentsDataTableRow = paymentsDataTable.NewRow();
                paymentsDataTableRow["Count"] = i + 1;
                paymentsDataTableRow["TotalInvoiceAmount"] = paymentsDetailsViewModels[i].TotalInvoiceAmount;
                paymentsDataTableRow["PaidAmount"] = paymentsDetailsViewModels[i].PaidAmount;
                paymentsDataTableRow["RemainingAmount"] = paymentsDetailsViewModels[i].RemainingAmount;
                paymentsDataTableRow["Date"] = paymentsDetailsViewModels[i].Date.ToString("dd-MM-yyyy");
                paymentsDataTableRow["PaymentType"] = paymentsDetailsViewModels[i].PaymentType;
                paymentsDataTableRow["BankAccountNumber"] = paymentsDetailsViewModels[i].BankAccountNumber;
                paymentsDataTableRow["InvoiceNumber"] = paymentsDetailsViewModels[i].InvoiceNumber;
                paymentsDataTable.Rows.Add(paymentsDataTableRow);
            }
            
            var mimetype = String.Empty;
            var extension = 1;

            var reportPath = $"{_webHostEnvironment.WebRootPath}\\reports\\payments.rdlc";

            var pdfReportName = $"{Guid.NewGuid()}.pdf";

            var localReport = new LocalReport(reportPath);

            localReport.AddDataSource("payments", paymentsDataTable);
            localReport.AddDataSource("paymentsInfo", paymentsInfoDataTable);

            var result = localReport.Execute(RenderType.Pdf, extension, null, mimetype);
            return File(result.MainStream, "application/pdf", pdfReportName);
        }
        catch (Exception e)
        {
            TempData["payments-index-temp-msg"] = "unknown-error";
            Log.Error("PaymentsController - GetPaymentsPdf[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }


    private async Task<SelectList> GetFilteredInvoiceSelectList()
    {
        var invoices = await (_unitOfWork.InvoiceRepository.All
            .Where(x => !x.IsPaid).Select(x => new
            {
                x.Id,
                x.Number
            })).ToListAsync();

        return new SelectList(invoices, "Id", "Number");
    }

    private async Task<SelectList> GetInvoiceSelectList()
    {
        var invoices = await (_unitOfWork.InvoiceRepository.All
            .Select(x => new
            {
                x.Id,
                x.Number
            })).ToListAsync();

        return new SelectList(invoices, "Id", "Number");
    }

    private Task<AppUser> GetAuthenticatedUser()
    {
        return _userManager.GetUserAsync(User);
    }
}