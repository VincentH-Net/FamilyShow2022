using System;

namespace Microsoft.FamilyShowLib
{
  [Serializable]
  public class Contact
  {
    private string mail;
    private Address address;
    private string phone;

    public string Mail
    {
      get => mail;
      set => mail = value;
    }

    public Address Address
    {
      get => address;
      set => address = value;
    }

    public string Phone
    {
      get => phone;
      set => phone = value;
    }
  }
}
