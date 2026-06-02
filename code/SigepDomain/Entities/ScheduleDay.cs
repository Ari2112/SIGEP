
namespace SigepDomain.Entities;

public class ScheduleDay
{
    public int Id { get; set; }

    public int ScheduleId { get; set; }

    /// <summary>0=Domingo, 1=Lunes, 2=Martes, 3=Miércoles, 4=Jueves, 5=Viernes, 6=Sábado</summary>
    public int DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    /// <summary>True si el turno cruza medianoche (ej: 10pm-6am)</summary>
    public bool CrossesMidnight { get; set; } = false;

    public decimal WorkHours { get; set; } = 8;

    public bool IsActive { get; set; } = true;

    // Navegación
    public Schedule? Schedule { get; set; }
}