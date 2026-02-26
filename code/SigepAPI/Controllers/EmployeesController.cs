using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SigepApplication.DTOs.Employees;
using SigepApplication.Interfaces;
using System.Security.Claims;

namespace SigepAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        var employees = await _employeeService.GetAllAsync();
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null)
            return NotFound(new { message = "Empleado no encontrado" });
        return Ok(employee);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,RRHH")]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
    {
        try
        {
            var employee = await _employeeService.CreateAsync(dto, GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,RRHH")]
    public async Task<ActionResult<EmployeeDto>> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        try
        {
            var employee = await _employeeService.UpdateAsync(id, dto, GetUserId());
            return Ok(employee);
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

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await _employeeService.DeactivateAsync(id, GetUserId());
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("positions")]
    public async Task<ActionResult<IEnumerable<PositionDto>>> GetPositions()
    {
        var positions = await _employeeService.GetAllPositionsAsync();
        return Ok(positions);
    }

    [HttpPost("positions")]
    [Authorize(Roles = "Admin,RRHH")]
    public async Task<ActionResult<PositionDto>> CreatePosition([FromBody] CreatePositionDto dto)
    {
        var position = await _employeeService.CreatePositionAsync(dto);
        return Ok(position);
    }

    [HttpGet("schedules")]
    public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetSchedules()
    {
        var schedules = await _employeeService.GetAllSchedulesAsync();
        return Ok(schedules);
    }

    [HttpPost("schedules")]
    [Authorize(Roles = "Admin,RRHH")]
    public async Task<ActionResult<ScheduleDto>> CreateSchedule([FromBody] CreateScheduleDto dto)
    {
        var schedule = await _employeeService.CreateScheduleAsync(dto);
        return Ok(schedule);
    }
}
