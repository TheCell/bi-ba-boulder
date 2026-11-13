export interface SpraywallNameMatch {
  startIndex: number;
  length: number;
}

export function findSpraywallNameMatch(name: string, searchTerm: string): SpraywallNameMatch | undefined {
  const normalizedName: string = name.toUpperCase();
  const normalizedSearchTerm: string = searchTerm.trim().toUpperCase();
  if (normalizedSearchTerm.length === 0) {
    return undefined;
  }

  const maxDistance: number = normalizedSearchTerm.length < 3 ? 0 : Math.max(1, Math.floor(normalizedSearchTerm.length / 4));
  let bestMatch: (SpraywallNameMatch & { distance: number }) | undefined;

  for (let startIndex = 0; startIndex < normalizedName.length; startIndex += 1) {
    const minimumLength: number = Math.max(1, normalizedSearchTerm.length - maxDistance);
    const maximumLength: number = Math.min(normalizedName.length - startIndex, normalizedSearchTerm.length + maxDistance);
    for (let length: number = minimumLength; length <= maximumLength; length += 1) {
      const distance: number = getLevenshteinDistance(
        normalizedSearchTerm,
        normalizedName.slice(startIndex, startIndex + length)
      );
      const lengthDifference: number = Math.abs(length - normalizedSearchTerm.length);
      if (
        distance <= maxDistance &&
        (!bestMatch ||
          distance < bestMatch.distance ||
          (distance === bestMatch.distance &&
            lengthDifference < Math.abs(bestMatch.length - normalizedSearchTerm.length)))
      ) {
        bestMatch = { startIndex, length, distance };
        if (distance === 0) {
          return { startIndex: bestMatch.startIndex, length: bestMatch.length };
        }
      }
    }
  }

  return bestMatch ? { startIndex: bestMatch.startIndex, length: bestMatch.length } : undefined;
}

function getLevenshteinDistance(first: string, second: string): number {
  let previousRow: number[] = Array.from({ length: second.length + 1 }, (_: unknown, index: number): number => index);
  let currentRow: number[] = Array.from({ length: second.length + 1 }, (): number => 0);

  for (let row = 1; row <= first.length; row += 1) {
    currentRow[0] = row;
    for (let column = 1; column <= second.length; column += 1) {
      const substitutionCost: number = first[row - 1] === second[column - 1] ? 0 : 1;
      currentRow[column] = Math.min(
        currentRow[column - 1] + 1,
        previousRow[column] + 1,
        previousRow[column - 1] + substitutionCost
      );
    }

    [previousRow, currentRow] = [currentRow, previousRow];
  }

  return previousRow[second.length];
}
