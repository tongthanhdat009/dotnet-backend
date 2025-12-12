namespace dotnet_backend.Services.Interface;

public interface IInvoicePdfService
{
    Task<byte[]> GenerateInvoicePdfAsync(int orderId);
}
