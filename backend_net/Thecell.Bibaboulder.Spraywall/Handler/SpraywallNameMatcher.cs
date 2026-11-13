using System;

namespace Thecell.Bibaboulder.Indoor.Handler;

internal readonly record struct SpraywallNameMatch(int StartIndex, int Length, int Distance);

internal static class SpraywallNameMatcher
{
    public static SpraywallNameMatch? FindBestMatch(string name, string searchTerm)
    {
        var normalizedName = name.ToUpperInvariant();
        var normalizedSearchTerm = searchTerm.Trim().ToUpperInvariant();
        if (normalizedSearchTerm.Length == 0)
        {
            return null;
        }

        var maxDistance = normalizedSearchTerm.Length < 3
            ? 0
            : Math.Max(1, normalizedSearchTerm.Length / 4);
        SpraywallNameMatch? bestMatch = null;

        for (var startIndex = 0; startIndex < normalizedName.Length; startIndex++)
        {
            var minimumLength = Math.Max(1, normalizedSearchTerm.Length - maxDistance);
            var maximumLength = Math.Min(normalizedName.Length - startIndex, normalizedSearchTerm.Length + maxDistance);
            for (var length = minimumLength; length <= maximumLength; length++)
            {
                var distance = GetLevenshteinDistance(
                    normalizedSearchTerm,
                    normalizedName.Substring(startIndex, length));
                if (distance <= maxDistance &&
                    (!bestMatch.HasValue ||
                     distance < bestMatch.Value.Distance ||
                     (distance == bestMatch.Value.Distance &&
                      Math.Abs(length - normalizedSearchTerm.Length) <
                      Math.Abs(bestMatch.Value.Length - normalizedSearchTerm.Length))))
                {
                    bestMatch = new SpraywallNameMatch(startIndex, length, distance);
                    if (distance == 0)
                    {
                        return bestMatch;
                    }
                }
            }
        }

        return bestMatch;
    }

    private static int GetLevenshteinDistance(string first, string second)
    {
        var previousRow = new int[second.Length + 1];
        var currentRow = new int[second.Length + 1];
        for (var column = 0; column <= second.Length; column++)
        {
            previousRow[column] = column;
        }

        for (var row = 1; row <= first.Length; row++)
        {
            currentRow[0] = row;
            for (var column = 1; column <= second.Length; column++)
            {
                var substitutionCost = first[row - 1] == second[column - 1] ? 0 : 1;
                currentRow[column] = Math.Min(
                    Math.Min(currentRow[column - 1] + 1, previousRow[column] + 1),
                    previousRow[column - 1] + substitutionCost);
            }

            (previousRow, currentRow) = (currentRow, previousRow);
        }

        return previousRow[second.Length];
    }
}
