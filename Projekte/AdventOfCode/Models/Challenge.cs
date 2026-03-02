using System.Collections.ObjectModel;

namespace AdventOfCode.Models
{
  public class AocYear
  {
    public int Year { get; set; }
    public ObservableCollection<AocDayEntry> Days { get; set; }

    public override string ToString()
    {
      return (Year.ToString());
    }
  }

  public class AocDayEntry
  {
    public int Day { get; set; }
    public int Stars { get; set; }

    public override string ToString()
    {
      return (Day.ToString());
    }
  }
}
