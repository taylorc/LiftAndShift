namespace LiftAndShift.Core.FamilyMemberAggregate;

public class FamilyMember(FamilyMemberName name, Pin pin) : EntityBase<FamilyMember, FamilyMemberId>, IAggregateRoot
{
  public FamilyMemberName Name { get; private set; } = name;
  public Pin Pin { get; private set; } = pin;

  public FamilyMember UpdateName(FamilyMemberName newName)
  {
    Name = newName;
    return this;
  }

  public FamilyMember UpdatePin(Pin newPin)
  {
    Pin = newPin;
    return this;
  }
}
