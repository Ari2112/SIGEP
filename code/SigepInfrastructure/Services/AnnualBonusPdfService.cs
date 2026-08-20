using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SigepApplication.DTOs.AnnualBonus;

namespace SigepInfrastructure.Services;

public class AnnualBonusPdfService
{
    public byte[] GenerateAnnualBonusReport(AnnualBonusDto bonus)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Content().Column(col =>
                {
                    // Encabezado verde
                    col.Item().Background("#2d5a1b").Padding(15).Column(c =>
                    {
                        c.Item().Text("CENTRO AGRÍCOLA CANTONAL DE CORONADO")
                         .Bold().FontSize(14).FontColor(Colors.White);
                        c.Item().Text($"Aguinaldo {bonus.Year} — SIGEP")
                         .FontSize(11).FontColor("#ccffcc");
                    });

                    // Datos generales
                    col.Item().PaddingTop(15).Border(1).BorderColor("#dddddd").Padding(10).Column(c =>
                    {
                        c.Item().Text("DATOS DEL CÁLCULO").Bold().FontColor("#2d5a1b");
                        c.Item().PaddingTop(5).Text($"Año: {bonus.Year}");
                        c.Item().Text($"Período legal: {bonus.PeriodStartDate:dd/MM/yyyy} — {bonus.PeriodEndDate:dd/MM/yyyy}");
                        c.Item().Text($"Estado: {bonus.Status}");
                        c.Item().Text($"Total de empleados: {bonus.TotalEmployees}");
                        c.Item().Text($"Calculado por: {bonus.CalculatedByName}   |   Fecha: {bonus.CalculatedAt:dd/MM/yyyy HH:mm}");
                        if (!string.IsNullOrWhiteSpace(bonus.ApprovedByName))
                            c.Item().Text($"Aprobado por: {bonus.ApprovedByName}   |   Fecha: {bonus.ApprovedAt:dd/MM/yyyy HH:mm}");
                        if (!string.IsNullOrWhiteSpace(bonus.Notes))
                            c.Item().Text($"Notas: {bonus.Notes}").FontSize(8).Italic().FontColor("#555555");
                    });

                    // Tabla de detalle por empleado
                    col.Item().PaddingTop(12).Text("DETALLE POR EMPLEADO").Bold().FontColor("#2d5a1b");

                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);   // Empleado
                            columns.RelativeColumn(2);   // Puesto
                            columns.RelativeColumn(1.3f);  // Meses
                            columns.RelativeColumn(2);   // Salario promedio
                            columns.RelativeColumn(2);   // Proporcional
                            columns.RelativeColumn(2);   // Neto
                        });

                        table.Header(header =>
                        {
                            void HeaderCell(string text) =>
                                header.Cell().Background("#2d5a1b").Padding(5)
                                    .Text(text).Bold().FontColor(Colors.White).FontSize(8);

                            HeaderCell("Empleado");
                            HeaderCell("Puesto");
                            HeaderCell("Meses trab.");
                            HeaderCell("Salario base");
                            HeaderCell("Proporcional");
                            HeaderCell("Neto");
                        });

                        bool alternate = false;
                        foreach (var d in bonus.Details)
                        {
                            var bg = alternate ? "#f5f9f3" : "#FFFFFF";
                            alternate = !alternate;

                            table.Cell().Background(bg).Padding(5).Text(d.EmployeeName).FontSize(8);
                            table.Cell().Background(bg).Padding(5).Text(d.PositionName ?? "-").FontSize(8);
                            table.Cell().Background(bg).Padding(5).Text($"{d.WorkedMonths} meses").FontSize(8);
                            table.Cell().Background(bg).Padding(5).Text($"₡{d.AverageSalary:N0}").FontSize(8);
                            table.Cell().Background(bg).Padding(5).Text($"₡{d.ProportionalAmount:N0}").FontSize(8);
                            table.Cell().Background(bg).Padding(5).Text($"₡{d.NetAmount:N0}").Bold().FontSize(8);
                        }
                    });

                    // Total
                    col.Item().PaddingTop(12).Background("#2d5a1b").Padding(15).Column(c =>
                    {
                        c.Item().Text($"TOTAL A PAGAR:   ₡{bonus.TotalAmount:N0}")
                         .Bold().FontSize(14).FontColor(Colors.White);
                    });

                    // Firmas
                    col.Item().PaddingTop(50).Row(row =>
                    {
                        row.RelativeItem().BorderTop(1).BorderColor("#333333").PaddingTop(5)
                           .Text("Firma de RRHH").FontSize(9);
                        row.ConstantItem(40);
                        row.RelativeItem().BorderTop(1).BorderColor("#333333").PaddingTop(5)
                           .Text("Firma de Gerencia").FontSize(9);
                    });

                    col.Item().PaddingTop(20)
                       .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm} — SIGEP   |   Estado: {bonus.Status}")
                       .FontSize(8).FontColor("#888888");
                });
            });
        }).GeneratePdf();
    }
}