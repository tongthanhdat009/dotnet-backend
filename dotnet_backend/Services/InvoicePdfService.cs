using dotnet_backend.Database;
using dotnet_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace dotnet_backend.Services;

public class InvoicePdfService : IInvoicePdfService
{
    private readonly ApplicationDbContext _context;

    public InvoicePdfService(ApplicationDbContext context)
    {
        _context = context;
        // Set QuestPDF license (Community license for non-commercial use)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(int orderId)
    {
        // Fetch order with all related data
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Payments)
            .Include(o => o.Promo)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
            throw new Exception("Order not found");

        // Fetch bill information
        var bill = await _context.Bills
            .FirstOrDefaultAsync(b => b.OrderId == orderId);

        // Generate PDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header()
                    .Height(100)
                    .Background(Colors.Green.Lighten3)
                    .Padding(20)
                    .Column(column =>
                    {
                        column.Item().Text("HÓA ĐƠN ĐIỆN TỬ")
                            .FontSize(24)
                            .Bold()
                            .FontColor(Colors.Green.Darken2);
                        
                        column.Item().PaddingTop(5).Text("STORE MANAGER")
                            .FontSize(14)
                            .FontColor(Colors.Green.Darken1);
                    });

                page.Content()
                    .PaddingVertical(20)
                    .Column(column =>
                    {
                        // Invoice Info Section
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Thông tin hóa đơn").Bold().FontSize(14);
                                col.Item().PaddingTop(5).Text($"Mã đơn hàng: {order.OrderId}");
                                col.Item().Text($"Ngày đặt: {order.OrderDate:dd/MM/yyyy HH:mm}");
                                col.Item().Text($"Trạng thái thanh toán: {order.PayStatus}");
                                col.Item().Text($"Trạng thái đơn hàng: {order.OrderStatus}");
                                if (bill != null)
                                {
                                    col.Item().Text($"Mã hóa đơn: {bill.BillId}");
                                    col.Item().Text($"Ngày thanh toán: {bill.PaidAt:dd/MM/yyyy HH:mm}");
                                }
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Thông tin khách hàng").Bold().FontSize(14);
                                col.Item().PaddingTop(5).Text($"Tên: {order.Customer?.Name ?? "N/A"}");
                                col.Item().Text($"Email: {order.Customer?.Email ?? "N/A"}");
                                col.Item().Text($"SĐT: {order.Customer?.Phone ?? "N/A"}");
                            });
                        });

                        column.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Order Items Table
                        column.Item().PaddingTop(20).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);  // STT
                                columns.RelativeColumn(3);   // Tên sản phẩm
                                columns.RelativeColumn(1);   // Số lượng
                                columns.RelativeColumn(1.5f); // Đơn giá
                                columns.RelativeColumn(1.5f); // Thành tiền
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Background(Colors.Green.Lighten3).Text("STT").Bold();
                                header.Cell().Element(CellStyle).Background(Colors.Green.Lighten3).Text("Sản phẩm").Bold();
                                header.Cell().Element(CellStyle).Background(Colors.Green.Lighten3).Text("SL").Bold();
                                header.Cell().Element(CellStyle).Background(Colors.Green.Lighten3).Text("Đơn giá").Bold();
                                header.Cell().Element(CellStyle).Background(Colors.Green.Lighten3).Text("Thành tiền").Bold();

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                                }
                            });

                            // Rows
                            int index = 1;
                            foreach (var item in order.OrderItems)
                            {
                                var subtotal = item.Price * item.Quantity;
                                
                                table.Cell().Element(CellStyle).Text(index.ToString());
                                table.Cell().Element(CellStyle).Text(item.Product?.ProductName ?? "N/A");
                                table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Price:N0} đ");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{subtotal:N0} đ");
                                
                                index++;

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                                }
                            }
                        });

                        // Summary Section
                        column.Item().PaddingTop(20).AlignRight().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.AutoItem().Width(150).Text("Tạm tính:").FontSize(12);
                                row.AutoItem().Width(120).AlignRight().Text($"{order.TotalAmount:N0} đ").FontSize(12);
                            });

                            if (order.DiscountAmount > 0)
                            {
                                col.Item().Row(row =>
                                {
                                    row.AutoItem().Width(150).Text("Giảm giá:");
                                    row.AutoItem().Width(120).AlignRight().Text($"-{order.DiscountAmount:N0} đ").FontColor(Colors.Red.Medium);
                                });
                                
                                if (order.Promo != null)
                                {
                                    col.Item().Row(row =>
                                    {
                                        row.AutoItem().Width(150).Text("Mã khuyến mãi:");
                                        row.AutoItem().Width(120).AlignRight().Text(order.Promo.PromoCode ?? "N/A").FontSize(10);
                                    });
                                }
                            }

                            col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            var finalAmount = (order.TotalAmount ?? 0) - (order.DiscountAmount ?? 0);
                            col.Item().PaddingTop(5).Row(row =>
                            {
                                row.AutoItem().Width(150).Text("TỔNG CỘNG:").Bold().FontSize(14).FontColor(Colors.Green.Darken2);
                                row.AutoItem().Width(120).AlignRight().Text($"{finalAmount:N0} đ").Bold().FontSize(14).FontColor(Colors.Green.Darken2);
                            });

                            // Payment method
                            var payment = order.Payments.FirstOrDefault();
                            if (payment != null)
                            {
                                col.Item().PaddingTop(10).Row(row =>
                                {
                                    row.AutoItem().Width(150).Text("Phương thức:");
                                    row.AutoItem().Width(120).AlignRight().Text(payment.PaymentMethod ?? "N/A");
                                });
                            }
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Cảm ơn quý khách đã mua hàng! ").FontSize(10);
                        text.Span("- Store Manager").FontSize(10).Italic();
                    });
            });
        });

        // Generate PDF bytes
        return document.GeneratePdf();
    }
}
