using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SigepApplication.DTOs.Payroll;
using SigepApplication.Interfaces;
using SigepInfrastructure.Services;
using System.Security.Claims;

namespace SigepAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin,Administrador,RRHH")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;
    private readonly PayrollPdfService _pdfService;
    private readonly ILogger<PayrollController> _logger;

    public PayrollController(
        IPayrollService payrollService,
        PayrollPdfService pdfService,
        ILogger<PayrollController> logger)
    {
        _payrollService = payrollService;
        _pdfService = pdfService;
        _logger = logger;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Obtiene todas las planillas (HU-5.1)</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PayrollDto>>> GetAll()
    {
        var payrolls = await _payrollService.GetAllAsync();
        return Ok(payrolls);
    }

    /// <summary>Obtiene una planilla por ID con detalles (HU-5.1)</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PayrollDto>> GetById(int id)
    {
        var payroll = await _payrollService.GetByIdAsync(id);
        if (payroll == null)
            return NotFound(new { message = "Planilla no encontrada" });
        return Ok(payroll);
    }

    /// <summary>Genera PDF general de planilla (HU-10.3)</summary>
    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DownloadPayrollPdf(int id)
    {
        var payroll = await _payrollService.GetByIdAsync(id);
        if (payroll == null)
            return NotFound(new { message = "Planilla no encontrada" });

        var bytes = _pdfService.GeneratePayrollReport(payroll);
        var filename = $"Planilla_{payroll.PeriodStartDate:yyyy-MM}_{payroll.PeriodType}.pdf";
        return File(bytes, "application/pdf", filename);
    }

    /// <summary>Genera PDF de colilla individual por empleado (HU-10.3)</summary>
    [HttpGet("{id}/pdf/employee/{employeeId}")]
    public async Task<IActionResult> DownloadEmployeePayslip(int id, int employeeId)
    {
        var payroll = await _payrollService.GetByIdAsync(id);
        if (payroll == null)
            return NotFound(new { message = "Planilla no encontrada" });

        var employee = payroll.Details.FirstOrDefault(d => d.EmployeeId == employeeId);
        if (employee == null)
            return NotFound(new { message = "Empleado no encontrado en esta planilla" });

        var bytes = _pdfService.GeneratePayslip(payroll, employee);
        var filename = $"Colilla_{employee.EmployeeName?.Replace(" ", "_")}_{payroll.PeriodStartDate:yyyy-MM}.pdf";
        return File(bytes, "application/pdf", filename);
    }

    /// <summary>Genera una planilla quincenal/mensual (HU-5.1)</summary>
    [HttpPost("generate")]
    public async Task<ActionResult<PayrollDto>> Generate([FromBody] CreatePayrollDto dto)
    {
        try
        {
            var payroll = await _payrollService.GenerateAsync(dto, GetUserId());
            return Ok(payroll);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Aprueba una planilla (HU-5.2)</summary>
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<PayrollDto>> Approve(int id, [FromBody] ApprovePayrollDto dto)
    {
        try
        {
            var payroll = await _payrollService.ApproveAsync(id, GetUserId(), dto.Notes);
            return Ok(payroll);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Anula una planilla (HU-5.4)</summary>
    [HttpPost("{id}/annul")]
    [Authorize(Roles = "Admin,Administrador")]
    public async Task<ActionResult<PayrollDto>> Annul(int id, [FromBody] ApprovePayrollDto dto)
    {
        try
        {
            var payroll = await _payrollService.AnnulAsync(id, GetUserId(), dto.Notes);
            return Ok(payroll);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Obtiene tipos de deducciones configurados</summary>
    [HttpGet("deduction-types")]
    public async Task<ActionResult<IEnumerable<DeductionTypeDto>>> GetDeductionTypes()
    {
        var types = await _payrollService.GetDeductionTypesAsync();
        return Ok(types);
    }

    /// <summary>Obtiene tipos de beneficios configurados</summary>
    [HttpGet("benefit-types")]
    public async Task<ActionResult<IEnumerable<BenefitTypeDto>>> GetBenefitTypes()
    {
        var types = await _payrollService.GetBenefitTypesAsync();
        return Ok(types);
    }
}
