using Avalonia.Media.Imaging;
using Docnet.Core.Models;
using Docnet.Core;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia;
using MigraDoc.DocumentObjectModel.Tables;
using UnRechnung.Data;
using UnRechnung.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace UnRechnung.Services
{
  /// <summary>Necessary for Linux and Mac.</summary>
  public class FontResolver : IFontResolver
  {
    /// <summary>Default font.</summary>
    public string DefaultFontName => "Arial";

    private readonly Dictionary<string, byte[]> _fonts = new();

    /// <summary>Get the corresponding font to <paramref name="faceName"/>.</summary>
    /// <param name="faceName">Font name.</param>
    /// <returns>Font.</returns>
    public byte[] GetFont(string faceName)
    {
      if (_fonts.TryGetValue(faceName, out var data))
        return data;

      var assembly = Assembly.GetExecutingAssembly();

      string resourceName = faceName switch
      {
        // Arial
        "Arial#Regular" => "UnRechnung.Resources.Fonts.ARIAL.TTF",
        "Arial#Bold" => "UnRechnung.Resources.Fonts.ARIALBD.TTF",
        "Arial#BoldItalic" => "UnRechnung.Resources.Fonts.ARIALBI.TTF",
        "Arial#Italic" => "UnRechnung.Resources.Fonts.ARIALI.TTF",

        // Courier New
        "Courier#Regular" => "UnRechnung.Resources.Fonts.cour.ttf",
        "Courier#Bold" => "UnRechnung.Resources.Fonts.courbd.ttf",
        "Courier#Italic" => "UnRechnung.Resources.Fonts.couri.ttf",
        "Courier#BoldItalic" => "UnRechnung.Resources.Fonts.courbi.ttf",

        _ => throw new Exception("Unknown font face")
      };

      using var stream = assembly.GetManifestResourceStream(resourceName)
        ?? throw new Exception($"Font resource not found: {resourceName}");

      using var ms = new MemoryStream();
      stream.CopyTo(ms);
      data = ms.ToArray();
      _fonts[faceName] = data;

      return data;
    }

    /// <summary></summary>
    /// <param name="familyName">Font family.</param>
    /// <param name="isBold">Font is bold.</param>
    /// <param name="isItalic">Font is italic.</param>
    /// <returns>Font resolver info.</returns>
    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
      switch (familyName.ToLower())
      {
        case "arial":
          {
            if (!familyName.Equals("Arial", StringComparison.OrdinalIgnoreCase)) return null;
            if (isBold && isItalic) return new FontResolverInfo("Arial#BoldItalic");
            if (isBold) return new FontResolverInfo("Arial#Bold");
            if (isItalic) return new FontResolverInfo("Arial#Italic");
            return new FontResolverInfo("Arial#Regular");
          }
        case "courier new":
          {
            if (isBold && isItalic) return new FontResolverInfo("Courier#BoldItalic");
            if (isBold) return new FontResolverInfo("Courier#Bold");
            if (isItalic) return new FontResolverInfo("Courier#Italic");
            return new FontResolverInfo("Courier#Regular");
          }
      }

      return (new FontResolverInfo("Arial#Regular"));
    }
  }

  /// <summary>Pdf service interface.</summary>
  public interface IPdfService
  {
    /// <summary>
    /// Creates a PDF file on the location of <paramref name="path"/>
    /// with all the data from the invoice with the number <paramref name="invoiceNumber"/>.
    /// </summary>
    /// <param name="path">Destination path for the created file.</param>
    /// <param name="invoiceNumber">Invoice number.</param>
    /// <returns>False, if the file could not be created.</returns>
    bool CreatePdf(string path, string invoiceNumber);

    /// <summary>
    /// Renders a PDF file on the location of <paramref name="pdfPath"/>
    /// and creates a bitmap that can be drawn on the UI.
    /// </summary>
    /// <param name="pdfPath">Location of the file.</param>
    /// <param name="pageNumber">Page number.</param>
    /// <returns>Bitmap that can be drawn on the UI.</returns>
    WriteableBitmap RenderPdfPageAsBitmap(string pdfPath, int pageNumber = 0);
  }

  /// <summary>Provides a service to create and render a PDF file.</summary>
  public class PdfService : IPdfService
  {
    private readonly AppDbContext _dbContext;
    private readonly IJsonConfigService _jsonConfigService;

    public PdfService(AppDbContext dbContext, IJsonConfigService jsonConfigService)
    {
      _dbContext = dbContext;
      _jsonConfigService = jsonConfigService;
    }

    /// <summary>
    /// Creates a PDF file on the location of <paramref name="path"/>
    /// with all the data from the invoice with the number <paramref name="invoiceNumber"/>.
    /// </summary>
    /// <param name="path">Destination path for the created file.</param>
    /// <param name="invoiceNumber">Invoice number.</param>
    /// <returns>False, if the file could not be created.</returns>
    public bool CreatePdf(string path, string invoiceNumber)
    {
      Invoice invoice = _dbContext.Invoices
        .Include(i => i.Customer)
        .Include(i => i.Items)
        .Where(i => i.InvoiceNumber == invoiceNumber)
        .FirstOrDefault();

      var companyConfig = _jsonConfigService.Config.CompanyConfig;

      // (for Linux/macOS) set Font Resolver
      GlobalFontSettings.FontResolver ??= new FontResolver();

      var document = new Document();
      var section = document.AddSection();
      var paragraph = section.AddParagraph();

      // Logo
      var logo = paragraph.AddImage(companyConfig.LogoPath);
      logo.Width = Unit.FromCentimeter(4);
      logo.LockAspectRatio = true;
      paragraph.Format.SpaceAfter = "0.5cm";

      // Customer details + invoice details
      Table infoTable = section.AddTable();
      infoTable.Borders.Visible = false;
      infoTable.AddColumn("9cm"); // left: Customer details
      infoTable.AddColumn("9cm"); // right: Invoice details

      Row infoRow = infoTable.AddRow();

      // Left: Customer details
      var left = infoRow.Cells[0];
      paragraph = left.AddParagraph();
      paragraph.AddText(invoice!.Customer.Name);
      paragraph.AddLineBreak();
      if (!string.IsNullOrEmpty(invoice!.Customer.AddressComplement))
      {
        paragraph.AddText(invoice!.Customer.AddressComplement);
        paragraph.AddLineBreak();
      }
      paragraph.AddText(invoice!.Customer.Street);
      paragraph.AddLineBreak();
      paragraph.AddText($"{invoice!.Customer.PostalCode} {invoice!.Customer.City}");
      paragraph.AddLineBreak();
      paragraph.AddLineBreak();

      // Right: Invoice details
      var right = infoRow.Cells[1];
      paragraph = right.AddParagraph();
      paragraph.Format.Alignment = ParagraphAlignment.Right;
      paragraph.AddText($"Rechnungsnummer: {invoice!.InvoiceNumber}");
      paragraph.AddLineBreak();
      paragraph.AddText($"Datum: {invoice!.InvoiceDate.ToShortDateString()}");
      paragraph.AddLineBreak();
      paragraph.AddText($"Fällig am: {invoice!.DueDate.ToShortDateString()}");

      // Invoice caption
      var title = section.AddParagraph("Rechnung");
      title.Format.Font.Size = 16;
      title.Format.Font.Bold = true;
      title.Format.SpaceAfter = "0.5cm";

      // Table
      Table table = section.AddTable();
      table.AddColumn("2cm");
      table.AddColumn("5cm");
      table.AddColumn("2.5cm");
      table.AddColumn("3cm");
      table.AddColumn("3cm");
      table.AddColumn("2.5cm");

      Row header = table.AddRow();
      header.Shading.Color = Color.Parse("#c4c4c4");
      header.TopPadding = 5;
      header.BottomPadding = 5;
      paragraph = header.Cells[0].AddParagraph("Pos.");
      paragraph.Format.Alignment = ParagraphAlignment.Center;
      paragraph = header.Cells[1].AddParagraph("Leistung");
      paragraph.Format.Alignment = ParagraphAlignment.Left;
      paragraph = header.Cells[2].AddParagraph("Menge");
      paragraph.Format.Alignment = ParagraphAlignment.Right;
      paragraph = header.Cells[3].AddParagraph("Einheit");
      paragraph.Format.Alignment = ParagraphAlignment.Left;
      paragraph = header.Cells[4].AddParagraph("Einzelpreis");
      paragraph.Format.Alignment = ParagraphAlignment.Right;
      paragraph = header.Cells[5].AddParagraph("Gesamt");
      paragraph.Format.Alignment = ParagraphAlignment.Right;

      Row row;

      foreach (var item in invoice!.Items)
      {
        table.AddRow(); // Extra space

        row = table.AddRow();
        paragraph = row.Cells[0].AddParagraph(item.Position.ToString());
        paragraph.Format.Alignment = ParagraphAlignment.Center;
        paragraph = row.Cells[1].AddParagraph(item.Name ?? string.Empty);
        paragraph.Format.Font.Bold = true;
        paragraph.Format.Alignment = ParagraphAlignment.Left;
        paragraph = row.Cells[2].AddParagraph(item.Quantity.ToString());
        paragraph.Format.Alignment = ParagraphAlignment.Right;
        paragraph = row.Cells[3].AddParagraph(item.Unit ?? string.Empty);
        paragraph.Format.Alignment = ParagraphAlignment.Left;
        paragraph = row.Cells[4].AddParagraph(item.UnitPrice.ToString("C2"));
        paragraph.Format.Alignment = ParagraphAlignment.Right;
        paragraph = row.Cells[5].AddParagraph(item.Total.ToString("C2"));
        paragraph.Format.Alignment = ParagraphAlignment.Right;

        row = table.AddRow();
        paragraph = row.Cells[1].AddParagraph(item.Description ?? string.Empty);
      }

      table.AddRow();

      // --- Total amount ---
      row = table.AddRow();
      row.Borders.Top.Width = 1.5;
      row.Cells[0].MergeRight = 3;
      row.TopPadding = 5;
      paragraph = row.Cells[0].AddParagraph("Gesamtbetrag");
      paragraph.Format.Font.Bold = true;
      paragraph = row.Cells[5].AddParagraph(invoice.TotalAmount.ToString("C2"));
      paragraph.Format.Font.Bold = true;
      paragraph.Format.Alignment = ParagraphAlignment.Right;

      // --- Payment information ---
      paragraph = section.AddParagraph();
      paragraph.AddLineBreak();
      paragraph.AddText("Bitte überweisen Sie den Betrag innerhalb von 14 Tagen auf folgendes Konto:");
      paragraph.AddLineBreak();
      paragraph.AddText("IBAN: DE00 0000 0000 0000 0000 00");
      paragraph.AddLineBreak();
      paragraph.AddText("BIC: GENODEF1XXX");

      // Footer
      var footer = section.Footers.Primary.AddParagraph();
      footer.AddText($"{companyConfig.Name} • {companyConfig.Street} • {companyConfig.PostalCode} {companyConfig.City} • HRB 12345");
      footer.AddLineBreak();
      footer.AddText("Geschäftsführung: Max Beispielmann • USt-ID: DE123456789 • www.meinefirma.de");
      footer.Format.Font.Size = 8;
      footer.Format.Alignment = ParagraphAlignment.Center;

      var renderer = new PdfDocumentRenderer()
      {
        Document = document
      };

      renderer.RenderDocument();

      try
      {
        renderer.PdfDocument.Save(path);
        return (true);
      }
      catch (Exception e)
      {
        return (false);
      }
    }

    /// <summary>
    /// Renders a PDF file on the location of <paramref name="pdfPath"/>
    /// and creates a bitmap that can be drawn on the UI.
    /// </summary>
    /// <param name="pdfPath">Location of the file.</param>
    /// <param name="pageNumber">Page number.</param>
    /// <returns>Bitmap that can be drawn on the UI.</returns>
    public WriteableBitmap RenderPdfPageAsBitmap(string pdfPath, int pageNumber = 0)
    {
      using var docReader = DocLib.Instance.GetDocReader(File.ReadAllBytes(pdfPath), new PageDimensions(1080, 1440));
      using var pageReader = docReader.GetPageReader(pageNumber);

      byte[] rgbBytes = pageReader.GetImage(); // RGB raw

      for (int i = 0; i < 5; i++) // 5 Pixel
      {
        int index = i * 3;
        byte r = rgbBytes[index + 0];
        byte g = rgbBytes[index + 1];
        byte b = rgbBytes[index + 2];
      }

      int width = pageReader.GetPageWidth();
      int height = pageReader.GetPageHeight();

      return (ConvertToWriteableBitmap(rgbBytes, width, height));

    }

    /// <summary>Convert PDF to bitmap.</summary>
    /// <param name="rgbaData">RGBA data.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <returns>Bitmap that can be drawn on the UI.</returns>
    private WriteableBitmap ConvertToWriteableBitmap(byte[] rgbaData, int width, int height)
    {
      WriteableBitmap wb = new WriteableBitmap(
        new PixelSize(width, height),
        new Vector(96, 96),
        Avalonia.Platform.PixelFormat.Bgra8888,
        Avalonia.Platform.AlphaFormat.Opaque);

      using (var fb = wb.Lock())
      {
        int srcStride = width * 4;  // 3 bytes per pixel (RGB)
        int dstStride = fb.RowBytes;

        unsafe
        {
          fixed (byte* srcBase = rgbaData)
          {
            byte* dst = (byte*)fb.Address;

            for (int y = 0; y < height; y++)
            {
              byte* srcRow = srcBase + y * srcStride;
              byte* dstRow = dst + y * dstStride;

              Buffer.MemoryCopy(srcRow, dstRow, dstStride, srcStride);
            }
          }
        }
      }

      return (wb);
    }
  }
}
