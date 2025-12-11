namespace dotnet_backend.Dtos
{
    public class CheckoutDto
    {
        public int CustomerId { get; set; }
        public string? PromoCode { get; set; }   // FE gửi code
        // Optional: frontend may send promo id instead of code
        public int? PromoId { get; set; }
        public string? PaymentMethod { get; set; }
    }
}
