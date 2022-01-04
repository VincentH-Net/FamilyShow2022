using System;

namespace Microsoft.FamilyShowLib
{
  [Serializable]
  public class Address
  {
    #region Fields and Constants
    private string address1;
    private string address2;
    private string city;
    private string country;
    private string zipCode;
    #endregion

    #region Properties
    public string Address1
    {
      get => address1;
      set => address1 = value;
    }

    public string Address2
    {
      get => address2;
      set => address2 = value;
    }

    public string City
    {
      get => city;
      set => city = value;
    }

    public string Country
    {
      get => country;
      set => country = value;
    }

    public string ZipCode
    {
      get => zipCode;
      set => zipCode = value;
    }
    #endregion
  }
}
