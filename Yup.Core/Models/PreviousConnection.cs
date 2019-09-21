namespace Yup.Core.Models
{
  public class PreviousConnection
  {
    public string KeyName { get; set; }
    public string MongoUrl { get; set; }

    public bool IsActive { get; set; }
  }
}
