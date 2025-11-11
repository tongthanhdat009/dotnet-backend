using Microsoft.EntityFrameworkCore;
using dotnet_backend.Database;
using dotnet_backend.Services.Interface;
using dotnet_backend.Models;
using dotnet_backend.Dtos;

namespace dotnet_backend.Services;

public class PromotionService : IPromotionService
{
    private readonly ApplicationDbContext _context;

    public PromotionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync()
    {
        var promotions = await _context.Promotions.AsNoTracking().ToListAsync();
        return promotions.Select(p => new PromotionDto
        {
            PromoId = p.PromoId,
            PromoCode = p.PromoCode,
            Description = p.Description,
            DiscountType = p.DiscountType,
            DiscountValue = p.DiscountValue,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            MinOrderAmount = p.MinOrderAmount,
            UsageLimit = p.UsageLimit,
            UsedCount = p.UsedCount,
            Status = p.Status
        });
    }

    public async Task<PromotionDto> GetPromotionByIdAsync(int id)
    {
        var promo = await _context.Promotions.FindAsync(id);
        if (promo == null)
        {
            throw new KeyNotFoundException("Không tìm thấy khuyến mãi với ID/Mã code này.");
        }
        return new PromotionDto
        {
            PromoId = promo.PromoId,
            PromoCode = promo.PromoCode,
            Description = promo.Description,
            DiscountType = promo.DiscountType,
            DiscountValue = promo.DiscountValue,
            StartDate = promo.StartDate,
            EndDate = promo.EndDate,
            MinOrderAmount = promo.MinOrderAmount,
            UsageLimit = promo.UsageLimit,
            UsedCount = promo.UsedCount,
            Status = promo.Status
        };
    }

    public async Task<PromotionDto> CreatePromotionAsync(PromotionDto dto)
    {
        // Basic null check
        if (dto == null)
        {
            throw new ArgumentException("Dữ liệu không hợp lệ.");
        }

        // Validate enums
        var discountType = (dto.DiscountType ?? "").ToLower();
        if (discountType != "percent" && discountType != "fixed")
        {
            throw new ArgumentException("Loại giảm giá không hợp lệ");
        }

        var status = (dto.Status ?? "").ToLower();
        if (status != "active" && status != "inactive")
        {
            throw new ArgumentException("Trạng thái không hợp lệ.");
        }

        // Date validation
        if (dto.EndDate < dto.StartDate)
        {
            throw new ArgumentException("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");
        }

        // Discount value
        if (dto.DiscountValue <= 0)
        {
            throw new ArgumentException("Giá trị giảm giá phải lớn hơn 0.");
        }

        if (discountType == "percent" && (dto.DiscountValue < 1 || dto.DiscountValue > 100))
        {
            throw new ArgumentException("Loại giảm giá 'percent' (phần trăm) phải nằm trong khoảng từ 1 đến 100.");
        }

        // Non-negative fields
        if ((dto.MinOrderAmount ?? 0) < 0)
        {
            throw new ArgumentException("Giá trị đơn hàng tối thiểu không được âm.");
        }
        if ((dto.UsageLimit ?? 0) < 0)
        {
            throw new ArgumentException("Giới hạn sử dụng không được âm.");
        }

        // Unique promo_code
        var exists = await _context.Promotions.AnyAsync(p => p.PromoCode == dto.PromoCode);
        if (exists)
        {
            throw new InvalidOperationException("Mã khuyến mãi đã tồn tại.");
        }

        var model = new Promotion
        {
            PromoCode = dto.PromoCode,
            Description = dto.Description,
            DiscountType = dto.DiscountType!,
            DiscountValue = dto.DiscountValue,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            MinOrderAmount = dto.MinOrderAmount,
            UsageLimit = dto.UsageLimit ?? 0,
            UsedCount = dto.UsedCount ?? 0,
            Status = dto.Status
        };

        _context.Promotions.Add(model);
        await _context.SaveChangesAsync();

        return new PromotionDto
        {
            PromoId = model.PromoId,
            PromoCode = model.PromoCode,
            Description = model.Description,
            DiscountType = model.DiscountType,
            DiscountValue = model.DiscountValue,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            MinOrderAmount = model.MinOrderAmount,
            UsageLimit = model.UsageLimit,
            UsedCount = model.UsedCount,
            Status = model.Status
        };
    }

    public async Task<PromotionDto> UpdatePromotionAsync(int id, PromotionDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentException("Dữ liệu không hợp lệ.");
        }

        var promo = await _context.Promotions.FindAsync(id);
        if (promo == null)
        {
            throw new KeyNotFoundException("Không tìm thấy khuyến mãi với ID/Mã code này.");
        }

        // Unique promo_code check if changed
        if (!string.Equals(promo.PromoCode, dto.PromoCode, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _context.Promotions.AnyAsync(p => p.PromoCode == dto.PromoCode && p.PromoId != id);
            if (exists)
            {
                throw new InvalidOperationException("Mã khuyến mãi đã tồn tại.");
            }
        }

        // Date check
        if (dto.EndDate < dto.StartDate)
        {
            throw new ArgumentException("Không thể đặt ngày kết thúc (end_date) sớm hơn ngày bắt đầu (start_date).");
        }

        // Validate enums
        var discountType = (dto.DiscountType ?? "").ToLower();
        if (discountType != "percent" && discountType != "fixed")
        {
            throw new ArgumentException("Loại giảm giá không hợp lệ");
        }
        var status = (dto.Status ?? "").ToLower();
        if (status != "active" && status != "inactive")
        {
            throw new ArgumentException("Trạng thái không hợp lệ.");
        }

        // Validate numeric
        if (dto.DiscountValue <= 0)
        {
            throw new ArgumentException("Giá trị giảm giá phải lớn hơn 0.");
        }
        if (discountType == "percent" && (dto.DiscountValue < 1 || dto.DiscountValue > 100))
        {
            throw new ArgumentException("Loại giảm giá 'percent' (phần trăm) phải nằm trong khoảng từ 1 đến 100.");
        }
        if ((dto.MinOrderAmount ?? 0) < 0)
        {
            throw new ArgumentException("Giá trị đơn hàng tối thiểu không được âm.");
        }
        if ((dto.UsageLimit ?? 0) < 0)
        {
            throw new ArgumentException("Giới hạn sử dụng không được âm.");
        }

        // Business rules for used_count
        var usedCount = promo.UsedCount ?? 0;
        if ((dto.UsageLimit ?? 0) < usedCount)
        {
            throw new ArgumentException($"Không thể đặt giới hạn sử dụng ({dto.UsageLimit}) thấp hơn số lần đã sử dụng ({usedCount}).");
        }

        if (usedCount > 0)
        {
            // Do not allow changing discount type/value if already used
            if (!string.Equals(promo.DiscountType, dto.DiscountType, StringComparison.OrdinalIgnoreCase) || promo.DiscountValue != dto.DiscountValue)
            {
                throw new ArgumentException("Không thể thay đổi giá trị (discount_value) hoặc loại (discount_type) của khuyến mãi đã được sử dụng.");
            }
        }

        // Apply updates
        promo.PromoCode = dto.PromoCode;
        promo.Description = dto.Description;
        // Only update discount fields if used_count == 0
        if (usedCount == 0)
        {
            promo.DiscountType = dto.DiscountType!;
            promo.DiscountValue = dto.DiscountValue;
        }
        promo.StartDate = dto.StartDate;
        promo.EndDate = dto.EndDate;
        promo.MinOrderAmount = dto.MinOrderAmount;
        promo.UsageLimit = dto.UsageLimit ?? 0;
        promo.Status = dto.Status;

        _context.Promotions.Update(promo);
        await _context.SaveChangesAsync();

        return new PromotionDto
        {
            PromoId = promo.PromoId,
            PromoCode = promo.PromoCode,
            Description = promo.Description,
            DiscountType = promo.DiscountType,
            DiscountValue = promo.DiscountValue,
            StartDate = promo.StartDate,
            EndDate = promo.EndDate,
            MinOrderAmount = promo.MinOrderAmount,
            UsageLimit = promo.UsageLimit,
            UsedCount = promo.UsedCount,
            Status = promo.Status
        };
    }

    public async Task<bool> DeletePromotionAsync(int id)
    {
        var promo = await _context.Promotions.FindAsync(id);
        if (promo == null)
        {
            return false;
        }

        var usedCount = promo.UsedCount ?? 0;
        if (usedCount > 0)
        {
            throw new InvalidOperationException("Không thể xóa khuyến mãi đã được sử dụng. Vui lòng chuyển trạng thái sang 'inactive' (không hoạt động) để vô hiệu hóa.");
        }

        _context.Promotions.Remove(promo);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ApplyPromoResponseDto> ApplyPromotionAsync(ApplyPromoRequestDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.PromoCode))
        {
            throw new ArgumentException("Dữ liệu không hợp lệ.");
        }

        var promo = await _context.Promotions.FirstOrDefaultAsync(p => p.PromoCode == request.PromoCode);
        if (promo == null)
        {
            throw new KeyNotFoundException("Không tìm thấy khuyến mãi với ID/Mã code này.");
        }

        var nowDate = DateOnly.FromDateTime(DateTime.UtcNow);
        if (promo.EndDate < nowDate)
        {
            throw new InvalidOperationException("Mã đã hết hạn sử dụng.");
        }
        if (promo.StartDate > nowDate)
        {
            throw new InvalidOperationException("Mã chưa có hiệu lực.");
        }
        if ((promo.Status ?? "") != "active")
        {
            throw new InvalidOperationException("Mã không còn hoạt động.");
        }
        if ((promo.UsageLimit ?? 0) > 0 && (promo.UsedCount ?? 0) >= (promo.UsageLimit ?? 0))
        {
            throw new InvalidOperationException("Mã đã đạt giới hạn sử dụng.");
        }
        if ((promo.MinOrderAmount ?? 0) > 0 && request.TotalAmount < (promo.MinOrderAmount ?? 0))
        {
            throw new InvalidOperationException("Đơn hàng chưa đạt giá trị tối thiểu.");
        }

        // Compute discount value for response
        decimal discount = 0;
        if (promo.DiscountType == "percent")
            discount = Math.Round(request.TotalAmount * (promo.DiscountValue / 100m), 2);
        else
            discount = promo.DiscountValue;

        return new ApplyPromoResponseDto
        {
            PromoId = promo.PromoId,
            PromoCode = promo.PromoCode,
            Description = promo.Description,
            DiscountType = promo.DiscountType,
            DiscountValue = promo.DiscountValue,
            DiscountAmount = discount,
            MinOrderAmount = promo.MinOrderAmount,
            UsageLimit = promo.UsageLimit,
            UsedCount = promo.UsedCount,
            Status = promo.Status
        };
    }
}