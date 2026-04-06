using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class CouponUsage
{
    public int Id { get; set; }
    public int CouponId { get; set; }
    public string? UserId { get; set; }
    public int? OrderId { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CouponId))]
    public virtual Coupon? Coupon { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; }
}
