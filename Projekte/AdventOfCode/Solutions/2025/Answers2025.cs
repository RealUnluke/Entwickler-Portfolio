using AdventOfCode.Resources;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using SkiaSharp;
using AdventOfCode.Solutions._2025;

namespace AdventOfCode.Solutions
{
  public class Answers2025
  {
    /// <param name="day">1-12</param>
    /// <param name="part">1 or 2</param>
    /// <returns>Corresponding answer</returns>
    public static string Solve2025(string input, int day, int part)
    {
      switch (day)
      {
        case 1: return (Day1(input, part));
        case 2: return (Day2(input, part));
        case 3: return (Day3(input, part));
        case 4: return (Day4(input, part));
        case 5: return (Day5(input, part));
        case 6: return (Day6(input, part));
        case 7: return (Day7(input, part));
        case 8: return (Day8(input, part));
        case 9: return (BLANKO(input, part));
        case 10: return (BLANKO(input, part));
        case 11: return (BLANKO(input, part));
        case 12: return (BLANKO(input, part));
        default: return ("There is no such day in year 2025 of AOC. :c");
      }
    }

    #region Day 1
    public static string Day1(string input, int part)
    {
      switch (part)
      {
        case 1:
          {
            int zeroCount = 0;
            var puzzle = input
              .Replace('L', '-')
              .Replace('R', '+')
              .Split(Environment.NewLine);

            foreach (var line in puzzle)
            {
              int currentDialPosition = 50;
              int dialMovement = 0;

              if (Int32.TryParse(line, out dialMovement))
              {
                currentDialPosition += dialMovement;

                if ((currentDialPosition > 99) || (currentDialPosition < 0))
                {
                  currentDialPosition = Helpers2025.BetterMod(currentDialPosition, 100);
                }

                if (currentDialPosition == 0)
                {
                  ++zeroCount;
                }
              }
              else
              {
                return ("ERROR: Parsing line string to int");
              }
            }

            return (zeroCount.ToString());
          }
        case 2:
          {
            int zeroCount = 0;
            var puzzle = input
              .Replace('L', '-')
              .Replace('R', '+')
              .Split(Environment.NewLine);

            foreach (var line in puzzle)
            {
              int currentDialPosition = 50;
              int dialMovement = 0;

              if (Int32.TryParse(line, out dialMovement))
              {
                do
                {
                  if (dialMovement < 0) // Negative movement
                  {
                    if (currentDialPosition == 0)
                    {
                      currentDialPosition = 99;
                    }
                    else
                    {
                      --currentDialPosition;
                    }

                    ++dialMovement;
                  }
                  else // Positive movement
                  {
                    if (currentDialPosition == 100)
                    {
                      currentDialPosition = 1;
                    }
                    else
                    {
                      ++currentDialPosition;
                    }

                    --dialMovement;
                  }

                  if ((currentDialPosition == 0) || (currentDialPosition == 100))
                  {
                    ++zeroCount;
                  }
                } while (dialMovement != 0);
              }
              else
              {
                return ("ERROR: Parsing line string to int");
              }
            }

            return (zeroCount.ToString());
          }
        default:
          return (Resource.OnlyPart1Or2);
      }
    }
    #endregion

    #region Day 2
    public static string Day2(string input, int part)
    {
      switch (part)
      {
        case 1:
          {
            string[] idRanges = input.Split(',');
            long result = 0;

            foreach (var range in idRanges)
            {
              string[] currentRange = range.Split('-');
              string idString = string.Empty;

              long minId = Int64.Parse(currentRange[0]);
              long maxId = Int64.Parse(currentRange[1]);

              for (long currentId = minId; currentId <= maxId; ++currentId)
              {
                idString = currentId.ToString();

                if (idString.Length % 2 != 0)
                {
                  int stringHalfLength = idString.Length / 2;

                  if (idString.Substring(0, stringHalfLength) == idString.Substring(stringHalfLength))
                  {
                    result += currentId;
                  }
                }
              }
            }

            return (result.ToString());
          }
        case 2:
          {
            string[] idRanges = input.Split(',');
            string idString = string.Empty;
            long result = 0;

            foreach (var range in idRanges)
            {
              string[] currentRange = range.Split('-');

              long minId = Int64.Parse(currentRange[0]);
              long maxId = Int64.Parse(currentRange[1]);

              for (long currentId = minId; currentId <= maxId; ++currentId)
              {
                idString = currentId.ToString();

                for (int groupSize = idString.Length / 2; groupSize > 0; --groupSize)
                {
                  if (idString.Length % groupSize != 0)
                  {
                    continue;
                  }

                  string compareGroup = idString.Substring(0, groupSize);
                  bool isValid = false;

                  for (int group = 1; group < (idString.Length / groupSize) && !isValid; ++group)
                  {
                    if (compareGroup != idString.Substring(groupSize * group, groupSize))
                    {
                      isValid = true;
                    }
                  }

                  if (!isValid)
                  {
                    result += currentId;
                    break;
                  }
                }
              }
            }

            return (result.ToString());
          }
        default:
          return (Resource.OnlyPart1Or2);
      }
    }
    #endregion

    #region Day 3
    public static string Day3(string input, int part)
    {
      switch (part)
      {
        case 1:
          {
            string[] banks = input.Split(Environment.NewLine);
            int result = 0;

            foreach (string bank in banks)
            {
              char largestDigit = '0';
              char secLargestDigit = '0';

              for (int i = 0; i < (bank.Length - 1); ++i)
              {
                if (largestDigit < bank[i])
                {
                  largestDigit = bank[i];
                }
              }

              for (int i = bank.IndexOf(largestDigit) + 1; i < bank.Length; ++i)
              {
                if (secLargestDigit < bank[i])
                {
                  secLargestDigit = bank[i];
                }
              }

              string currentJolt = $"{largestDigit}{secLargestDigit}";

              result += Int32.Parse(currentJolt);
            }

            return (result.ToString());
          }
        case 2:
          {
            string[] banks = input.Split(Environment.NewLine);
            long result = 0;

            foreach (string bank in banks)
            {
              int leftOffset = 0;
              int rightOffset = 11;
              string currentJolt = string.Empty;

              for (int batteryCount = 1; batteryCount <= 12; ++batteryCount)
              {
                char largestDigit = '0';

                for (int i = leftOffset; i < (bank.Length - rightOffset) && rightOffset >= 0; ++i)
                {
                  if (largestDigit < bank[i])
                  {
                    largestDigit = bank[i];
                    leftOffset = i + 1;
                  }
                }

                currentJolt += $"{largestDigit}";
                --rightOffset;
              }

              result += Int64.Parse(currentJolt);
            }

            return (result.ToString());
          }
        default:
          return (Resource.OnlyPart1Or2);
      }
    }
    #endregion

    #region Day 4
    public static string Day4(string input, int part)
    {
      switch (part)
      {
        case 1:
          {
            string[] rows = input.Trim().Split(Environment.NewLine);
            char[,] grid = GetGridFromStrArr(rows);
            int accessablePaperRolls = 0;

            for (int rowIndex = 0; rowIndex < rows.GetLength(0); ++rowIndex)
            {
              for (int columnIndex = 0; columnIndex < grid.GetLength(1); ++columnIndex)
              {
                if (grid[columnIndex, rowIndex] == '@' && IsSurroundingAccessable(grid, rowIndex, columnIndex))
                {
                  ++accessablePaperRolls;
                }
              }
            }

            return (accessablePaperRolls.ToString());
          }
        case 2:
          {
            string[] rows = input.Trim().Split(Environment.NewLine);
            char[,] grid = GetGridFromStrArr(rows);
            int removedPaperRolls = 0;
            bool removedAtLeastOne;

            do
            {
              removedAtLeastOne = false;

              for (int rowIndex = 0; rowIndex < rows.GetLength(0); ++rowIndex)
              {
                for (int columnIndex = 0; columnIndex < grid.GetLength(1); ++columnIndex)
                {
                  if (grid[columnIndex, rowIndex] == '@' && IsSurroundingAccessable(grid, rowIndex, columnIndex))
                  {
                    ++removedPaperRolls;
                    grid[columnIndex, rowIndex] = '.';
                    removedAtLeastOne = true;
                  }
                }
              }
            } while (removedAtLeastOne);

            return (removedPaperRolls.ToString());
          }
        default:
          return (Resource.OnlyPart1Or2);
      }
    }

    private static char[,] GetGridFromStrArr(string[] input)
    {
      char[,] grid = new char[input[0].Length, input.Length];

      for (int rowIndex = 0; rowIndex < input.Length; ++rowIndex)
      {
        for (int columnIndex = 0; columnIndex < input[rowIndex].Length; ++columnIndex)
        {
          grid[rowIndex, columnIndex] = input[columnIndex][rowIndex];
        }
      }

      return (grid);
    }

    private static bool IsSurroundingAccessable(char[,] grid, int rowIndex, int columnIndex)
    {
      int paperRollCount = 0;
      (int X, int Y)[] checkCoordinates =
      {
        (-1, -1), (0, -1), (1, -1),
        (-1, 0), (1, 0),
        (-1, 1), (0, 1), (1, 1)
      };

      foreach (var (x, y) in checkCoordinates)
      {
        int checkX = columnIndex + x;
        int checkY = rowIndex + y;

        // Check boundaries
        if ((checkX < 0) || (checkX >= grid.GetLength(0)) || (checkY < 0) || (checkY >= grid.GetLength(1)))
        {
          continue;
        }

        if (grid[columnIndex + x, rowIndex + y] == '@')
        {
          ++paperRollCount;
        }

        if (paperRollCount >= 4)
        {
          return (false);
        }
      }

      return (true);
    }
    #endregion

    #region Day 5
    public static string Day5(string input, int part)
    {
      switch (part)
      {
        case 1:
          {
            string[] database = input.Split(Environment.NewLine + Environment.NewLine);
            string[] freshIngredientIdRanges = database[0].Split(Environment.NewLine);
            string[] availableIngredientIDs = database[1].Split(Environment.NewLine);
            List<(long min, long max)> freshIngrediewntIDsMinMax = GetFreshIngredienIDsMinMax(freshIngredientIdRanges);
            int freshAvailableIngredientIDs = 0;

            foreach (string currentIngredientIdString in availableIngredientIDs)
            {
              long currentIngredientId = Int64.Parse(currentIngredientIdString);

              foreach ((long min, long max) in freshIngrediewntIDsMinMax)
              {
                if (currentIngredientId >= min && currentIngredientId <= max)
                {
                  ++freshAvailableIngredientIDs;
                  break;
                }
              }
            }

            return (freshAvailableIngredientIDs.ToString());
          }
        case 2:
          {
            string[] database = input.Split(Environment.NewLine + Environment.NewLine);
            string[] freshIngredientIdRanges = database[0].Split(Environment.NewLine);
            List<(long min, long max)> freshIngrediewntIDsMinMax = GetFreshIngredienIDsMinMax(freshIngredientIdRanges);
            long lastMaxId = 0;
            long freshIngredientIDs = 0;

            foreach ((long min, long max) in freshIngrediewntIDsMinMax)
            {
              if (max > lastMaxId)
              {
                freshIngredientIDs += (max - Math.Max(min, lastMaxId + 1)) + 1;

                lastMaxId = max;
              }
            }

            return (freshIngredientIDs.ToString());
          }
        default:
          return (Resource.OnlyPart1Or2);
      }
    }

    private static List<(long min, long max)> GetFreshIngredienIDsMinMax(string[] freshIngredientIdRanges)
    {
      List<(long min, long max)> freshIngredientIDsMinMax = new List<(long min, long max)>();

      foreach (string currentIdRange in freshIngredientIdRanges)
      {
        string[] minAndMaxId = currentIdRange.Split('-');

        freshIngredientIDsMinMax.Add((Int64.Parse(minAndMaxId[0]), Int64.Parse(minAndMaxId[1])));
      }

      freshIngredientIDsMinMax.Sort();
      return (freshIngredientIDsMinMax);
    }
    #endregion

    #region Day 6
    public static string Day6(string input, int part)
    {
      switch (part)
      {
        case 1:
          {
            string[] rows = Regex.Replace(input, @"[ ]+", " ").Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            int problemCount = rows[0].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
            string[] operators = rows[rows.Length - 1].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            long result = 0;

            for (int problemIndex = 0; problemIndex < problemCount; ++problemIndex)
            {
              long problemResult = 0;

              for (int rowIndex = rows.Length - 2; rowIndex >= 0; --rowIndex)
              {
                string[] currentRow = rows[rowIndex].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (operators[problemIndex] == "*")
                {
                  if (problemResult == 0)
                  {
                    problemResult = Int64.Parse(currentRow[problemIndex]);
                  }
                  else
                  {
                    problemResult *= Int64.Parse(currentRow[problemIndex]);
                  }
                }
                else if (operators[problemIndex] == "+")
                {
                  problemResult += Int64.Parse(currentRow[problemIndex]);
                }
              }

              result += problemResult;
            }

            return (result.ToString());
          }
        case 2:
          {
            List<string> rows = input.Split(Environment.NewLine).ToList();
            string operators = rows[rows.Count - 1];
            string number = "0";
            char currentOperator = '0';
            long problemResult = 0;
            long result = 0;

            rows.RemoveAt(rows.Count - 1);

            for (int columnIndex = 0; columnIndex < rows[0].Length; ++columnIndex)
            {
              if (operators[columnIndex] == '*' || operators[columnIndex] == '+')
              {
                result += problemResult;
                problemResult = 0;
                currentOperator = operators[columnIndex];
              }

              for (int rowIndex = 0; rowIndex < rows.Count; ++rowIndex)
              {
                if (rows[rowIndex][columnIndex] != ' ')
                {
                  number += rows[rowIndex][columnIndex];
                }
              }

              if (currentOperator == '*')
              {
                if (problemResult == 0)
                {
                  problemResult = Int64.Parse(number);
                }
                else
                {
                  if (number != "0")
                  {
                    problemResult *= Int64.Parse(number);
                  }
                }
              }
              else if (currentOperator == '+')
              {
                problemResult += Int64.Parse(number);
              }

              number = "0";
            }

            result += problemResult;

            return (result.ToString());
          }
        default:
          return (Resource.OnlyPart1Or2);
      }
    }
    #endregion

    #region Day 7
    public static string Day7(string input, int part)
    {
      switch (part)
      {
        case 1:
          {
            string[] rows = input.Split(Environment.NewLine);
            HashSet<int> beamIndices = new HashSet<int>();
            int beamSplitCount = 0;

            // The beam starts at the index of 'S'
            beamIndices.Add(rows[0].IndexOf('S'));

            // First two rows can be ignored
            for (int rowIndex = 2; rowIndex < rows.Length; ++rowIndex)
            {
              foreach (int beamIndex in beamIndices.ToArray())
              {
                if (rows[rowIndex][beamIndex] == '^')
                {
                  if (rowIndex - 1 >= 0)
                  {
                    beamIndices.Add(beamIndex - 1);
                  }
                  if (rowIndex + 1 < rows.Length)
                  {
                    beamIndices.Add(beamIndex + 1);
                  }

                  ++beamSplitCount;
                  beamIndices.Remove(beamIndex);
                }
              }
            }

            return (beamSplitCount.ToString());
          }
        case 2:
          {
            string[] rows = input.Split(Environment.NewLine);
            Dictionary<int, long> beamIndices = new Dictionary<int, long>();
            long timelineCount = 0;

            for (int i = 0; i < rows[0].Length; ++i)
            {
              beamIndices.Add(i, 0);
            }

            // The beam starts at the index of 'S'
            beamIndices[rows[0].IndexOf('S')] = 1;
            ++timelineCount;

            // First two rows can be ignored and only every second row after that is relevant
            for (int rowIndex = 2; rowIndex < rows.Length; rowIndex += 2)
            {
              for (int columnIndex = 0; columnIndex < rows[rowIndex].Length; ++columnIndex)
              {
                if (rows[rowIndex][columnIndex] == '^')
                {
                  if (beamIndices[columnIndex] > 0)
                  {
                    beamIndices[columnIndex - 1] += beamIndices[columnIndex];
                    beamIndices[columnIndex + 1] += beamIndices[columnIndex];
                    timelineCount += beamIndices[columnIndex];

                    beamIndices[columnIndex] = 0;
                  }
                }
              }
            }

            return (timelineCount.ToString());
          }
        default:
          return (Resource.OnlyPart1Or2);
      }
    }
    #endregion

    #region Day 8
    public static string Day8(string input, int part)
    {
      switch (part)
      {
        case 1:
          {
            List<SKPoint3> coordinates = ParseCoordinates(input);
            List<JunctionDistance> distances = GetJunctionBoxDistances(coordinates);
            List<List<SKPoint3>> circuits = new List<List<SKPoint3>>();

            for (int i = 0; i < distances.Count - 1; i++)
            {
              for (int j = i + 1; j < distances.Count; j++)
              {
                if (distances[i].leftBox == distances[j].leftBox)
                {
                  circuits[0].Add(distances[i].leftBox);
                  circuits[0].Add(distances[i].rightBox);
                  circuits[0].Add(distances[j].rightBox);
                }
                else if (distances[i].leftBox == distances[j].rightBox)
                {
                  circuits[0].Add(distances[i].leftBox);
                  circuits[0].Add(distances[i].rightBox);
                  circuits[0].Add(distances[j].leftBox);
                }
                else if (distances[i].rightBox == distances[j].rightBox)
                {
                  circuits[0].Add(distances[i].leftBox);
                  circuits[0].Add(distances[i].rightBox);
                  circuits[0].Add(distances[j].leftBox);
                }
              }
            }

            return (string.Empty);
          }
        case 2:
          {
            return (string.Empty);
          }
        default:
          return (Resource.OnlyPart1Or2);
      }
    }

    private struct JunctionDistance
    {
      public double distance;
      public SKPoint3 leftBox;
      public SKPoint3 rightBox;
    }

    private static List<SKPoint3> ParseCoordinates(string input)
    {
      List<SKPoint3> positions = new List<SKPoint3>();
      SKPoint3 new3dPoint;

      foreach (var currentLine in input.Split(Environment.NewLine))
      {
        var currentCoordinates = currentLine.Split(',');

        new3dPoint = new SKPoint3();
        new3dPoint.X = float.Parse(currentCoordinates[0]);
        new3dPoint.Y = float.Parse(currentCoordinates[1]);
        new3dPoint.Z = float.Parse(currentCoordinates[2]);

        positions.Add(new3dPoint);
      }

      return (positions);
    }

    private static double GetDistance(SKPoint3 leftCoordinate, SKPoint3 rightCoordinate)
    {
      double distance = 0;

      distance = Math.Sqrt
      (
        Math.Pow(rightCoordinate.X - leftCoordinate.X, 2) +
        Math.Pow(rightCoordinate.Y - leftCoordinate.Y, 2) +
        Math.Pow(rightCoordinate.Z - leftCoordinate.Z, 2)
      );

      return (distance);
    }

    private static List<JunctionDistance> GetJunctionBoxDistances(List<SKPoint3> coordinates)
    {
      List<JunctionDistance> distances = new List<JunctionDistance>();
      JunctionDistance currentJunctionDistance;

      for (int i = 0; i < coordinates.Count - 1; ++i)
      {
        currentJunctionDistance.leftBox = coordinates[i];

        for (int j = i + 1; j < coordinates.Count; ++j)
        {
          currentJunctionDistance.rightBox = coordinates[j];
          currentJunctionDistance.distance = GetDistance(currentJunctionDistance.leftBox, currentJunctionDistance.rightBox);

          distances.Add(currentJunctionDistance);
        }
      }

      return (distances.OrderBy(j => j.distance).ToList());
    }
    #endregion

    #region BLANKO
    public static string BLANKO(string input, int part)
    {
      switch (part)
      {
        case 1:
          {
            return (string.Empty);
          }
        case 2:
          {
            return (string.Empty);
          }
        default:
          return (Resource.OnlyPart1Or2);
      }
    }
    #endregion
  }
}
