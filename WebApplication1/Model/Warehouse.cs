namespace WebApplication1;

public class Warehouse
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedDateTime { get { return DateTime.Now; } }

}