using AdventOfCode.Models;
using AdventOfCode.Solutions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AdventOfCode.ViewModels
{
  public partial class MainWindowViewModel : ViewModelBase
  {
    public enum eYear
    {
      y2025,
      y2024,
      y2023,
      y2022,
      y2021,
      y2020,
      y2019,
      y2018,
      y2017,
      y2016,
      y2015
    }

    [ObservableProperty]
    private eYear _selectedYear = eYear.y2025;
    [ObservableProperty]
    private AocDayEntry ?_selectedDay;
    [ObservableProperty]
    private int _selectedRating = 1;
    [ObservableProperty]
    private AocYear? _selectedYearEntry;
    [ObservableProperty]
    private string _input = string.Empty;
    [ObservableProperty]
    private string _output1 = string.Empty;
    [ObservableProperty]
    private string _output2 = string.Empty;

    public ObservableCollection<AocYear> Stats { get; }


    public MainWindowViewModel()
    {
      const int oldestYearOfAOC = 2015; // 2015 was the first year of the adventofcode challanges
      const int firstShortYear = 2025; // 2025 was the first year when adventofcode challanges only offered 12 days
      bool isDecember = (DateTime.Now.Month == 12);
      int maxYear = (isDecember) ? (DateTime.Now.Year - oldestYearOfAOC + 1) : (DateTime.Now.Year - oldestYearOfAOC);

      Stats = new ObservableCollection<AocYear>
        (
          Enumerable.Range(oldestYearOfAOC, maxYear)
            .Reverse()
            .Select(y =>
            {
              int maxDay = (y >= firstShortYear) ? 12 : 25;

              return new AocYear
              {
                Year = y,
                Days = new ObservableCollection<AocDayEntry>
                (
                  Enumerable.Range(1, maxDay)
                  .Reverse()
                  .Select(d => new AocDayEntry
                  {
                    Day = d,
                    Stars = 0
                  }))
              };
            })
        );

      SelectedYearEntry = Stats.FirstOrDefault();

      if (SelectedYearEntry != null)
      {
        SelectedDay = SelectedYearEntry.Days.FirstOrDefault();
      }
    }

    partial void OnSelectedYearChanged(eYear value)
    {
      // Select day one when changing the year
      if (SelectedYearEntry != null)
      {
        SelectedDay = SelectedYearEntry.Days.FirstOrDefault();
      }

      // Show the amount of collected stars for the selected day
      GetSelectedRating();

      // Fetch stats from selected year
      switch (SelectedYear)
      {
        case eYear.y2025: SelectedYearEntry = Stats.FirstOrDefault(e => (e.Year == 2025));
          break;
        case eYear.y2024: SelectedYearEntry = Stats.FirstOrDefault(e => (e.Year == 2024));
          break;
        case eYear.y2023: SelectedYearEntry = Stats.FirstOrDefault(e => (e.Year == 2023));
          break;
        case eYear.y2022: SelectedYearEntry = Stats.FirstOrDefault(e => (e.Year == 2022));
          break;
        case eYear.y2021: SelectedYearEntry = Stats.FirstOrDefault(e => (e.Year == 2021));
          break;
        case eYear.y2020: SelectedYearEntry = Stats.FirstOrDefault(e => (e.Year == 2020));
          break;
        case eYear.y2019: SelectedYearEntry = Stats.FirstOrDefault(e => (e.Year == 2019));
          break;
        case eYear.y2018: SelectedYearEntry = Stats.FirstOrDefault(e => (e.Year == 2018));
          break;
        case eYear.y2017: SelectedYearEntry = Stats.FirstOrDefault(e => (e.Year == 2017));
          break;
        case eYear.y2016: SelectedYearEntry = Stats.FirstOrDefault(e => (e.Year == 2016));
          break;
        case eYear.y2015: SelectedYearEntry = Stats.FirstOrDefault(e => (e.Year == 2015));
          break;
        default:
          break;
      }
    }

    [RelayCommand]
    private void Solve()
    {
      int day = 8;

      Output1 = Answers2025.Solve2025(Input, day, 1);
      Output2 = Answers2025.Solve2025(Input, day, 2);
    }

    private void GetSelectedRating()
    {

    }
  }
}
