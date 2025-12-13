namespace dotnet_backend.Dtos
{
    /// <summary>
    /// Request tạo thanh toán VNPay
    /// </summary>
    public class VNPayRequestDto
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
    }
}
