using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UnRechnung.Models
{
  public class Invoice
  {
    public enum eStatus
    {
      Open,
      Paid
    }

    public int Id { get; set; }
    [Required]
    public string InvoiceNumber { get; set; }

    public int Position { get; set; }

    public long CustomerId { get; set; }
    [Required]
    public Customer Customer { get; set; }

    [Required]
    public DateTime InvoiceDate { get; set; }
    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public eStatus Status { get; set; }
    [Required]
    public decimal TotalAmount { get; set; }

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
  }
}
