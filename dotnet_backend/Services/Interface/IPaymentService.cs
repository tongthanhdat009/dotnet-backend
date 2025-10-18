using dotnet_backend.Services;
using dotnet_backend.Dtos;

namespace dotnet_backend.Services.Interface;

public interface IPaymentService
{
    Task<PaymentDto> CreatePayment(PaymentDto paymentDto);
}