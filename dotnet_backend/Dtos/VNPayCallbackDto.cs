namespace dotnet_backend.Dtos
{
    /// <summary>
    /// DTO xử lý callback từ VNPay
    /// </summary>
    public class VNPayCallbackDto
    {
        public bool Success { get; set; }
        public string? TransactionId { get; set; }
        public string? OrderId { get; set; }
        public decimal Amount { get; set; }
        public string? OrderInfo { get; set; }
        public string? ResponseCode { get; set; }
        public string? TransactionStatus { get; set; }
        public string? PayDate { get; set; }
        public string? Message { get; set; }
    }
}
