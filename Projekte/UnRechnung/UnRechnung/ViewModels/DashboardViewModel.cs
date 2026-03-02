using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;
using System.Linq;
using UnRechnung.Data;
using UnRechnung.Models;
using UnRechnung.Services;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UnRechnung.Interfaces;
using System;

namespace UnRechnung.ViewModels
{
  public partial class DashboardViewModel : ViewModelBase, IAsyncInitializable
  {
    public enum eTheme
    {
      Light,
      Dark
    }

    public enum eYearSelection
    {
      YearBefore,
      CurrentYear
    }

    public ICartesianAxis[] XAxes { get; } =
    [
      new Axis
      {
        Labels =
        [
          "Jan.",
          "Feb.",
          "Mär.",
          "Apr.",
          "Mai.",
          "Jun.",
          "Jul.",
          "Aug.",
          "Sep.",
          "Okt.",
          "Nov.",
          "Dez."
        ],
        MinLimit = 0,
        MaxLimit = 11
      }
    ];
    public ICartesianAxis[] YAxes { get; } =
    [
      new Axis
      {
        Labeler = value => $"{value:N2} €",
        MinLimit = 0
      }
    ];

    [ObservableProperty]
    private eYearSelection _selectedYear = eYearSelection.CurrentYear;
    [ObservableProperty]
    private decimal _totalIncome = 0.00m;
    [ObservableProperty]
    private string _productCount = string.Empty;
    [ObservableProperty]
    private string _customerCount = string.Empty;
    [ObservableProperty]
    private string _invoiceCount = string.Empty;

    public ObservableCollection<Invoice>? OpenInvoices { get; set; }
    public ObservableCollection<ISeries> Series { get; set; } = new ObservableCollection<ISeries>();
    public ObservableCollection<ObservablePoint> IncomePoints { get; set; } = new ObservableCollection<ObservablePoint>();

    private SKColor _greenColor = new SKColor(32, 160, 70);
    private SKColor _greenColorTransparent = new SKColor(32, 160, 70, 70);

    private readonly AppDbContext _dbContext;
    private readonly INavigationService _navigationService;

    /// <summary>Needed for the design view.</summary>
    public DashboardViewModel() { }

    public DashboardViewModel(AppDbContext dbContext, INavigationService navigationService)
    {
      _dbContext = dbContext;
      _navigationService = navigationService;
    }

    public async Task InitializeAsync()
    {
      EnsureNumberRanges();

      Series =
      [
        new LineSeries<ObservablePoint>(IncomePoints)
        {
          Name = "Einnahmen",
          GeometrySize = 2,
          Stroke = new SolidColorPaint(_greenColor, 4),
          GeometryStroke = new SolidColorPaint(_greenColor, 6),
          Fill = new SolidColorPaint(_greenColorTransparent)
        }
      ];

      OpenInvoices = new ObservableCollection<Invoice>(await _dbContext.Invoices.Where(i => (i.Status == Invoice.eStatus.Open)).ToListAsync());
      LoadCustomerNames();

      InvoiceCount = _dbContext.Invoices.Count().ToString("N0");
      CustomerCount = _dbContext.Customers.Count().ToString("N0");
      ProductCount = _dbContext.Products.Count().ToString("N0");

      AddIncomeToChart();
    }

    [RelayCommand]
    private void NewInvoice()
    {
      _navigationService.NavigateTo<InvoiceViewModel>();
    }

    partial void OnSelectedYearChanged(eYearSelection value)
    {
      IncomePoints.Clear();
      TotalIncome = 0.0m;
      AddIncomeToChart();
    }

    private void EnsureNumberRanges()
    {
      if (_dbContext.NumberRanges.Any(n => (n.Type == NumberRange.eType.Customer)) == false)
      {
        var customerNumberRange = new NumberRange();

        customerNumberRange.Type = NumberRange.eType.Customer;
        customerNumberRange.NextNumber = 10_001;
        customerNumberRange.PaddingLength = 0;

        _dbContext.NumberRanges.Add(customerNumberRange);
      }

      if (_dbContext.NumberRanges.Any(n => (n.Type == NumberRange.eType.Invoice)) == false)
      {
        var invoiceNumberRange = new NumberRange();

        invoiceNumberRange.Type = NumberRange.eType.Invoice;
        invoiceNumberRange.Prefix = "RE";
        invoiceNumberRange.Format = "-{yyyy}-{mm}-";
        invoiceNumberRange.NextNumber = 1;
        invoiceNumberRange.PaddingLength = 4;

        _dbContext.NumberRanges.Add(invoiceNumberRange);
      }

      _dbContext.SaveChanges();
    }

    private void AddIncomeToChart()
    {
      int month;
      int selectedYear;
      ObservablePoint existingPoint;

      switch (SelectedYear)
      {
        case eYearSelection.YearBefore: selectedYear = DateTime.Today.Year - 1; break;
        default:
        case eYearSelection.CurrentYear: selectedYear = DateTime.Today.Year; break;
      }

      foreach (var invoice in _dbContext.Invoices.Where(i => (i.Status == Invoice.eStatus.Paid) && (i.InvoiceDate.Year == selectedYear) && (i.InvoiceDate <= DateTime.Now)).ToList())
      {
        month = invoice.DueDate.Month - 1;
        existingPoint = IncomePoints.FirstOrDefault(p => p.X == month);

        if (existingPoint != null)
        {
          existingPoint.Y += decimal.ToDouble(invoice.TotalAmount);
        }
        else
        {
          IncomePoints.Add(new ObservablePoint(month, decimal.ToDouble(invoice.TotalAmount)));
        }

        TotalIncome += invoice.TotalAmount;
      }
    }

    private void LoadCustomerNames()
    {
      Customer? customer;

      foreach (var invoice in OpenInvoices!)
      {
        customer = _dbContext.Customers.Where(c => (c.CustomerNumber == invoice.CustomerId)).FirstOrDefault();

        if (customer != null)
        {
          invoice.Customer.Name = customer.Name;
          invoice.Customer.Representative = customer.Representative;
        }
      }
    }
  }
}
