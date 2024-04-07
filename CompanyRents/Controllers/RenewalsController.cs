using System.Data;
using System.Diagnostics.Contracts;
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

public class RenewalsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly UserManager<AppUser> _userManager;

    public RenewalsController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment,
        UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironment = webHostEnvironment;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Permissions.Renewals.View)]
    public async Task<IActionResult> Index()
    {
        try
        {
            var renewalViewModels = (await _unitOfWork
                .RenewalRepository
                .GetAllAsync(include: x
                    => x.Include(e => e.User)
                        .Include(w => w.Lessor)
                )).Select(x => new RenewalDetailsViewModel
            {
                Id = x.Id,
                AgreementNumber = x.AgreementNumber,
                ContractNumber = x.Lessor.ContractNumber,
                Amount = x.Amount,
                TaxRatio = x.TaxRatio,
                TotalAmount = x.TotalAmount,
                AgreementDate = x.AgreementDate,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                RenewalPeriod = x.RenewalPeriod,
                UserName = x.User.UserName,
                ParticipationRatio = x.ParticipationRatio,
                AgreementDateString = x.AgreementDateString,
                StartDateString = x.StartDateString,
                EndDateString = x.EndDateString,
                Notes = x.Notes,
                IsPaid = x.IsPaid ? "نعم" : "لا"
            }).ToList();
            ViewBag.LessorsSelectList = await GetLessorSelectList(new List<Expression<Func<Lessor, bool>>>()
            {
                e => e.Renewals.Any()
            });

            return View(renewalViewModels);
        }
        catch (Exception e)
        {
            Log.Error("RenewalsController - Index[GET] - {@message}", e.Message);
            TempData["home-temp-msg"] = "unknown-error";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    [Authorize(Permissions.Renewals.Add)]
    public async Task<IActionResult> Create()
    {
        var renewalViewModel = new RenewalViewModel();

        renewalViewModel.LessorSelectList = await GetLessorSelectList(filters: new List<Expression<Func<Lessor, bool>>>
        {
            e => e.IsPaid
        });

        return View(renewalViewModel);
    }

    [HttpPost]
    [Authorize(Permissions.Renewals.Add)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RenewalViewModel model)
    {
        try
        {
            ModelState.Remove(nameof(model.LessorSelectList));
            if (!ModelState.IsValid)
            {
                model.LessorSelectList = await GetLessorSelectList(filters: new List<Expression<Func<Lessor, bool>>>
                {
                    e => e.IsPaid
                });

                return View(model);
            }

            var renewal = new Renewal();
            renewal.RenewalPeriod = model.RenewalPeriod.Value;
            renewal.Amount = model.Amount.Value;
            renewal.AgreementNumber = model.AgreementNumber;
            renewal.LessorId = model.LessorId.Value;
            renewal.TaxRatio = model.TaxRatio.Value;
            renewal.ParticipationRatio = model.ParticipationRatio.Value;
            renewal.Notes = model.Notes;
            renewal.User = await GetAuthenticatedUser();


            model.AgreementDate =
                DateConversionUtility.ConvertDate(model.AgreementDateString,
                    DateConversionUtility.OutputCalendar.Gregorian);


            model.StartDate =
                DateConversionUtility.ConvertDate(model.StartDateString,
                    DateConversionUtility.OutputCalendar.Gregorian);

            model.EndDate =
                DateConversionUtility.ConvertDate(model.EndDateString,
                    DateConversionUtility.OutputCalendar.Gregorian);

            renewal.AgreementDateString = model.AgreementDateString;
            renewal.StartDateString = model.StartDateString;
            renewal.EndDateString = model.EndDateString;

            renewal.AgreementDate = model.AgreementDate;
            renewal.StartDate = model.StartDate;
            renewal.EndDate = model.EndDate;

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
            var uniqueFileName = Guid.NewGuid() + "_" + model.AgreementDocumentFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.AgreementDocumentFile.CopyToAsync(fileStream);
            }

            renewal.Document = uniqueFileName;


            _unitOfWork.RenewalRepository.Add(renewal);

            var isSuccessSave = await _unitOfWork.SaveAsync();

            if (isSuccessSave)
            {
                TempData["renewals-index-temp-msg"] = "create-success";
                return RedirectToAction(nameof(Index));
            }

            TempData["renewals-index-temp-msg"] = "create-error";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["renewals-index-temp-msg"] = "unknown-error";
            Log.Error("RenewalsController - Create[POST] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }


    [HttpGet]
    [Authorize(Permissions.Renewals.View)]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var renewal = await _unitOfWork
                .RenewalRepository
                .GetFirstOrDefaultAsync(x => x.Id == id,
                    include: r => r.Include(t => t.Invoices));

            if (renewal is null)
            {
                return NotFound();
            }

            var renewalViewModel = new RenewalViewModel()
            {
                AgreementNumber = renewal.AgreementNumber,
                RenewalPeriod = renewal.RenewalPeriod,
                Amount = renewal.Amount,
                TaxRatio = renewal.TaxRatio,
                TotalAmount = renewal.TotalAmount,
                StartDate = renewal.StartDate,
                EndDate = renewal.EndDate,
                AgreementDate = renewal.AgreementDate,
                AgreementDocumentFileName = renewal.Document,
                LessorId = renewal.LessorId,
                ParticipationRatio = renewal.ParticipationRatio,
                LessorSelectList = await GetLessorSelectList(),
                AgreementDateString = renewal.AgreementDateString,
                StartDateString = renewal.StartDateString,
                EndDateString = renewal.EndDateString,
                Notes = renewal.Notes,
            };

            return View(renewalViewModel);
        }
        catch (Exception e)
        {
            TempData["renewals-index-temp-msg"] = "unknown-error";
            Log.Error("RenewalsController - Details[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    [Authorize(Permissions.Renewals.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var renewal = await _unitOfWork
                .RenewalRepository
                .GetFirstOrDefaultAsync(x => x.Id == id,
                    include: e => e.Include(t => t.Invoices));


            var canEditFinances = !(renewal.Invoices.Any());

            var renewalViewModel = new EditRenewalViewModel()
            {
                Id = renewal.Id,
                AgreementNumber = renewal.AgreementNumber,
                RenewalPeriod = renewal.RenewalPeriod,
                Amount = renewal.Amount,
                TaxRatio = renewal.TaxRatio,
                TotalAmount = renewal.TotalAmount,
                StartDate = renewal.StartDate,
                EndDate = renewal.EndDate,
                AgreementDocumentFileName = renewal.Document,
                AgreementDate = renewal.AgreementDate,
                ParticipationRatio = renewal.ParticipationRatio,
                LessorSelectList = await GetLessorSelectList(filters: new List<Expression<Func<Lessor, bool>>>
                {
                    e => !canEditFinances || e.IsPaid
                }),
                LessorId = renewal.LessorId,
                AgreementDateString = renewal.AgreementDateString,
                StartDateString = renewal.StartDateString,
                EndDateString = renewal.EndDateString,
                Notes = renewal.Notes,
            };


            ViewBag.CanEditFinances = canEditFinances;

            return View(renewalViewModel);
        }
        catch (Exception e)
        {
            TempData["renewals-index-temp-msg"] = "unknown-error";
            Log.Error("RenewalsController - Edit[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Permissions.Renewals.Edit)]
    public async Task<IActionResult> Edit(EditRenewalViewModel model)
    {
        try
        {
            var renewal = await _unitOfWork
                .RenewalRepository
                .GetFirstOrDefaultAsync(x => x.Id == model.Id,
                    include: e => e.Include(t => t.Invoices));

            if (renewal is null)
            {
                TempData["renewals-index-temp-msg"] = "unknown-error";
                return RedirectToAction(nameof(Index));
            }


            var canEditFinances = !(renewal.Invoices.Any());

            ModelState.Remove(nameof(model.LessorSelectList));
            
            if (!ModelState.IsValid)
            {
                model.LessorSelectList = await GetLessorSelectList(filters: new List<Expression<Func<Lessor, bool>>>
                {
                    e => !canEditFinances || e.IsPaid
                });
                return View(model);
            }


            if (canEditFinances)
            {
                renewal.Amount = model.Amount.Value;
                renewal.TaxRatio = model.TaxRatio.Value;
                renewal.LessorId = model.LessorId.Value;
            }

            renewal.RenewalPeriod = model.RenewalPeriod.Value;
            renewal.Notes = model.Notes;

            renewal.AgreementNumber = model.AgreementNumber;
            renewal.ParticipationRatio = model.ParticipationRatio.Value;
            renewal.User = await GetAuthenticatedUser();


            model.AgreementDate =
                DateConversionUtility.ConvertDate(model.AgreementDateString,
                    DateConversionUtility.OutputCalendar.Gregorian);


            model.StartDate =
                DateConversionUtility.ConvertDate(model.StartDateString,
                    DateConversionUtility.OutputCalendar.Gregorian);

            model.EndDate =
                DateConversionUtility.ConvertDate(model.EndDateString,
                    DateConversionUtility.OutputCalendar.Gregorian);

            renewal.AgreementDateString = model.AgreementDateString;
            renewal.StartDateString = model.StartDateString;
            renewal.EndDateString = model.EndDateString;

            renewal.AgreementDate = model.AgreementDate;
            renewal.StartDate = model.StartDate;
            renewal.EndDate = model.EndDate;


            if (model.AgreementDocumentFile is not null)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
                var uniqueFileName = Guid.NewGuid() + "_" + model.AgreementDocumentFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AgreementDocumentFile.CopyToAsync(fileStream);
                }

                renewal.Document = uniqueFileName;
            }


            _unitOfWork.RenewalRepository.Update(renewal);

            var isSuccessSave = await _unitOfWork.SaveAsync();

            if (isSuccessSave)
            {
                TempData["renewals-index-temp-msg"] = "edit-success";
                return RedirectToAction(nameof(Index));
            }

            TempData["renewals-index-temp-msg"] = "edit-error";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["renewals-index-temp-msg"] = "unknown-error";
            Log.Error("RenewalsController - Edit[POST] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }


    [HttpGet]
    [Authorize(Permissions.Renewals.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var renewal = await _unitOfWork
                .RenewalRepository
                .GetFirstOrDefaultAsync(x => x.Id == id);

            if (renewal is null)
            {
                return NotFound();
            }

            _unitOfWork.RenewalRepository.Remove(renewal);

            var isSuccessSave = await _unitOfWork.SaveAsync();

            if (isSuccessSave)
            {
                TempData["renewals-index-temp-msg"] = "delete-success";
                return RedirectToAction(nameof(Index));
            }

            TempData["renewals-index-temp-msg"] = "delete-error";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["renewals-index-temp-msg"] = "delete-error";
            Log.Error("RenewalsController - Delete[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
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
            TempData["renewals-index-temp-msg"] = "unknown-error";
            Log.Error("RenewalsController - DownloadFile[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    [Authorize(Permissions.Renewals.View)]
    public async Task<ActionResult> GetRenewalsPdf(int? lessorId, DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var renewalDetailsViewModels = (await _unitOfWork.RenewalRepository.GetAllAsync(
                filters: new List<Expression<Func<Renewal, bool>>>()
                {
                    c => lessorId == null || lessorId == c.LessorId,
                    c => fromDate == null || c.AgreementDate >= fromDate,
                    c => toDate == null || c.AgreementDate <= toDate,
                },
                include: x => x.Include(
                    a => a.Lessor
                )
            )).Select(x => new RenewalDetailsViewModel()
            {
                AgreementNumber = x.AgreementNumber,
                ContractNumber = x.Lessor.ContractNumber,
                RenewalPeriod = x.RenewalPeriod,
                AgreementDateString = x.AgreementDateString,
                StartDateString = x.StartDateString,
                EndDateString = x.EndDateString,
                Amount = x.Amount,
                TaxRatio = x.TaxRatio,
                TotalAmount = x.TotalAmount,
                ParticipationRatio = x.ParticipationRatio
            }).ToList();

            if (!renewalDetailsViewModels.Any())
            {
                TempData["renewals-index-temp-msg"] = "create-success";
                return RedirectToAction(nameof(Index));
            }

            var renewalsDataTable = new DataTable();
            renewalsDataTable.Columns.Add("index");
            renewalsDataTable.Columns.Add("AgreementNumber");
            renewalsDataTable.Columns.Add("RenewalPeriod");
            renewalsDataTable.Columns.Add("Amount");
            renewalsDataTable.Columns.Add("TaxRatio");
            renewalsDataTable.Columns.Add("TotalAmount");
            renewalsDataTable.Columns.Add("ParticipationRatio");
            renewalsDataTable.Columns.Add("AgreementDate");
            renewalsDataTable.Columns.Add("StartDate");
            renewalsDataTable.Columns.Add("EndDate");
            renewalsDataTable.Columns.Add("ContractNumber");

            for (var i = 0; i < renewalDetailsViewModels.Count; i++)
            {
                var row = renewalsDataTable.NewRow();
                row["index"] = i + 1;
                row["AgreementNumber"] = renewalDetailsViewModels[i].AgreementNumber;
                row["RenewalPeriod"] = renewalDetailsViewModels[i].RenewalPeriod;
                row["Amount"] = Math.Round(renewalDetailsViewModels[i].Amount, 2);
                row["TaxRatio"] = Math.Round(renewalDetailsViewModels[i].TaxRatio, 2);
                row["TotalAmount"] = Math.Round(renewalDetailsViewModels[i].TotalAmount, 2);
                row["ParticipationRatio"] = renewalDetailsViewModels[i].ParticipationRatio;
                row["AgreementDate"] = renewalDetailsViewModels[i].AgreementDateString;
                row["StartDate"] = renewalDetailsViewModels[i].StartDateString;
                row["EndDate"] = renewalDetailsViewModels[i].EndDateString;
                row["ContractNumber"] = renewalDetailsViewModels[i].ContractNumber;

                renewalsDataTable.Rows.Add(row);
            }

            var renewalsInfoDataTable = new DataTable();
            renewalsInfoDataTable.Columns.Add("PrintDate");
            renewalsInfoDataTable.Columns.Add("User");
            var renewalsInfoDataTableRow = renewalsInfoDataTable.NewRow();
            renewalsInfoDataTableRow["PrintDate"] = DateTime.Now.ToString("yyyy-MM-dd HH:m");
            renewalsInfoDataTableRow["User"] = User.Identity.Name;
            renewalsInfoDataTable.Rows.Add(renewalsInfoDataTableRow);

            var filteredRenewalsInfoDataTable = new DataTable();
            filteredRenewalsInfoDataTable.Columns.Add("PrintDate");
            filteredRenewalsInfoDataTable.Columns.Add("User");
            filteredRenewalsInfoDataTable.Columns.Add("ContractNumber");
            var filteredRenewalsInfoDataTableRow = filteredRenewalsInfoDataTable.NewRow();
            filteredRenewalsInfoDataTableRow["PrintDate"] = DateTime.Now.ToString("yyyy-MM-dd HH:m");
            filteredRenewalsInfoDataTableRow["User"] = User.Identity.Name;
            var lessor = await _unitOfWork.LessorRepository.GetFirstOrDefaultAsync(x => x.Id == lessorId);

            filteredRenewalsInfoDataTableRow["ContractNumber"] = lessor?.ContractNumber ?? "--";
            filteredRenewalsInfoDataTable.Rows.Add(filteredRenewalsInfoDataTableRow);


            var mimetype = String.Empty;
            var extension = 1;

            string reportPath;

            if (lessor is not null)
            {
                reportPath = $"{_webHostEnvironment.WebRootPath}\\reports\\filteredRenewals.rdlc";
            }
            else
            {
                reportPath = $"{_webHostEnvironment.WebRootPath}\\reports\\renewals.rdlc";
            }

            var pdfReportName = $"{Guid.NewGuid()}.pdf";

            var localReport = new LocalReport(reportPath);

            localReport.AddDataSource("renewals", renewalsDataTable);
            localReport.AddDataSource("renewalsInfo", renewalsInfoDataTable);
            localReport.AddDataSource("filteredRenewalsInfo", filteredRenewalsInfoDataTable);

            var result = localReport.Execute(RenderType.Pdf, extension, null, mimetype);
            return File(result.MainStream, "application/pdf", pdfReportName);
        }
        catch (Exception e)
        {
            TempData["renewals-index-temp-msg"] = "unknown-error";
            Log.Error("RenewalsController - GetRenewalsPdf[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task<SelectList> GetLessorSelectList(List<Expression<Func<Lessor, bool>>>? filters = null)
    {
        var lessors = await _unitOfWork
            .LessorRepository
            .GetAll(filters: filters)
            .Select(x => new
            {
                x.Id,
                x.ContractNumber
            }).ToListAsync();

        return new SelectList(lessors, "Id", "ContractNumber");
    }

    private Task<AppUser> GetAuthenticatedUser()
    {
        return _userManager.GetUserAsync(User);
    }
}