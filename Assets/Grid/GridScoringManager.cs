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
            if (line.Count >= minRunLength)
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
            if (line.Count >= minRunLength)
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
            if (line.Count >= minRunLength)
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
            if (line.Count >= minRunLength)
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

    private ScoringLine CheckSeasonRun(List<GridTile> tiles)
    {
        if (tiles.Count < minRunLength)
            return null;

        var runs = FindConsecutiveRuns(
            tiles,
            GetSeasonType,
            values => values.All(v => v != SeasonType.None && v == values[0])
        );
        var longestRun = runs.OrderByDescending(run => run.Count).FirstOrDefault();

        if (longestRun != null && longestRun.Count >= minRunLength)
        {
            return new ScoringLine(ScoringPattern.SeasonRun, longestRun);
        }

        return null;
    }

    private ScoringLine CheckSeasonAlternating(List<GridTile> tiles)
    {
        if (tiles.Count < minRunLength)
            return null;

        var runs = FindConsecutiveRuns(tiles, GetSeasonType, IsValidSeasonAlternating);
        var longestRun = runs.OrderByDescending(run => run.Count).FirstOrDefault();

        if (longestRun != null && longestRun.Count >= minRunLength)
        {
            return new ScoringLine(ScoringPattern.SeasonAlternating, longestRun);
        }

        return null;
    }

    private ScoringLine CheckSeasonSequence(List<GridTile> tiles)
    {
        if (tiles.Count < minRunLength)
            return null;

        // Check for forward sequence (Spring -> Summer -> Autumn -> Winter -> Spring...)
        var forwardRuns = FindConsecutiveRuns(
            tiles,
            GetSeasonType,
            values => IsValidSeasonSequence(values, true)
        );
        var longestForwardRun = forwardRuns.OrderByDescending(run => run.Count).FirstOrDefault();

        // Check for backward sequence (Winter -> Autumn -> Summer -> Spring -> Winter...)
        var backwardRuns = FindConsecutiveRuns(
            tiles,
            GetSeasonType,
            values => IsValidSeasonSequence(values, false)
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

        if (longestRun != null && longestRun.Count >= minRunLength)
        {
            return new ScoringLine(ScoringPattern.SeasonSequence, longestRun);
        }

        return null;
    }

    private bool IsValidSeasonAlternating(List<SeasonType> values)
    {
        if (values.Count <= 1)
            return true;

        if (values[0] == values[1])
            return false;

        return values
            .Select((v, index) => new { Value = v, Index = index })
            .All(item =>
            {
                if (item.Value == SeasonType.None)
                    return false;

                return item.Index % 2 == 0 ? item.Value == values[0] : item.Value == values[1];
            });
    }

    private bool IsValidSeasonSequence(List<SeasonType> values, bool isForward)
    {
        if (values == null || values.Count == 0)
            return false;

        // Filter out None values
        var validSeasons = values.Where(v => v != SeasonType.None).ToList();
        if (validSeasons.Count != values.Count)
            return false;

        // Check if the sequence follows the specified order with looping
        for (int i = 1; i < validSeasons.Count; i++)
        {
            var currentSeason = validSeasons[i];
            var previousSeason = validSeasons[i - 1];

            // Calculate expected next season (with looping)
            var expectedNextSeason = isForward
                ? (SeasonType)(((int)previousSeason + 1) % 4)
                : (SeasonType)(((int)previousSeason - 1 + 4) % 4);

            if (currentSeason != expectedNextSeason)
                return false;
        }

        return true;
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
