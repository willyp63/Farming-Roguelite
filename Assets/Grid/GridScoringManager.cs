using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ScoringPattern
{
    SeasonRun, // 4 or more with the same SeasonType
    SeasonAlternating, // 4 or more with alternating SeasonType
    SeasonSequence, // 4 or more with SeasonType in sequence
}

public class ScoringLine
{
    public ScoringPattern pattern;
    public List<GridTile> tiles;

    public ScoringLine(ScoringPattern pattern, List<GridTile> tiles)
    {
        this.pattern = pattern;
        this.tiles = tiles;
    }
}

public class GridScoringManager : Singleton<GridScoringManager>
{
    [Header("Scoring Settings")]
    [SerializeField]
    private int minRunLength = 3;

    [SerializeField]
    private int minAlternatingLength = 4;

    [SerializeField]
    private int minSequenceLength = 4;

    public int MinLineLength => Mathf.Min(minRunLength, minAlternatingLength, minSequenceLength);

    private List<List<Vector2Int>> allLines;

    private void GenerateAllLines()
    {
        allLines = new List<List<Vector2Int>>();
        int gridWidth = GridManager.Instance.GridWidth;
        int gridHeight = GridManager.Instance.GridHeight;

        // Generate horizontal lines
        for (int y = 0; y < gridHeight; y++)
        {
            List<Vector2Int> line = new List<Vector2Int>();
            for (int x = 0; x < gridWidth; x++)
            {
                line.Add(new Vector2Int(x, y));
            }
            allLines.Add(line);
        }

        // Generate vertical lines
        for (int x = 0; x < gridWidth; x++)
        {
            List<Vector2Int> line = new List<Vector2Int>();
            for (int y = 0; y < gridHeight; y++)
            {
                line.Add(new Vector2Int(x, y));
            }
            allLines.Add(line);
        }

        // Generate diagonal lines (top-left to bottom-right)
        for (int startX = 0; startX < gridWidth; startX++)
        {
            List<Vector2Int> line = new List<Vector2Int>();
            int x = startX;
            int y = 0;
            while (x < gridWidth && y < gridHeight)
            {
                line.Add(new Vector2Int(x, y));
                x++;
                y++;
            }
            if (line.Count >= MinLineLength)
            {
                allLines.Add(line);
            }
        }

        for (int startY = 1; startY < gridHeight; startY++)
        {
            List<Vector2Int> line = new List<Vector2Int>();
            int x = 0;
            int y = startY;
            while (x < gridWidth && y < gridHeight)
            {
                line.Add(new Vector2Int(x, y));
                x++;
                y++;
            }
            if (line.Count >= MinLineLength)
            {
                allLines.Add(line);
            }
        }

        // Generate diagonal lines (top-right to bottom-left)
        for (int startX = 0; startX < gridWidth; startX++)
        {
            List<Vector2Int> line = new List<Vector2Int>();
            int x = startX;
            int y = 0;
            while (x >= 0 && y < gridHeight)
            {
                line.Add(new Vector2Int(x, y));
                x--;
                y++;
            }
            if (line.Count >= MinLineLength)
            {
                allLines.Add(line);
            }
        }

        for (int startY = 1; startY < gridHeight; startY++)
        {
            List<Vector2Int> line = new List<Vector2Int>();
            int x = gridWidth - 1;
            int y = startY;
            while (x >= 0 && y < gridHeight)
            {
                line.Add(new Vector2Int(x, y));
                x--;
                y++;
            }
            if (line.Count >= MinLineLength)
            {
                allLines.Add(line);
            }
        }
    }

    public List<ScoringLine> GetScoringLines()
    {
        if (allLines == null)
            GenerateAllLines();

        List<ScoringLine> scoringLines = new List<ScoringLine>();

        foreach (var line in allLines)
        {
            var tiles = GetTilesFromPositions(line);
            if (tiles.Count == 0)
                continue;

            // Check for SeasonRun
            var seasonRun = CheckSeasonRun(tiles);
            if (seasonRun != null)
            {
                scoringLines.Add(seasonRun);
            }

            // Check for SeasonAlternating
            var seasonAlternating = CheckSeasonAlternating(tiles);
            if (seasonAlternating != null)
            {
                scoringLines.Add(seasonAlternating);
            }

            // Check for SeasonSequence
            var seasonSequence = CheckSeasonSequence(tiles);
            if (seasonSequence != null)
            {
                scoringLines.Add(seasonSequence);
            }
        }

        return scoringLines;
    }

    public List<List<Vector2Int>> GetAllLinesForPosition(Vector2Int position)
    {
        if (allLines == null)
            GenerateAllLines();

        return allLines.Where(line => line.Contains(position)).ToList();
    }

    public bool AreAllLinesScorable()
    {
        if (allLines == null)
            GenerateAllLines();

        var unScorableLines = allLines.Where(line => !IsLineScorable(line)).ToList();
        Debug.Log($"Unscorable lines: {unScorableLines.Count}");
        foreach (var line in unScorableLines)
        {
            Debug.Log($"Unscorable line: {string.Join(", ", line)}");
        }
        return unScorableLines.Count == 0;
    }

    public bool IsLineScorable(List<Vector2Int> positions)
    {
        var tiles = GetTilesFromPositions(positions);

        // If the line is too short, it's not possible to score
        if (tiles.Count < MinLineLength)
            return false;

        // Check if full SeasonRun is possible
        var seasonRun = CheckSeasonRun(tiles, true);
        if (seasonRun != null && seasonRun.tiles.Count >= tiles.Count)
        {
            return true;
        }

        // Check if full SeasonAlternating is possible
        var seasonAlternating = CheckSeasonAlternating(tiles, true);
        if (seasonAlternating != null && seasonAlternating.tiles.Count >= tiles.Count)
        {
            return true;
        }

        // Check if full SeasonSequence is possible
        var seasonSequence = CheckSeasonSequence(tiles, true);
        if (seasonSequence != null && seasonSequence.tiles.Count >= tiles.Count)
        {
            return true;
        }

        return false;
    }

    private List<GridTile> GetTilesFromPositions(List<Vector2Int> positions)
    {
        List<GridTile> tiles = new List<GridTile>();
        foreach (var pos in positions)
        {
            tiles.Add(GridManager.Instance.GetTile(pos));
        }
        return tiles;
    }

    private SeasonType GetSeasonType(GridTile tile)
    {
        return tile.PlacedObject?.Season ?? SeasonType.None;
    }

    private bool IsWild(SeasonType season, bool emptyTilesAreWild)
    {
        return (emptyTilesAreWild && season == SeasonType.None) || season == SeasonType.Wild;
    }

    private ScoringLine CheckSeasonRun(List<GridTile> tiles, bool emptyTilesAreWild = false)
    {
        if (tiles.Count < minRunLength)
            return null;

        var runs = FindConsecutiveRuns(
            tiles,
            GetSeasonType,
            values =>
            {
                if (!emptyTilesAreWild && values.Any(v => v == SeasonType.None))
                    return false;

                if (values.All(v => IsWild(v, emptyTilesAreWild)))
                    return true;

                SeasonType firstNonWild = values.First(v => !IsWild(v, emptyTilesAreWild));
                return values.All(v => IsWild(v, emptyTilesAreWild) || v == firstNonWild);
            }
        );
        var longestRun = runs.OrderByDescending(run => run.Count).FirstOrDefault();

        if (longestRun != null && longestRun.Count >= minRunLength)
        {
            return new ScoringLine(ScoringPattern.SeasonRun, longestRun);
        }

        return null;
    }

    private ScoringLine CheckSeasonAlternating(List<GridTile> tiles, bool emptyTilesAreWild = false)
    {
        if (tiles.Count < minAlternatingLength)
            return null;

        var runs = FindConsecutiveRuns(
            tiles,
            GetSeasonType,
            values => IsValidSeasonAlternating(values, emptyTilesAreWild)
        );
        var longestRun = runs.OrderByDescending(run => run.Count).FirstOrDefault();

        if (longestRun != null && longestRun.Count >= minAlternatingLength)
        {
            return new ScoringLine(ScoringPattern.SeasonAlternating, longestRun);
        }

        return null;
    }

    private ScoringLine CheckSeasonSequence(List<GridTile> tiles, bool emptyTilesAreWild = false)
    {
        if (tiles.Count < minSequenceLength)
            return null;

        // Check for forward sequence (Spring -> Summer -> Autumn -> Winter -> Spring...)
        var forwardRuns = FindConsecutiveRuns(
            tiles,
            GetSeasonType,
            values => IsValidSeasonSequence(values, true, emptyTilesAreWild)
        );
        var longestForwardRun = forwardRuns.OrderByDescending(run => run.Count).FirstOrDefault();

        // Check for backward sequence (Winter -> Autumn -> Summer -> Spring -> Winter...)
        var backwardRuns = FindConsecutiveRuns(
            tiles,
            GetSeasonType,
            values => IsValidSeasonSequence(values, false, emptyTilesAreWild)
        );
        var longestBackwardRun = backwardRuns.OrderByDescending(run => run.Count).FirstOrDefault();

        // Return the longest run found
        var longestRun = longestForwardRun;
        if (
            longestBackwardRun != null
            && (longestRun == null || longestBackwardRun.Count > longestRun.Count)
        )
        {
            longestRun = longestBackwardRun;
        }

        if (longestRun != null && longestRun.Count >= minSequenceLength)
        {
            return new ScoringLine(ScoringPattern.SeasonSequence, longestRun);
        }

        return null;
    }

    private bool IsValidSeasonAlternating(List<SeasonType> values, bool emptyTilesAreWild = false)
    {
        if (!emptyTilesAreWild && values.Any(v => v == SeasonType.None))
            return false;

        // Any 1 length is valid
        if (values.Count <= 1)
            return true;

        // 2 length
        if (values.Count == 2)
        {
            // If either tile is wild, the run is valid
            if (IsWild(values[0], emptyTilesAreWild) || IsWild(values[1], emptyTilesAreWild))
                return true;

            // If both tiles are not wild, then they must be different
            return values[0] != values[1];
        }

        // 3+ length
        var firstEvenItem = values
            .Select((v, index) => new { Value = v, Index = index })
            .FirstOrDefault(item => !IsWild(item.Value, emptyTilesAreWild) && item.Index % 2 == 0);
        SeasonType firstEvenNonWild = firstEvenItem == null ? SeasonType.None : firstEvenItem.Value;

        var firstOddItem = values
            .Select((v, index) => new { Value = v, Index = index })
            .FirstOrDefault(item => !IsWild(item.Value, emptyTilesAreWild) && item.Index % 2 == 1);
        SeasonType firstOddNonWild = firstOddItem == null ? SeasonType.None : firstOddItem.Value;

        return values
            .Select((v, index) => new { Value = v, Index = index })
            .All(item =>
            {
                if (IsWild(item.Value, emptyTilesAreWild))
                    return true;

                return item.Index % 2 == 0
                    ? item.Value == firstEvenNonWild
                    : item.Value == firstOddNonWild;
            });
    }

    private bool IsValidSeasonSequence(
        List<SeasonType> values,
        bool isForward,
        bool emptyTilesAreWild = false
    )
    {
        if (!emptyTilesAreWild && values.Any(v => v == SeasonType.None))
            return false;

        if (values.All(v => IsWild(v, emptyTilesAreWild)))
            return true;

        SeasonType firstNonWild = values.First(v => !IsWild(v, emptyTilesAreWild));
        return values
            .Select((v, index) => new { Value = v, Index = index })
            .All(item =>
            {
                if (IsWild(item.Value, emptyTilesAreWild))
                    return true;

                SeasonType expectedSeason = isForward
                    ? (SeasonType)(((int)firstNonWild + item.Index) % 4)
                    : (SeasonType)(((int)firstNonWild - item.Index + (item.Index * 4)) % 4);

                return item.Value == expectedSeason;
            });
    }

    private List<List<GridTile>> FindConsecutiveRuns<T>(
        List<GridTile> tiles,
        System.Func<GridTile, T> selector,
        System.Func<List<T>, bool> isValidRun
    )
    {
        List<List<GridTile>> runs = new List<List<GridTile>>();

        if (tiles == null || tiles.Count == 0)
            return runs;

        List<GridTile> currentRun = new List<GridTile>();
        List<T> currentValues = new List<T>();

        for (int i = 0; i < tiles.Count; i++)
        {
            GridTile tile = tiles[i];
            T value = selector(tile);

            if (currentRun.Count == 0)
            {
                currentRun.Add(tile);
                currentValues.Add(value);
                continue;
            }

            // Check if this value continues the current run
            var currentValuesWithNewValue = new List<T>(currentValues)
                .Concat(new List<T> { value })
                .ToList();
            if (isValidRun(currentValuesWithNewValue))
            {
                currentRun.Add(tile);
                currentValues.Add(value);
            }
            else
            {
                // Current run is broken, save it if it's valid
                if (currentRun.Count >= minRunLength && isValidRun(currentValues))
                {
                    runs.Add(new List<GridTile>(currentRun));
                }

                // Required for alternating runs
                i -= currentRun.Count > 1 ? 2 : 1;

                currentRun.Clear();
                currentValues.Clear();
            }
        }

        // Don't forget to add the last run if it's valid
        if (currentRun.Count >= minRunLength && isValidRun(currentValues))
        {
            runs.Add(new List<GridTile>(currentRun));
        }

        return runs;
    }
}
