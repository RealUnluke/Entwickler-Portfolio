using System;
using System.ComponentModel.DataAnnotations;

namespace UnRechnung.Models
{
  public class NumberRange
  {
    public enum eType
    {
      Customer,
      Invoice
    }

    public int Id { get; set; }
    public eType Type { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    [Required]
    public long NextNumber { get; set; }
    public int PaddingLength { get; set; }

    public string GetInvoiceNumberString()
    {
      return (Prefix + ParseDate(Format) + NextNumber.ToString($"D{PaddingLength}"));
    }

    private string ParseDate(string format)
    {
      format = format.Replace("{yyyy}", DateTime.Now.ToString("yyyy"));
      format = format.Replace("{yy}", DateTime.Now.ToString("yy"));
      format = format.Replace("{mm}", DateTime.Now.ToString("MM"));
      format = format.Replace("{m}", DateTime.Now.ToString("M"));
      format = format.Replace("{dd}", DateTime.Now.ToString("dd"));
      format = format.Replace("{d}", DateTime.Now.Day.ToString());

      return (format);
    }
  }
}
