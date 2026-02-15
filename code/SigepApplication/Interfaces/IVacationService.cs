using SigepApplication.DTOs.Vacations;

namespace SigepApplication.Interfaces;

public interface IVacationService
{
    // === SALDO DE VACACIONES ===
    Task<VacationBalanceDto?> GetBalanceAsync(int employeeId, int year);
    Task<IEnumerable<VacationBalanceDto>> GetBalanceHistoryAsync(int employeeId);
    Task<VacationBalanceDto> InitializeBalanceAsync(int employeeId, int year);

    // === SOLICITUDES DE VACACIONES ===
    Task<VacationRequestDto?> GetRequestByIdAsync(int requestId);
    Task<IEnumerable<VacationRequestDto>> GetEmployeeRequestsAsync(int employeeId);
    Task<IEnumerable<VacationRequestDto>> GetPendingRequestsAsync();
    Task<IEnumerable<VacationRequestDto>> GetAllRequestsAsync(VacationRequestFilterDto? filter = null);

    // === ACCIONES DE SOLICITUD ===
    Task<VacationRequestDto> CreateRequestAsync(CreateVacationRequestDto dto, int userId);
    Task<VacationRequestDto> UpdateRequestAsync(int requestId, UpdateVacationRequestDto dto, int userId);
    Task<VacationRequestDto> ApproveRequestAsync(int requestId, int approverUserId, string? comments = null);
    Task<VacationRequestDto> RejectRequestAsync(int requestId, int approverUserId, string reason);
    Task<VacationRequestDto> CancelRequestAsync(int requestId, int userId, string? reason = null);

    // === HISTORIAL ===
    Task<IEnumerable<VacationRequestHistoryDto>> GetRequestHistoryAsync(int requestId);
}
