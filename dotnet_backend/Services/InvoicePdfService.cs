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
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header()
                    .Column(column =>
                    {
                        // Company Header
                        column.Item().BorderBottom(2).BorderColor(Colors.Green.Darken2).PaddingBottom(10).Row(row =>
                        {
                            // Left side - Company info
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("STORE MANAGER")
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor(Colors.Green.Darken2);
                                
                                col.Item().PaddingTop(3).Text("Hệ thống quản lý bán hàng")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                                
                                col.Item().PaddingTop(2).Text("Địa chỉ: Trường Đại học Sài Gòn TP.HCM")
                                    .FontSize(8)
                                    .FontColor(Colors.Grey.Darken1);
                                    
                                col.Item().Text("Hotline: 1900-xxxx | Email: support@storemanager.vn")
                                    .FontSize(8)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                            
                            // Right side - Invoice type
                            row.ConstantItem(150).AlignRight().Column(col =>
                            {
                                col.Item().Background(Colors.Green.Darken2)
                                    .Padding(8)
                                    .AlignCenter()
                                    .Text("PHIẾU XÁC NHẬN")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.White);
                                    
                                col.Item().PaddingTop(5).AlignRight().Text($"#{order.OrderId.ToString().PadLeft(6, '0')}")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Green.Darken2);
                            });
                        });
                        
                        // Invoice title
                        column.Item().PaddingTop(15).AlignCenter()
                            .Text("XÁC NHẬN ĐƠN HÀNG")
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.Green.Darken3);
                    });

                page.Content()
                    .PaddingVertical(15)
                    .Column(column =>
                    {
                        // Customer and Order Info in boxes
                        column.Item().Row(row =>
                        {
                            // Customer Info Box
                            row.RelativeItem().PaddingRight(10).Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Background(Colors.Grey.Lighten4).Padding(12).Column(col =>
                            {
                                col.Item().Text("THÔNG TIN KHÁCH HÀNG").Bold().FontSize(11).FontColor(Colors.Green.Darken2);
                                col.Item().PaddingTop(8).Text($"Họ tên: {order.Name ?? order.Customer?.Name ?? "N/A"}");
                                col.Item().PaddingTop(3).Text($"Email: {order.Email ?? order.Customer?.Email ?? "N/A"}");
                                col.Item().PaddingTop(3).Text($"Điện thoại: {order.Phone ?? order.Customer?.Phone ?? "N/A"}");
                                col.Item().PaddingTop(3).Text($"Địa chỉ: {order.Address ?? order.Customer?.Address ?? "N/A"}");
                            });

                            // Order Info Box
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Background(Colors.Grey.Lighten4).Padding(12).Column(col =>
                            {
                                col.Item().Text("THÔNG TIN ĐỐN HÀNG").Bold().FontSize(11).FontColor(Colors.Green.Darken2);
                                col.Item().PaddingTop(8).Text($"Ngày đặt: {order.OrderDate:dd/MM/yyyy HH:mm}");
                                col.Item().PaddingTop(3).Text($"Loại đơn: {(order.OrderType == "online" ? "Đặt hàng online" : "Tại quầy")}");
                                
                                var statusText = order.OrderStatus switch
                                {
                                    "pending" => "Chờ xác nhận",
                                    "approved" => "Đã duyệt",
                                    "processing" => "Đang xử lý",
                                    "shipping" => "Đang giao hàng",
                                    "delivered" => "Đã giao hàng",
                                    "completed" => "Hoàn thành",
                                    "canceled" => "Đã hủy",
                                    _ => order.OrderStatus ?? "N/A"
                                };
                                col.Item().PaddingTop(3).Text($"Trạng thái: {statusText}");
                                
                                var payment = order.Payments.FirstOrDefault();
                                var paymentMethod = payment?.PaymentMethod switch
                                {
                                    "cash" => "Tiền mặt",
                                    "card" => "Thẻ",
                                    "bank_transfer" => "Chuyển khoản",
                                    "e-wallet" => "Ví điện tử",
                                    _ => payment?.PaymentMethod ?? "N/A"
                                };
                                col.Item().PaddingTop(3).Text($"Thanh toán: {paymentMethod}");
                            });
                        });

                        column.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Order Items Table
                        column.Item().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(35);  // STT
                                columns.RelativeColumn(3);   // Tên sản phẩm
                                columns.RelativeColumn(1);   // Số lượng
                                columns.RelativeColumn(1.3f); // Đơn giá
                                columns.RelativeColumn(1.3f); // Thành tiền
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("STT");
                                header.Cell().Element(HeaderStyle).Text("TÊN SẢN PHẨM");
                                header.Cell().Element(HeaderStyle).AlignCenter().Text("SỐ LƯỢNG");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("ĐƠN GIÁ");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("THÀNH TIỀN");

                                static IContainer HeaderStyle(IContainer container)
                                {
                                    return container
                                        .Border(1)
                                        .BorderColor(Colors.Green.Darken2)
                                        .Background(Colors.Green.Lighten3)
                                        .Padding(8);
                                }
                            });

                            // Rows
                            int index = 1;
                            foreach (var item in order.OrderItems)
                            {
                                var subtotal = item.Price * item.Quantity;
                                var isEven = index % 2 == 0;
                                
                                table.Cell().Element(c => CellStyle(c, isEven)).Text(index.ToString());
                                table.Cell().Element(c => CellStyle(c, isEven)).Text(item.Product?.ProductName ?? "N/A");
                                table.Cell().Element(c => CellStyle(c, isEven)).AlignCenter().Text(item.Quantity.ToString());
                                table.Cell().Element(c => CellStyle(c, isEven)).AlignRight().Text($"{item.Price:N0}");
                                table.Cell().Element(c => CellStyle(c, isEven)).AlignRight().Text($"{subtotal:N0}");
                                
                                index++;

                                static IContainer CellStyle(IContainer container, bool isEven)
                                {
                                    return container
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Background(isEven ? Colors.Grey.Lighten4 : Colors.White)
                                        .Padding(7);
                                }
                            }
                        });

                        // Summary Section with border
                        column.Item().PaddingTop(15).Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Background(Colors.Grey.Lighten4).Padding(12).Row(row =>
                        {
                            row.RelativeItem(); // Spacer
                            
                            row.ConstantItem(280).Column(col =>
                            {
                                col.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Tạm tính:").FontSize(11);
                                    r.ConstantItem(100).AlignRight().Text($"{order.TotalAmount:N0} đ").FontSize(11);
                                });

                                if (order.DiscountAmount > 0)
                                {
                                    col.Item().PaddingTop(5).Row(r =>
                                    {
                                        r.RelativeItem().Text("Giảm giá:").FontSize(11);
                                        r.ConstantItem(100).AlignRight().Text($"-{order.DiscountAmount:N0} đ").FontSize(11).FontColor(Colors.Red.Medium);
                                    });
                                    
                                    if (order.Promo != null)
                                    {
                                        col.Item().PaddingTop(2).Row(r =>
                                        {
                                            r.RelativeItem().Text($"({order.Promo.PromoCode})").FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                                            r.ConstantItem(100);
                                        });
                                    }
                                }

                                col.Item().PaddingTop(8).LineHorizontal(2).LineColor(Colors.Green.Darken2);

                                var finalAmount = (order.TotalAmount ?? 0) - (order.DiscountAmount ?? 0);
                                col.Item().PaddingTop(8).Background(Colors.Green.Lighten3).Padding(8).Row(r =>
                                {
                                    r.RelativeItem().Text("TỔNG CỘNG:").Bold().FontSize(13).FontColor(Colors.Green.Darken3);
                                    r.ConstantItem(100).AlignRight().Text($"{finalAmount:N0} đ").Bold().FontSize(13).FontColor(Colors.Green.Darken3);
                                });
                                
                                col.Item().PaddingTop(3).AlignRight()
                                    .Text($"({ConvertNumberToVietnameseWords(finalAmount)} đồng)")
                                    .FontSize(9)
                                    .Italic()
                                    .FontColor(Colors.Grey.Darken1);
                            });
                        });

                        // Notes section
                        if (order.OrderType == "online")
                        {
                            column.Item().PaddingTop(15).Column(col =>
                            {
                                col.Item().Text("LƯU Ý:").Bold().FontSize(10).FontColor(Colors.Red.Medium);
                                col.Item().PaddingTop(3).Text("• Vui lòng kiểm tra kỹ sản phẩm khi nhận hàng").FontSize(9);
                                col.Item().PaddingTop(2).Text("• Liên hệ hotline trong vòng 24h nếu có vấn đề về đơn hàng").FontSize(9);
                                col.Item().PaddingTop(2).Text("• Giữ lại phiếu này để đổi/trả hàng hoặc bảo hành").FontSize(9);
                            });
                        }
                    });

                page.Footer()
                    .BorderTop(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .PaddingTop(10)
                    .Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Cảm ơn quý khách đã tin tưởng!").FontSize(10).Bold().FontColor(Colors.Green.Darken2);
                                col.Item().PaddingTop(3).Text("Mọi thắc mắc vui lòng liên hệ: 1900-xxxx").FontSize(8);
                            });
                            
                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().AlignRight().Text($"In ngày: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                col.Item().AlignRight().PaddingTop(2).Text("Được tạo bởi Store Manager System").FontSize(7).Italic().FontColor(Colors.Grey.Medium);
                            });
                        });
                    });
            });
        });

        // Generate PDF bytes
        return document.GeneratePdf();
    }
    
    // Helper method to convert number to Vietnamese words
    private string ConvertNumberToVietnameseWords(decimal amount)
    {
        if (amount == 0) return "Không";
        
        string[] ones = { "", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
        string[] tens = { "", "", "hai mươi", "ba mươi", "bốn mươi", "năm mươi", "sáu mươi", "bảy mươi", "tám mươi", "chín mươi" };
        
        long intAmount = (long)amount;
        
        if (intAmount < 10) return CapitalizeFirst(ones[intAmount]);
        if (intAmount < 20)
        {
            return CapitalizeFirst("mười " + (intAmount % 10 == 0 ? "" : ones[intAmount % 10]));
        }
        if (intAmount < 100)
        {
            var ten = intAmount / 10;
            var one = intAmount % 10;
            return CapitalizeFirst(tens[ten] + (one == 0 ? "" : " " + (one == 1 && ten > 1 ? "mốt" : ones[one])));
        }
        if (intAmount < 1000)
        {
            var hundred = intAmount / 100;
            var remainder = intAmount % 100;
            var result = ones[hundred] + " trăm";
            if (remainder > 0)
            {
                if (remainder < 10) result += " lẻ";
                result += " " + ConvertNumberToVietnameseWords(remainder).ToLower();
            }
            return CapitalizeFirst(result);
        }
        if (intAmount < 1000000)
        {
            var thousand = intAmount / 1000;
            var remainder = intAmount % 1000;
            var result = ConvertNumberToVietnameseWords(thousand).ToLower() + " nghìn";
            if (remainder > 0)
            {
                if (remainder < 100) result += " lẻ";
                result += " " + ConvertNumberToVietnameseWords(remainder).ToLower();
            }
            return CapitalizeFirst(result);
        }
        
        var million = intAmount / 1000000;
        var remainderM = intAmount % 1000000;
        var resultM = ConvertNumberToVietnameseWords(million).ToLower() + " triệu";
        if (remainderM > 0)
        {
            if (remainderM < 1000) resultM += " lẻ";
            resultM += " " + ConvertNumberToVietnameseWords(remainderM).ToLower();
        }
        return CapitalizeFirst(resultM);
    }
    
    private string CapitalizeFirst(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return char.ToUpper(text[0]) + text.Substring(1);
    }
}
