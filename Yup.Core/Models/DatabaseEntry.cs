using System.Collections.ObjectModel;

namespace Yup.Core.Models
{
  public class DatabaseEntry
  {
    public string Database { get; set; }
    public string Name { get; set; }
    public ObservableCollection<DatabaseEntry> Children { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpanded { get; set; }
    public EntryType EntryType { get; set; }
  }
}
