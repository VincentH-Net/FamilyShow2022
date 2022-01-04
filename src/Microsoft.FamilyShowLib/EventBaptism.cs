using System;

namespace Microsoft.FamilyShowLib
{
  [Serializable]
  public class EventBaptism
  {
    private DateTime? _baptismDate;
    private string _baptismPlace;

    public DateTime? BaptismDate
    {
      get => _baptismDate;
      set => _baptismDate = value;
    }

    public string BaptismPlace
    {
      get => _baptismPlace;
      set => _baptismPlace = value;
    }
  }
}
