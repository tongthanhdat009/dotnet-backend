namespace dotnet_backend.Dtos
{
    public class CheckoutDto
    {
        public int CustomerId { get; set; }
        public string? PromoCode { get; set; }   // FE gửi code
        // Optional: frontend may send promo id instead of code
        public int? PromoId { get; set; }
        public string? PaymentMethod { get; set; }
        
        // Customer information
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
    }
}
