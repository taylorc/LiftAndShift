using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Domain.Entities;

public class Exercise : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public MuscleGroup MuscleGroup { get; set; }

    public EquipmentType EquipmentType { get; set; }

    public MovementPattern MovementPattern { get; set; }

    public bool IsCustom { get; set; } = false;

    public string? CreatedByUserId { get; set; }

    public bool IsActive { get; set; } = true;
}
