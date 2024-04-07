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

public class InvoicesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly UserManager<AppUser> _userManager;

    public InvoicesController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment,
        UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironment = webHostEnvironment;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Permissions.Invoices.View)]
    public async Task<IActionResult> Index()
    {
        try
        {
            var invoiceDetailsViewModels = (await _unitOfWork
                    .InvoiceRepository
                    .GetAllAsync(include: x
                        => x.Include(e => e.User)
                            .Include(w => w.Lessor)
                            .Include(q => q.Renewal)))
                .Select(x => new InvoiceDetailsViewModel()
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    InvoiceNumber = x.Number,
                    BankAccountNumber = x.BankAccountNumber,
                    To = x.To,
                    AgreementNumber = x.Renewal == null ? "--" : x.Renewal.AgreementNumber,
                    ContractNumber = x.Lessor.ContractNumber,
                    From = x.From,
                    UserName = x.User.UserName,
                    Notes = x.Notes,
                    IsPaid = x.IsPaid ? "نعم" : "لا"
                }).ToList();

            ViewBag.LessorsSelectList = await GetLessorsSelectList();
            ViewBag.RenewalsSelectList = await GetRenewalsSelectList();

            return View(invoiceDetailsViewModels);
        }
        catch (Exception e)
        {
            Log.Error("InvoicesController - Index[GET] - {@message}", e.Message);
            TempData["home-temp-msg"] = "unknown-error";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    [Authorize(Permissions.Invoices.Add)]
    public async Task<IActionResult> Create()
    {
        var invoiceDetailsViewModel = new InvoiceViewModel();

        invoiceDetailsViewModel.LessorSelectList = await GetLessorsSelectList(new List<Expression<Func<Lessor, bool>>>()
        {
            e => !e.IsPaid ||
                 (e.Renewals.Any() && !e.Renewals.OrderByDescending(r => r.EndDate)
                     .First().IsPaid)
        });

        return View(invoiceDetailsViewModel);
    }

    [HttpPost]
    [Authorize(Permissions.Invoices.Add)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InvoiceViewModel model)
    {
        try
        {
            ModelState.Remove(nameof(model.LessorSelectList));

            if (!ModelState.IsValid)
            {
                model.LessorSelectList = await GetLessorsSelectList(new List<Expression<Func<Lessor, bool>>>()
                {
                    e => !e.IsPaid ||
                         (e.Renewals.Any() && !e.Renewals.OrderByDescending(r => r.EndDate)
                             .First().IsPaid)
                });
                return View(model);
            }

            var lessor = (await _unitOfWork.LessorRepository.GetFirstOrDefaultAsync(x => x.Id == model.LessorId,
                include: q
                    => q.Include(s => s.Invoices)))!;

            var renewal =
                await _unitOfWork.RenewalRepository.GetFirstOrDefaultAsync(x =>
                        x.AgreementNumber == model.AgreementNumber,
                    include: t => t.Include(g => g.Invoices)
                );

            var isRenewalInvoice = renewal is not null;

            if (isRenewalInvoice)
            {
                var renewalInvoicesAmount = renewal!.Invoices.Sum(x => x.Amount);
                if (model.Amount > (renewal.TotalAmount - renewalInvoicesAmount))
                {
                    TempData["invoices-index-temp-msg"] = "unknown-error";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                var lessorInvoicesAmount = lessor.Invoices.Sum(e => e.Amount);
                if (model.Amount > (lessor.TotalRentAmount - lessorInvoicesAmount))
                {
                    TempData["invoices-index-temp-msg"] = "unknown-error";
                    return RedirectToAction(nameof(Index));
                }
            }


            var invoice = new Invoice();
            invoice.Number = model.Number;
            invoice.InvoiceDate = model.InvoiceDate.Value;
            invoice.From = model.From.Value;
            invoice.To = model.To.Value;
            invoice.Amount = model.Amount.Value;
            invoice.BankAccountNumber = model.BankAccountNumber;
            invoice.User = await GetAuthenticatedUser();
            invoice.Lessor = lessor;
            invoice.Renewal = renewal;
            invoice.Notes = model.Notes;


            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
            var uniqueFileName = Guid.NewGuid() + "_" + model.InvoiceDocumentFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.InvoiceDocumentFile.CopyToAsync(fileStream);
            }

            invoice.Document = uniqueFileName;


            _unitOfWork.InvoiceRepository.Add(invoice);

            var isSuccessSave = await _unitOfWork.SaveAsync();

            if (isSuccessSave)
            {
                TempData["invoices-index-temp-msg"] = "create-success";
                return RedirectToAction(nameof(Index));
            }

            TempData["invoices-index-temp-msg"] = "create-error";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["invoices-index-temp-msg"] = "unknown-error";
            Log.Error("InvoicesController - Create[POST] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }


    [HttpGet]
    [Authorize(Permissions.Invoices.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var invoice = await _unitOfWork
                .InvoiceRepository
                .GetFirstOrDefaultAsync(x => x.Id == id,
                    include: e => e.Include(r => r.Renewal));


            var lessorViewModel = new EditInvoiceViewModel()
            {
                Id = invoice.Id,
                Number = invoice.Number,
                AgreementNumber = invoice.Renewal?.AgreementNumber,
                InvoiceDate = invoice.InvoiceDate,
                From = invoice.From,
                To = invoice.To,
                Amount = invoice.Amount,
                Notes = invoice.Notes,
                BankAccountNumber = invoice.BankAccountNumber,
                InvoiceDocumentFileName = invoice.Document,
                LessorId = invoice.LessorId,
                LessorSelectList = await GetLessorsSelectList()
            };


            return View(lessorViewModel);
        }
        catch (Exception e)
        {
            TempData["invoices-index-temp-msg"] = "unknown-error";
            Log.Error("InvoicesController - Edit[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Permissions.Invoices.Edit)]
    public async Task<IActionResult> Edit(EditInvoiceViewModel model)
    {
        try
        {
            ModelState.Remove(nameof(model.LessorSelectList));

            if (!ModelState.IsValid)
            {
                model.LessorSelectList = await GetLessorsSelectList();
                return View(model);
            }

            var invoice = await _unitOfWork
                .InvoiceRepository
                .GetFirstOrDefaultAsync(x => x.Id == model.Id,
                    include: e => e.Include(r => r.Renewal));

            if (invoice is null)
            {
                TempData["invoices-index-temp-msg"] = "unknown-error";
                return RedirectToAction(nameof(Index));
            }


            invoice.Number = model.Number;
            invoice.InvoiceDate = model.InvoiceDate.Value;
            invoice.From = model.From.Value;
            invoice.To = model.To.Value;
            invoice.BankAccountNumber = model.BankAccountNumber;
            invoice.Notes = model.Notes;
            invoice.User = await GetAuthenticatedUser();


            if (model.InvoiceDocumentFile is not null)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
                var uniqueFileName = Guid.NewGuid() + "_" + model.InvoiceDocumentFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.InvoiceDocumentFile.CopyToAsync(fileStream);
                }

                invoice.Document = uniqueFileName;
            }


            invoice.User = await GetAuthenticatedUser();

            _unitOfWork.InvoiceRepository.Update(invoice);

            var isSuccessSave = await _unitOfWork.SaveAsync();

            if (isSuccessSave)
            {
                TempData["invoices-index-temp-msg"] = "edit-success";
                return RedirectToAction(nameof(Index));
            }

            TempData["invoices-index-temp-msg"] = "edit-error";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["invoices-index-temp-msg"] = "unknown-error";
            Log.Error("InvoicesController - Edit[POST] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    [Authorize(Permissions.Invoices.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var invoice = await _unitOfWork
                .InvoiceRepository
                .GetFirstOrDefaultAsync(x => x.Id == id);

            if (invoice is null)
            {
                return NotFound();
            }

            _unitOfWork.InvoiceRepository.Remove(invoice);

            var isSuccessSave = await _unitOfWork.SaveAsync();

            if (isSuccessSave)
            {
                TempData["invoices-index-temp-msg"] = "delete-success";
                return RedirectToAction(nameof(Index));
            }

            TempData["invoices-index-temp-msg"] = "delete-error";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["invoices-index-temp-msg"] = "delete-error";
            Log.Error("InvoicesController - Delete[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    [Authorize(Permissions.Invoices.View)]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var invoice = await _unitOfWork
                .InvoiceRepository
                .GetFirstOrDefaultAsync(x => x.Id == id,
                    include: t => t.Include(h => h.Renewal));

            if (invoice is null)
            {
                return NotFound();
            }

            var lessorViewModel = new InvoiceViewModel()
            {
                Id = invoice.Id,
                Number = invoice.Number,
                AgreementNumber = invoice.Renewal?.AgreementNumber,
                InvoiceDate = invoice.InvoiceDate,
                From = invoice.From,
                To = invoice.To,
                Amount = invoice.Amount,
                Notes = invoice.Notes,
                BankAccountNumber = invoice.BankAccountNumber,
                InvoiceDocumentFileName = invoice.Document,
                LessorId = invoice.LessorId,
                RenewalId = invoice.RenewalId,
                LessorSelectList = await GetLessorsSelectList()
            };

            return View(lessorViewModel);
        }
        catch (Exception e)
        {
            TempData["invoices-index-temp-msg"] = "unknown-error";
            Log.Error("InvoicesController - Details[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }


    [HttpGet]
    public async Task<IActionResult> FetchNoPaidRenewals(int lessorId)
    {
        var renewals = (await _unitOfWork.RenewalRepository.All.Where(x => x.LessorId == lessorId).ToListAsync());

        if (renewals.Any())
        {
            return Json(renewals.First(x => !x.IsPaid).AgreementNumber);
        }

        return NotFound();
    }

    [HttpGet]
    public IActionResult DownloadFile(string fileName)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileStream = System.IO.File.OpenRead(filePath);
            return File(fileStream, "application/octet-stream", fileName);
        }
        catch (Exception e)
        {
            TempData["invoices-index-temp-msg"] = "unknown-error";
            Log.Error("InvoicesController - DownloadFile[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    
    [HttpGet]
    [Authorize(Permissions.Invoices.View)]
    public async Task<ActionResult> GetInvoicesPdf(int? lessorId, int? renewalId, DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            if (lessorId != null && renewalId != null)
            {
                TempData["invoices-index-temp-msg"] = "unknown-error";
                return RedirectToAction(nameof(Index));
            }

            var invoicesDetailsViewModels =
                await _unitOfWork.InvoiceRepository.GetAll(
                    include: e =>
                        e.Include(x => x.Lessor)
                            .Include(x => x.Renewal),
                    filters: new List<Expression<Func<Invoice, bool>>>()
                    {
                        i => lessorId == null || lessorId == i.LessorId,
                        i => renewalId == null || lessorId == i.RenewalId,
                        i => fromDate == null || i.InvoiceDate >= fromDate,
                        i => toDate == null || i.InvoiceDate <= toDate
                    }).Select(i => new InvoiceDetailsViewModel()
                {
                    InvoiceNumber = i.Number,
                    ContractNumber = i.Lessor.ContractNumber,
                    From = i.From,
                    To = i.To,
                    Amount = i.Amount,
                    BankAccountNumber = i.BankAccountNumber,
                    AgreementNumber = i.Renewal != null ? i.Renewal.AgreementNumber : "--"
                }).ToListAsync();

            if (!invoicesDetailsViewModels.Any())
            {
                TempData["renewals-index-temp-msg"] = "create-success";
                return RedirectToAction(nameof(Index));
            }

            string agreementNumber = null;
            string contractNumber = null;

            if (lessorId is not null)
            {
                contractNumber = invoicesDetailsViewModels.First().ContractNumber;
            }
            else if (renewalId != null)
            {
                agreementNumber = invoicesDetailsViewModels.First().AgreementNumber;
            }

            var invoicesInfoDataTable = new DataTable();
            invoicesInfoDataTable.Columns.Add("PrintDate");
            invoicesInfoDataTable.Columns.Add("User");
            invoicesInfoDataTable.Columns.Add("ContractNumber");
            invoicesInfoDataTable.Columns.Add("AgreementNumber");

            var invoicesInfoDataTableRow = invoicesInfoDataTable.NewRow();
            invoicesInfoDataTableRow["PrintDate"] = DateTime.Now.ToString("yyyy-MM-dd HH:m");
            invoicesInfoDataTableRow["User"] = User.Identity.Name;
            invoicesInfoDataTableRow["ContractNumber"] = contractNumber;
            invoicesInfoDataTableRow["AgreementNumber"] = agreementNumber;
            invoicesInfoDataTable.Rows.Add(invoicesInfoDataTableRow);


            var invoicesDataTable = new DataTable();
            invoicesDataTable.Columns.Add("Count");
            invoicesDataTable.Columns.Add("InvoiceNumber");
            invoicesDataTable.Columns.Add("ContractNumber");
            invoicesDataTable.Columns.Add("From");
            invoicesDataTable.Columns.Add("To");
            invoicesDataTable.Columns.Add("Amount");
            invoicesDataTable.Columns.Add("BankAccountNumber");
            invoicesDataTable.Columns.Add("AgreementNumber");


            for (var i = 0; i < invoicesDetailsViewModels.Count; i++)
            {
                var row = invoicesDataTable.NewRow();
                row["Count"] = i + 1;
                row["InvoiceNumber"] = invoicesDetailsViewModels[i].InvoiceNumber;
                row["ContractNumber"] = invoicesDetailsViewModels[i].ContractNumber;
                row["From"] = invoicesDetailsViewModels[i].From.ToString("dd-MM-yyyy");
                row["To"] = invoicesDetailsViewModels[i].To.ToString("dd-MM-yyyy");
                row["Amount"] = Math.Round(invoicesDetailsViewModels[i].Amount, 2);
                row["BankAccountNumber"] = invoicesDetailsViewModels[i].BankAccountNumber;
                row["AgreementNumber"] = invoicesDetailsViewModels[i].AgreementNumber;

                invoicesDataTable.Rows.Add(row);
            }

            var mimetype = String.Empty;
            var extension = 1;

            var reportPath = $"{_webHostEnvironment.WebRootPath}\\reports\\invoices.rdlc";

            var pdfReportName = $"{Guid.NewGuid()}.pdf";

            var localReport = new LocalReport(reportPath);

            localReport.AddDataSource("invoices", invoicesDataTable);
            localReport.AddDataSource("invoicesInfo", invoicesInfoDataTable);

            var result = localReport.Execute(RenderType.Pdf, extension, null, mimetype);
            return File(result.MainStream, "application/pdf", pdfReportName);
        }
        catch (Exception e)
        {
            TempData["invoices-index-temp-msg"] = "unknown-error";
            Log.Error("InvoicesController - Details[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }
    
    private async Task<SelectList> GetRenewalsSelectList()
    {
        var lessors = await _unitOfWork.RenewalRepository.All
            .Select(x => new
            {
                x.Id,
                x.AgreementNumber
            })
            .ToListAsync();

        return new SelectList(lessors, "Id", "AgreementNumber");
    }

    private async Task<SelectList> GetFilteredLessors()
    {
        var result = _unitOfWork.LessorRepository.All
            .Where(e => !e.IsPaid ||
                        (e.Renewals.Any() && !e.Renewals.OrderByDescending(r => r.EndDate)
                            .First().IsPaid));

        var lessors = await result
            .Select(x => new
            {
                x.Id,
                x.ContractNumber
            })
            .ToListAsync();

        return new SelectList(lessors, "Id", "ContractNumber");
    }

    private async Task<SelectList> GetLessorsSelectList(List<Expression<Func<Lessor, bool>>>? filters = null)
    {
        var lessors = await _unitOfWork.LessorRepository.GetAll(filters).Select(x => new
            {
                x.Id,
                x.ContractNumber
            })
            .ToListAsync();

        return new SelectList(lessors, "Id", "ContractNumber");
    }


    private Task<AppUser> GetAuthenticatedUser()
    {
        return _userManager.GetUserAsync(User);
    }

    
}