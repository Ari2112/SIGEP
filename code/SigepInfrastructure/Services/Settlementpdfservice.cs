using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SigepApplication.DTOs.Settlement;

namespace SigepInfrastructure.Services;

public class SettlementPdfService
{
    public byte[] GenerateSettlementReport(SettlementDto settlement)
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
                        c.Item().Text("Liquidación Laboral — SIGEP")
                         .FontSize(11).FontColor("#ccffcc");
                    });

                    // Datos del colaborador
                    col.Item().PaddingTop(15).Border(1).BorderColor("#dddddd").Padding(10).Column(c =>
                    {
                        c.Item().Text("DATOS DEL COLABORADOR").Bold().FontColor("#2d5a1b");
                        c.Item().PaddingTop(5).Text($"Nombre: {settlement.EmployeeName}");
                        c.Item().Text($"Tipo de terminación: {settlement.TerminationType}");
                        c.Item().Text($"Fecha de contratación: {settlement.HireDate:dd/MM/yyyy}   |   Fecha de terminación: {settlement.TerminationDate:dd/MM/yyyy}");
                        c.Item().Text($"Tiempo trabajado: {settlement.WorkedYears} años, {settlement.WorkedMonths} meses, {settlement.WorkedDays} días");
                        c.Item().Text($"Último salario: ₡{settlement.LastSalary:N0}");
                    });

                    // Desglose de rubros
                    col.Item().PaddingTop(10).Row(row =>
                    {
                        // Rubros a favor del colaborador
                        row.RelativeItem().Border(1).BorderColor("#dddddd").Padding(10).Column(c =>
                        {
                            c.Item().Background("#eafaf1").Padding(5)
                             .Text("RUBROS A PAGAR").Bold().FontColor("#1a6b1a");

                            c.Item().PaddingTop(5)
                             .Text($"Vacaciones pendientes ({settlement.PendingVacationDays:N0} días):   ₡{settlement.VacationAmount:N0}");
                            c.Item().Text($"Aguinaldo proporcional:   ₡{settlement.ProportionalBonus:N0}");
                            if (settlement.NoticeAmount > 0)
                                c.Item().Text($"Preaviso:   ₡{settlement.NoticeAmount:N0}");
                            c.Item().Text($"Cesantía / Auxilio de cesantía:   ₡{settlement.SeveranceAmount:N0}");
                            if (settlement.OtherBenefits > 0)
                                c.Item().Text($"Otros beneficios:   ₡{settlement.OtherBenefits:N0}");

                            c.Item().BorderTop(1).BorderColor("#aaaaaa").PaddingTop(5)
                             .Text($"TOTAL BRUTO:   ₡{settlement.GrossTotal:N0}").Bold();
                        });

                        row.ConstantItem(10);

                        // Deducciones
                        row.RelativeItem().Border(1).BorderColor("#dddddd").Padding(10).Column(c =>
                        {
                            c.Item().Background("#fdf2f2").Padding(5)
                             .Text("DEDUCCIONES").Bold().FontColor("#c0392b");

                            if (settlement.Deductions == null || settlement.Deductions.Count == 0)
                            {
                                c.Item().PaddingTop(5).Text("Sin deducciones adicionales").FontColor("#888888");
                            }
                            else
                            {
                                foreach (var ded in settlement.Deductions)
                                    c.Item().PaddingTop(2).Text($"{ded.Description}:   ₡{ded.Amount:N0}").FontColor("#c0392b");
                            }

                            c.Item().BorderTop(1).BorderColor("#aaaaaa").PaddingTop(5)
                             .Text($"TOTAL DEDUCCIONES:   ₡{settlement.TotalDeductions:N0}").Bold().FontColor("#c0392b");
                        });
                    });

                    // Total neto
                    col.Item().PaddingTop(10).Background("#2d5a1b").Padding(15).Column(c =>
                    {
                        c.Item().Text($"TOTAL NETO A PAGAR:   ₡{settlement.NetTotal:N0}")
                         .Bold().FontSize(14).FontColor(Colors.White);
                    });

                    if (!string.IsNullOrWhiteSpace(settlement.Notes))
                    {
                        col.Item().PaddingTop(8).Text($"Notas: {settlement.Notes}")
                           .FontSize(8).Italic().FontColor("#555555");
                    }

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
                       .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm} — SIGEP   |   Estado: {settlement.Status}")
                       .FontSize(8).FontColor("#888888");
                });
            });
        }).GeneratePdf();
    }
}