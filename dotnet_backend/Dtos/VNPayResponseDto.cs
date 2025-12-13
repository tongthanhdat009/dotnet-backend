namespace dotnet_backend.Dtos
{
    /// <summary>
    /// Response trả về URL thanh toán VNPay
    /// </summary>
    public class VNPayResponseDto
    {
        public bool Success { get; set; }
        public string? PaymentUrl { get; set; }
        public string? Message { get; set; }
    }
}
