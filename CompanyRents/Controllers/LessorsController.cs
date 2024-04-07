using System.Data;
using System.Linq.Expressions;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompanyRents.Models;
using Serilog;
using AspNetCore.Reporting;

namespace CompanyRents.Controllers;

public class LessorsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly UserManager<AppUser> _userManager;

    public LessorsController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment,
        UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironment = webHostEnvironment;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Permissions.Lessors.View)]
    public async Task<IActionResult> Index()
    {
        try
        {
            var lessors = (await _unitOfWork
                .LessorRepository
                .GetAllAsync(include: x
                    => x.Include(e => e.User)));
            
            
            return View(lessors);
        }
        catch (Exception e)
        {
            Log.Error("LessorsController - Index[GET] - {@message}", e.Message);
            TempData["home-temp-msg"] = "unknown-error";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    [Authorize(Permissions.Lessors.Add)]
    public IActionResult Create()
    {
        var lessorViewModel = new LessorViewModel();
        return View(lessorViewModel);
    }

    [HttpPost]
    [Authorize(Permissions.Lessors.Add)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessorViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var lessor = new Lessor();

            lessor.Name = model.Name;
            lessor.PropertyType = model.PropertyType;
            lessor.ContractNumber = model.ContractNumber;
            lessor.RentAmount = model.RentAmount.Value;
            lessor.RentTaxRatio = model.RentTaxRatio.Value;
            lessor.RentPaymentPeriodType = model.RentPaymentPeriodType;
            lessor.Notes = model.Notes;

            if (model.PaymentMethod == "m")
            {
                lessor.RentPeriod = model.RentPeriod;
            }
            else
            {
                lessor.RentPeriod = model.RentPeriod * 12;
            }

            model.ContractDate =
                DateConversionUtility.ConvertDate(model.ContractDateString, DateConversionUtility.OutputCalendar.Gregorian);


            model.RentStartDate =
                DateConversionUtility.ConvertDate(model.RentStartDateString,
                    DateConversionUtility.OutputCalendar.Gregorian);

            model.RentEndDate =
                DateConversionUtility.ConvertDate(model.RentEndDateString,
                    DateConversionUtility.OutputCalendar.Gregorian);
            
            lessor.ContractDateString = model.ContractDateString;
            lessor.RentStartDateString = model.RentStartDateString;
            lessor.RentEndDateString = model.RentEndDateString;
            
            lessor.ContractDate = model.ContractDate;
            lessor.RentStartDate = model.RentStartDate;
            lessor.RentEndDate = model.RentEndDate;
            
            lessor.ArrivalHallNumber = model.ArrivalHallNumber;
            lessor.City = model.City;


            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
            var uniqueFileName = Guid.NewGuid() + "_" + model.ContractDocumentFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ContractDocumentFile.CopyToAsync(fileStream);
            }

            lessor.ContractDocument = uniqueFileName;

            if (!model.ParticipationRatiosLessorViewModels.Any())
            {
                TempData["lessors-index-temp-msg"] = "unknown-error";
                return RedirectToAction(nameof(Index));
            }
            
            if (model.ParticipationRatiosLessorViewModels.Any(e => e.Ratio == null || e.YearNumber == null))
            {
                TempData["lessors-index-temp-msg"] = "unknown-error";
                return RedirectToAction(nameof(Index));
            }
            
            lessor.ParticipationRatios = model.ParticipationRatiosLessorViewModels.Select(x => new ParticipationRatio()
            {
                YearNumber = x.YearNumber.Value,
                Ratio = x.Ratio.Value
            }).ToList();

            
            
            lessor.User = await GetAuthenticatedUser();
            _unitOfWork.LessorRepository.Add(lessor);

            var isSuccessSave = await _unitOfWork.SaveAsync();

            if (isSuccessSave)
            {
                TempData["lessors-index-temp-msg"] = "create-success";
                return RedirectToAction(nameof(Index));
            }

            TempData["lessors-index-temp-msg"] = "create-error";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["lessors-index-temp-msg"] = "unknown-error";
            Log.Error("LessorsController - Create[POST] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    [Authorize(Permissions.Lessors.View)]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var lessor = await _unitOfWork
                .LessorRepository
                .GetFirstOrDefaultAsync(x => x.Id == id,
                    include: r => r.Include(t => t.ParticipationRatios));

            if (lessor is null)
            {
                return NotFound();
            }

            var lessorViewModel = new LessorViewModel()
            {
                Id = lessor.Id,
                Name = lessor.Name,
                PropertyType = lessor.PropertyType,
                ContractNumber = lessor.ContractNumber,
                RentAmount = lessor.RentAmount,
                RentTaxRatio = lessor.RentTaxRatio,
                TotalRentAmount = lessor.TotalRentAmount,
                RentPaymentPeriodType = lessor.RentPaymentPeriodType,
                RentPeriod = lessor.RentPeriod,
                PaymentMethod = lessor.RentPeriod > 5 ? "m" : "y",
                ContractDate = lessor.ContractDate,
                RentStartDate = lessor.RentStartDate,
                RentEndDate = lessor.RentEndDate,
                ContractDateString = lessor.ContractDateString,
                RentStartDateString = lessor.RentStartDateString,
                RentEndDateString = lessor.RentEndDateString,
                ArrivalHallNumber = lessor.ArrivalHallNumber,
                DocumentFileName = lessor.ContractDocument,
                Notes = lessor.Notes,
                City = lessor.City,
                ParticipationRatiosLessorViewModels =
                    lessor.ParticipationRatios
                        .Select(x => new ParticipationRatioViewModel()
                        {
                            Ratio = x.Ratio,
                            YearNumber = x.YearNumber
                        }).ToList()
                    
            };

            return View(lessorViewModel);
        }
        catch (Exception e)
        {
            TempData["lessors-index-temp-msg"] = "unknown-error";
            Log.Error("LessorsController - Details[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }


    [HttpGet]
    [Authorize(Permissions.Lessors.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var lessor = await _unitOfWork
                .LessorRepository
                .GetFirstOrDefaultAsync(x => x.Id == id,
                    x => x.Include(a => a.Renewals)
                        .Include(t => t.Invoices)
                        .Include(w => w.ParticipationRatios)
                );

            var lessorViewModel = new EditLessorViewModel()
            {
                Id = lessor.Id,
                Name = lessor.Name,
                PropertyType = lessor.PropertyType,
                ContractNumber = lessor.ContractNumber,
                RentAmount = lessor.RentAmount,
                RentTaxRatio = lessor.RentTaxRatio,
                TotalRentAmount = lessor.TotalRentAmount,
                RentPaymentPeriodType = lessor.RentPaymentPeriodType,
                RentPeriod = lessor.RentPeriod,
                PaymentMethod = lessor.RentPeriod > 5 ? "m" : "y",
                ContractDate = lessor.ContractDate,
                RentStartDate = lessor.RentStartDate,
                RentEndDate = lessor.RentEndDate,
                ContractDateString = lessor.ContractDateString,
                RentStartDateString = lessor.RentStartDateString,
                RentEndDateString = lessor.RentEndDateString,
                ArrivalHallNumber = lessor.ArrivalHallNumber,
                DocumentFileName = lessor.ContractDocument,
                Notes = lessor.Notes,
                City = lessor.City,
                ParticipationRatiosLessorViewModels =
                    lessor.ParticipationRatios
                        .Select(x => new ParticipationRatioViewModel()
                        {
                            Ratio = x.Ratio,
                            YearNumber = x.YearNumber
                        }).ToList()
            };
            

            var canEditFinances = !(lessor.Renewals.Any() || lessor.Invoices.Any());

            ViewBag.CanEditFinances = canEditFinances;

            return View(lessorViewModel);
        }
        catch (Exception e)
        {
            TempData["lessors-index-temp-msg"] = "unknown-error";
            Log.Error("LessorsController - Edit[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Permissions.Lessors.Edit)]
    public async Task<IActionResult> Edit(EditLessorViewModel model)
    {
        try
        {
            ModelState.Remove(nameof(model.ParticipationRatiosLessorViewModels));

            var lessor = await _unitOfWork
                .LessorRepository
                .GetFirstOrDefaultAsync(x => x.Id == model.Id,
                    x => x.Include(a => a.Renewals)
                        .Include(t => t.Invoices)
                );

            if (lessor is null)
            {
                TempData["lessors-index-temp-msg"] = "unknown-error";
                return RedirectToAction(nameof(Index));
            }

            var canEditFinances = !(lessor.Renewals.Any() || lessor.Invoices.Any());
            
            if (!ModelState.IsValid)
            {
                ViewBag.CanEditFinances = canEditFinances;
                return View(model);
            }
            

            lessor.Name = model.Name;
            lessor.PropertyType = model.PropertyType;
            lessor.ContractNumber = model.ContractNumber;
            lessor.Notes = model.Notes;

            if (canEditFinances)
            {
                lessor.RentAmount = model.RentAmount.Value;
                lessor.RentTaxRatio = model.RentTaxRatio.Value;
            }
            
            lessor.RentPaymentPeriodType = model.RentPaymentPeriodType;


            if (model.PaymentMethod == "m")
            {
                lessor.RentPeriod = model.RentPeriod;
            }
            else
            {
                lessor.RentPeriod = model.RentPeriod * 12;
            }

            model.ContractDate =
                DateConversionUtility.ConvertDate(model.ContractDateString, DateConversionUtility.OutputCalendar.Gregorian);


            model.RentStartDate =
                DateConversionUtility.ConvertDate(model.RentStartDateString,
                    DateConversionUtility.OutputCalendar.Gregorian);

            model.RentEndDate =
                DateConversionUtility.ConvertDate(model.RentEndDateString,
                    DateConversionUtility.OutputCalendar.Gregorian);
            
            lessor.ContractDateString = model.ContractDateString;
            lessor.RentStartDateString = model.RentStartDateString;
            lessor.RentEndDateString = model.RentEndDateString;
            
            lessor.ContractDate = model.ContractDate;
            lessor.RentStartDate = model.RentStartDate;
            lessor.RentEndDate = model.RentEndDate;
            
            lessor.ArrivalHallNumber = model.ArrivalHallNumber;
            lessor.City = model.City;

            lessor.RentPaymentPeriodType = model.RentPaymentPeriodType;
            
            if (model.ContractDocumentFile is not null)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
                var uniqueFileName = Guid.NewGuid() + "_" + model.ContractDocumentFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ContractDocumentFile.CopyToAsync(fileStream);
                }

                lessor.ContractDocument = uniqueFileName;
            }

            lessor.User = await GetAuthenticatedUser();

            _unitOfWork.LessorRepository.Update(lessor);

            var isSuccessSave = await _unitOfWork.SaveAsync();

            if (isSuccessSave)
            {
                TempData["lessors-index-temp-msg"] = "edit-success";
                return RedirectToAction(nameof(Index));
            }

            TempData["lessors-index-temp-msg"] = "edit-error";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["lessors-index-temp-msg"] = "unknown-error";
            Log.Error("LessorsController - Edit[POST] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    [Authorize(Permissions.Lessors.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var lessor = await _unitOfWork
                .LessorRepository
                .GetFirstOrDefaultAsync(x => x.Id == id);

            if (lessor is null)
            {
                return NotFound();
            }

            _unitOfWork.LessorRepository.Remove(lessor);

            var isSuccessSave = await _unitOfWork.SaveAsync();

            if (isSuccessSave)
            {
                TempData["lessors-index-temp-msg"] = "delete-success";
                return RedirectToAction(nameof(Index));
            }

            TempData["lessors-index-temp-msg"] = "delete-error";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["lessors-index-temp-msg"] = "delete-error";
            Log.Error("LessorsController - Delete[GET] - {@message}", e.Message);
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
            TempData["lessors-index-temp-msg"] = "unknown-error";
            Log.Error("LessorsController - DownloadFile[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    [Authorize(Permissions.Lessors.View)]
    public async Task<ActionResult> GetLessorsPdf(DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var lessors = await _unitOfWork
                .LessorRepository
                .GetAllAsync(filters: new List<Expression<Func<Lessor, bool>>>()
                {
                    i => fromDate == null || i.ContractDate >= fromDate,
                    i => toDate == null || i.ContractDate <= toDate
                });

            if (!lessors.Any())
            {
                TempData["lessors-index-temp-msg"] = "unknown-error";
                return RedirectToAction(nameof(Index));
            }

            var lessorsDatatable = new DataTable();

            lessorsDatatable.Columns.Add("Name");
            lessorsDatatable.Columns.Add("PropertyType");
            lessorsDatatable.Columns.Add("ContractNumber");
            lessorsDatatable.Columns.Add("TotalRentAmount");
            lessorsDatatable.Columns.Add("RentPeriod");
            lessorsDatatable.Columns.Add("ContractDate");
            lessorsDatatable.Columns.Add("RentStartDate");
            lessorsDatatable.Columns.Add("RentEndDate");
            lessorsDatatable.Columns.Add("City");
            lessorsDatatable.Columns.Add("Index");

            var index = 0;

            foreach (var lessor in lessors)
            {
                index++;
                var row = lessorsDatatable.NewRow();
                row["Name"] = lessor.Name;
                row["PropertyType"] = lessor.PropertyType;
                row["PropertyType"] = lessor.PropertyType;
                row["ContractNumber"] = lessor.ContractNumber;
                row["TotalRentAmount"] = Math.Round(lessor.TotalRentAmount, 2);
                row["RentPeriod"] = lessor.RentPeriod;
                row["ContractDate"] = lessor.ContractDateString;
                row["RentStartDate"] = lessor.RentStartDateString;
                row["RentEndDate"] = lessor.RentEndDateString;
                row["City"] = lessor.City;
                row["Index"] = index;
                lessorsDatatable.Rows.Add(row);
            }

            var lessorsInfoDatatable = new DataTable();
            lessorsInfoDatatable.Columns.Add("PrintDate");
            lessorsInfoDatatable.Columns.Add("User");

            var lessorsInfoDatatableRow = lessorsInfoDatatable.NewRow();
            lessorsInfoDatatableRow["PrintDate"] = DateTime.Now.ToString("yyyy-MM-dd HH:m");
            lessorsInfoDatatableRow["User"] = User.Identity.Name;
            lessorsInfoDatatable.Rows.Add(lessorsInfoDatatableRow);

            var mimetype = String.Empty;
            var extension = 1;

            var reportPath = $"{_webHostEnvironment.WebRootPath}\\reports\\lessors.rdlc";
            var pdfReportName = $"{Guid.NewGuid()}.pdf";

            var localReport = new LocalReport(reportPath);
            localReport.AddDataSource("lessors", lessorsDatatable);
            localReport.AddDataSource("lessorsInfo", lessorsInfoDatatable);
            var result = localReport.Execute(RenderType.Pdf, extension, null, mimetype);


            return File(result.MainStream, "application/pdf", pdfReportName);
        }
        catch (Exception e)
        {
            TempData["lessors-index-temp-msg"] = "unknown-error";
            Log.Error("LessorsController - GetLessorsPdf[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }
    
    private Task<AppUser> GetAuthenticatedUser()
    {
        return _userManager.GetUserAsync(User);
    }
}