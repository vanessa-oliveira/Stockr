using Stockr.Domain.Enums;

namespace Stockr.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; }
    public string Domain { get; set; }
    public PlanType PlanType { get; set; }
}