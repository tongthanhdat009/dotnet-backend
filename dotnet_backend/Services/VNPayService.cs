using dotnet_backend.Services.Interface;
using dotnet_backend.Dtos;
using dotnet_backend.Database;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace dotnet_backend.Services
{
    public class VNPayService : IVNPayService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public VNPayService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public string CreatePaymentUrl(VNPayRequestDto request, string ipAddress)
        {
            var vnpayConfig = _configuration.GetSection("VNPay");
            var tmnCode = vnpayConfig["TmnCode"];
            var hashSecret = vnpayConfig["HashSecret"];
            var baseUrl = vnpayConfig["Url"];
            var returnUrl = request.ReturnUrl ?? vnpayConfig["ReturnUrl"];

            var vnpay = new VNPayLibrary();
            
            // Thêm các tham số bắt buộc
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", tmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString()); // VNPay yêu cầu số tiền * 100
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", request.OrderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other"); // Loại hàng hóa
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", request.OrderId.ToString()); // Mã đơn hàng

            // Tạo URL thanh toán
            string paymentUrl = vnpay.CreateRequestUrl(baseUrl, hashSecret);
            return paymentUrl;
        }

        public async Task<VNPayCallbackDto> ProcessCallbackAsync(Dictionary<string, string> vnpayData)
        {
            var result = new VNPayCallbackDto();

            try
            {
                // Lấy các tham số từ VNPay
                var vnp_ResponseCode = vnpayData.GetValueOrDefault("vnp_ResponseCode");
                var vnp_TransactionStatus = vnpayData.GetValueOrDefault("vnp_TransactionStatus");
                var vnp_TxnRef = vnpayData.GetValueOrDefault("vnp_TxnRef");
                var vnp_Amount = vnpayData.GetValueOrDefault("vnp_Amount");
                var vnp_OrderInfo = vnpayData.GetValueOrDefault("vnp_OrderInfo");
                var vnp_TransactionNo = vnpayData.GetValueOrDefault("vnp_TransactionNo");
                var vnp_PayDate = vnpayData.GetValueOrDefault("vnp_PayDate");
                var vnp_SecureHash = vnpayData.GetValueOrDefault("vnp_SecureHash");

                // Validate signature
                if (!ValidateSignature(vnpayData, vnp_SecureHash))
                {
                    result.Success = false;
                    result.Message = "Chữ ký không hợp lệ";
                    return result;
                }

                // Kiểm tra response code
                if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                {
                    result.Success = true;
                    result.Message = "Thanh toán thành công";
                    
                    // Cập nhật trạng thái đơn hàng và bill
                    if (int.TryParse(vnp_TxnRef, out int orderId))
                    {
                        var order = await _context.Orders.FindAsync(orderId);
                        if (order != null)
                        {
                            order.PayStatus = "paid";
                            
                            // Cập nhật Bill
                            var bill = await _context.Bills.FirstOrDefaultAsync(b => b.OrderId == orderId);
                            if (bill != null)
                            {
                                bill.PayStatus = "paid";
                                bill.PaidAt = DateTime.Now;
                            }
                            
                            // Cập nhật Payment
                            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
                            if (payment != null)
                            {
                                payment.TransactionStatus = "success";
                            }
                            
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    result.Success = false;
                    result.Message = GetResponseMessage(vnp_ResponseCode);
                }

                // Fill thông tin vào result
                result.TransactionId = vnp_TransactionNo;
                result.OrderId = vnp_TxnRef;
                result.Amount = decimal.Parse(vnp_Amount ?? "0") / 100;
                result.OrderInfo = vnp_OrderInfo;
                result.ResponseCode = vnp_ResponseCode;
                result.TransactionStatus = vnp_TransactionStatus;
                result.PayDate = vnp_PayDate;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Lỗi xử lý: {ex.Message}";
            }

            return result;
        }

        public bool ValidateSignature(Dictionary<string, string> vnpayData, string secureHash)
        {
            var hashSecret = _configuration.GetSection("VNPay")["HashSecret"];
            
            var vnpay = new VNPayLibrary();
            foreach (var kvp in vnpayData)
            {
                if (!kvp.Key.StartsWith("vnp_") || kvp.Key == "vnp_SecureHash" || kvp.Key == "vnp_SecureHashType")
                    continue;
                vnpay.AddResponseData(kvp.Key, kvp.Value);
            }

            string calculatedHash = vnpay.CreateResponseHash(hashSecret);
            return calculatedHash.Equals(secureHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string GetResponseMessage(string? responseCode)
        {
            return responseCode switch
            {
                "00" => "Giao dịch thành công",
                "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường)",
                "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng",
                "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
                "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch",
                "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa",
                "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP)",
                "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
                "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch",
                "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày",
                "75" => "Ngân hàng thanh toán đang bảo trì",
                "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định",
                _ => "Giao dịch thất bại"
            };
        }
    }

    /// <summary>
    /// Library xử lý VNPay
    /// </summary>
    public class VNPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VNPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VNPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            var data = new StringBuilder();
            foreach (var kvp in _requestData)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    data.Append(WebUtility.UrlEncode(kvp.Key) + "=" + WebUtility.UrlEncode(kvp.Value) + "&");
                }
            }

            string queryString = data.ToString();
            if (queryString.Length > 0)
            {
                queryString = queryString.Remove(queryString.Length - 1, 1);
            }

            string signData = queryString;
            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);
            
            return $"{baseUrl}?{queryString}&vnp_SecureHash={vnp_SecureHash}";
        }

        public string CreateResponseHash(string vnp_HashSecret)
        {
            var data = new StringBuilder();
            foreach (var kvp in _responseData)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    data.Append(WebUtility.UrlEncode(kvp.Key) + "=" + WebUtility.UrlEncode(kvp.Value) + "&");
                }
            }

            string signData = data.ToString();
            if (signData.Length > 0)
            {
                signData = signData.Remove(signData.Length - 1, 1);
            }

            return HmacSHA512(vnp_HashSecret, signData);
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var b in hashValue)
                {
                    hash.Append(b.ToString("x2"));
                }
            }

            return hash.ToString();
        }
    }

    public class VNPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
