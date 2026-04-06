namespace EssenzStore.Models.ViewModels;

public class OrderListViewModel
{
    public List<Order> Orders { get; set; } = new();
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; }
}

public class OrderDetailViewModel
{
    public Order Order { get; set; } = null!;
    public Shipment? Shipment { get; set; }
    public List<TrackingEvent> TrackingEvents { get; set; } = new();
    public Payment? Payment { get; set; }
}

public class TrackingViewModel
{
    public string? CodigoRastreio { get; set; }
    public string? NumeroPedido { get; set; }
    public Shipment? Shipment { get; set; }
    public List<TrackingEvent> Events { get; set; } = new();
    public Order? Order { get; set; }
}
