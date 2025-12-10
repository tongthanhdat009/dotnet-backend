public class UpdateOrderAndBillStatusDto
{
    public int OrderId { get; set; }
    public string StatusOrder { get; set; } = string.Empty;
    public string StatusBill { get; set; } = string.Empty;
}
