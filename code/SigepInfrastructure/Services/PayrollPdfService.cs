using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SigepApplication.DTOs.Payroll;

namespace SigepInfrastructure.Services;

public class PayrollPdfService
{
    public byte[] GeneratePayrollReport(PayrollDto payroll)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text("CENTRO AGRÍCOLA CANTONAL DE CORONADO")
                       .Bold().FontSize(14);
                    col.Item().Text("Sistema Integral de Gestión de Personal – SIGEP")
                       .FontSize(10).FontColor("#555555");
                    col.Item().Text($"Reporte de Planilla — {payroll.PeriodType}  {payroll.PeriodStartDate:dd/MM/yyyy} al {payroll.PeriodEndDate:dd/MM/yyyy}")
                       .FontSize(10).Bold();
                    col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}  |  Estado: {payroll.Status}")
                       .FontSize(8).FontColor("#888888");
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            var titles = new[] {
                                "Empleado", "Puesto", "Salario Base",
                                "H. Extra", "Salario Bruto",
                                "Ded. Obreras", "Imp. Renta", "Salario Neto"
                            };
                            foreach (var title in titles)
                            {
                                header.Cell()
                                    .Background("#2d5a1b")
                                    .Padding(5)
                                    .Text(title)
                                    .Bold()
                                    .FontColor(Colors.White)
                                    .FontSize(8);
                            }
                        });

                        bool alternate = false;
                        foreach (var d in payroll.Details)
                        {
                            var bg = alternate ? "#f5f5f5" : "#ffffff";
                            alternate = !alternate;

                            var ccss = d.Deductions
                                .Where(x => x.DeductionTypeName.Contains("CCSS") ||
                                            x.DeductionTypeName.Contains("Banco"))
                                .Sum(x => x.Amount);
                            var renta = d.Deductions
                                .Where(x => x.DeductionTypeName.Contains("Renta"))
                                .Sum(x => x.Amount);

                            table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#dddddd").Padding(5)
                                .Text(d.EmployeeName).FontSize(8);
                            table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#dddddd").Padding(5)
                                .Text(d.PositionName ?? "-").FontSize(8);
                            table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#dddddd").Padding(5)
                                .Text($"₡{d.BaseSalary:N0}").FontSize(8);
                            table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#dddddd").Padding(5)
                                .Text(d.OvertimeHours > 0 ? $"{d.OvertimeHours}h / ₡{d.OvertimeAmount:N0}" : "-").FontSize(8);
                            table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#dddddd").Padding(5)
                                .Text($"₡{d.GrossSalary:N0}").FontSize(8);
                            table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#dddddd").Padding(5)
                                .Text($"₡{ccss:N0}").FontSize(8).FontColor("#c0392b");
                            table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#dddddd").Padding(5)
                                .Text(renta > 0 ? $"₡{renta:N0}" : "-").FontSize(8).FontColor("#c0392b");
                            table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#dddddd").Padding(5)
                                .Text($"₡{d.NetSalary:N0}").Bold().FontSize(8).FontColor("#1a6b1a");
                        }
                    });

                    col.Item().PaddingTop(15).Border(1).BorderColor("#2d5a1b").Padding(10).Column(totals =>
                    {
                        totals.Item().Text("RESUMEN DE PLANILLA").Bold().FontSize(10).FontColor("#2d5a1b");
                        totals.Item().PaddingTop(5).Text($"Total empleados: {payroll.TotalEmployees}");
                        totals.Item().Text($"Salario bruto total: ₡{payroll.TotalGrossSalary:N0}");
                        totals.Item().Text($"Total deducciones: ₡{payroll.TotalDeductions:N0}").FontColor("#c0392b");
                        totals.Item().BorderTop(1).BorderColor("#2d5a1b").PaddingTop(5)
                            .Text($"TOTAL NETO A PAGAR: ₡{payroll.TotalNetSalary:N0}").Bold().FontColor("#2d5a1b");
                    });
                });

                page.Footer().Text(text =>
                {
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                    text.Span(" — SIGEP © Centro Agrícola Cantonal de Coronado");
                });
            });
        }).GeneratePdf();
    }

    public byte[] GeneratePayslip(PayrollDto payroll, PayrollDetailDto employee)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(col =>
                {
                    // Encabezado verde
                    col.Item().Background("#2d5a1b").Padding(15).Column(c =>
                    {
                        c.Item().Text("CENTRO AGRÍCOLA CANTONAL DE CORONADO")
                         .Bold().FontSize(14).FontColor(Colors.White);
                        c.Item().Text("Colilla de Pago — SIGEP")
                         .FontSize(11).FontColor("#ccffcc");
                    });

                    // Datos del empleado
                    col.Item().PaddingTop(15).Border(1).BorderColor("#dddddd").Padding(10).Column(c =>
                    {
                        c.Item().Text("DATOS DEL COLABORADOR").Bold().FontColor("#2d5a1b");
                        c.Item().PaddingTop(5).Text($"Nombre: {employee.EmployeeName}");
                        c.Item().Text($"Puesto: {employee.PositionName ?? "-"}");
                        c.Item().Text($"Período: {payroll.PeriodType}  |  {payroll.PeriodStartDate:dd/MM/yyyy} al {payroll.PeriodEndDate:dd/MM/yyyy}");
                    });

                    col.Item().PaddingTop(10).Row(row =>
                    {
                        // Ingresos
                        row.RelativeItem().Border(1).BorderColor("#dddddd").Padding(10).Column(c =>
                        {
                            c.Item().Background("#eafaf1").Padding(5)
                             .Text("INGRESOS").Bold().FontColor("#1a6b1a");
                            c.Item().PaddingTop(5).Text($"Salario base:   ₡{employee.BaseSalary:N0}");
                            if (employee.OvertimeHours > 0)
                                c.Item().Text($"Horas extra ({employee.OvertimeHours}h):   ₡{employee.OvertimeAmount:N0}");
                            c.Item().BorderTop(1).BorderColor("#aaaaaa").PaddingTop(5)
                             .Text($"SALARIO BRUTO:   ₡{employee.GrossSalary:N0}").Bold();
                        });

                        row.ConstantItem(10);

                        // Deducciones
                        row.RelativeItem().Border(1).BorderColor("#dddddd").Padding(10).Column(c =>
                        {
                            c.Item().Background("#fdf2f2").Padding(5)
                             .Text("DEDUCCIONES").Bold().FontColor("#c0392b");

                            if (employee.Deductions == null || employee.Deductions.Count == 0)
                            {
                                c.Item().PaddingTop(5).Text("Sin deducciones").FontColor("#888888");
                            }
                            else
                            {
                                foreach (var ded in employee.Deductions)
                                    c.Item().PaddingTop(2).Text($"{ded.DeductionTypeName}:   ₡{ded.Amount:N0}").FontColor("#c0392b");
                            }

                            c.Item().BorderTop(1).BorderColor("#aaaaaa").PaddingTop(5)
                             .Text($"TOTAL DEDUCCIONES:   ₡{employee.TotalDeductions:N0}").Bold().FontColor("#c0392b");
                        });
                    });

                    // Salario neto
                    col.Item().PaddingTop(10).Background("#2d5a1b").Padding(15).Column(c =>
                    {
                        c.Item().Text($"SALARIO NETO A RECIBIR:   ₡{employee.NetSalary:N0}")
                         .Bold().FontSize(14).FontColor(Colors.White);
                    });

                    // Firmas
                    col.Item().PaddingTop(50).Row(row =>
                    {
                        row.RelativeItem().BorderTop(1).BorderColor("#333333").PaddingTop(5)
                           .Text("Firma del Colaborador").FontSize(9);
                        row.ConstantItem(40);
                        row.RelativeItem().BorderTop(1).BorderColor("#333333").PaddingTop(5)
                           .Text("Firma de RRHH").FontSize(9);
                    });

                    col.Item().PaddingTop(20)
                       .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm} — SIGEP")
                       .FontSize(8).FontColor("#888888");
                });
            });
        }).GeneratePdf();
    }
}