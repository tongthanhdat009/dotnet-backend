using dotnet_backend.Dtos;
using System.Threading.Tasks;

namespace dotnet_backend.Services.Interface
{
    public interface IVNPayService
    {
        /// <summary>
        /// Tạo URL thanh toán VNPay
        /// </summary>
        string CreatePaymentUrl(VNPayRequestDto request, string ipAddress);

        /// <summary>
        /// Xử lý callback từ VNPay
        /// </summary>
        Task<VNPayCallbackDto> ProcessCallbackAsync(Dictionary<string, string> vnpayData);

        /// <summary>
        /// Verify signature từ VNPay
        /// </summary>
        bool ValidateSignature(Dictionary<string, string> vnpayData, string secureHash);
    }
}
