using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ArcherTokenPiece : TokenPiece
{
    public override ID TokenID { get { return ID.ID_ARCHER; } }

    protected override int MaxMoves { get { return MaxAttack; } }

    protected override int MaxAttack { get { return 2; } }

    protected override int MaxMovement { get { return 2; } }

    public override List<int> CalculateJeopardy(BoardPiece[] boardPieces)
    {
        // TODO: again, don't take into account '2 turns' yet

        // Anything attackable is threat
        return FindAttackableTiles(boardPieces);
    }

    public override List<int> FindAttackableTiles(BoardPiece[] boardPieces)
    {
        var result = new List<int>();
        var defendingSoldiers = new List<Vector2Int>();
        var currentPieceIndex = boardPieces[BoardPosition];
        Vector2Int gridPosition = GameHelper.BoardToGridPosition(BoardPosition);
        int newCol, newRow;

        // Assume everything that doesn't have a friendly is valid, hold onto enemy soldiers in defense
        for (var i = -Movement; i <= Movement; ++i)
        {
            newCol = gridPosition.x + i;
            if (newCol < 0 || newCol >= GameHelper.SHORT_DIMENSIONS) continue;
            for (int j = -Movement; j <= Movement; ++j)
            {
                if (i == 0 && j == 0) continue;
                newRow = gridPosition.y + j;
                if (newRow >= 0 && newRow < GameHelper.SHORT_DIMENSIONS)
                {
                    var index = GameHelper.GridToBoardPosition(newCol, newRow);
                    if (boardPieces[index].Piece != null)
                    {
                        var piece = boardPieces[index].Piece;
                        if (piece.PlayerOwner == PlayerOwner) continue;
                        else if (piece.TokenID == ID.ID_SOLDIER && ((SoldierTokenPiece)piece).IsDefense) defendingSoldiers.Add(new Vector2Int(newCol, newRow));
                    }
                    result.Add(index);
                }
            }
        }

        // Now, go through the soldiers in defense and make the spaces behind invalid
        foreach (var defenderPosition in defendingSoldiers)
        {
            // defenderIndex is the column, row of the soldier
            var colDifference = defenderPosition.x - gridPosition.x;
            var colDirection = colDifference != 0 ? (colDifference / Math.Abs(colDifference)) : 0;
            var rowDifference = defenderPosition.y - gridPosition.y;
            var rowDirection = rowDifference != 0 ? (rowDifference / Math.Abs(rowDifference)) : 0;

            // the largest distance is the number of moves to take away
            var distanceInMoves = Math.Max(colDifference, rowDifference);

            // If we've reached the maximum moves, soldier has nothing to defend from us
            if (distanceInMoves >= Attack) break;
            var movesRemaining = Attack - distanceInMoves;

            // rowDifference = 0 (left or right)
            if (rowDifference == 0)
            {
                var colBehind = defenderPosition.x + (colDifference / colDifference);   // offset by the direction
                for (var rowIndex = -1; rowIndex <= 1; ++rowIndex)
                {
                    // Make the squares within the cone invalid (for archers, it's just one behind)
                    var gridIndex = GameHelper.GridToBoardPosition(colBehind, defenderPosition.y + rowIndex);
                    result.Remove(gridIndex);
                }

            }
            // colDifference = 0 (above or below)
            else if (colDifference == 0)
            {
                var rowBehind = defenderPosition.y + (rowDifference / rowDifference); // offset by the direction
                for (var colIndex = -1; colIndex <= 1; ++colIndex)
                {
                    // Make the squares within the cone invalid (for archers, it's just one behind)
                    var gridIndex = GameHelper.GridToBoardPosition(defenderPosition.x + colIndex, rowBehind);
                    result.Remove(gridIndex);
                }
            }
            // either are 0 (for archer, has to be diagonal)
            else
            {
                // Make the squares within the cone invalid (for archers, it's just one behind)
                var gridIndex = GameHelper.GridToBoardPosition(defenderPosition.x + colDirection, defenderPosition.y);
                result.Remove(gridIndex);
                gridIndex = GameHelper.GridToBoardPosition(defenderPosition.x + colDirection, defenderPosition.y + rowDirection);
                result.Remove(gridIndex);
                gridIndex = GameHelper.GridToBoardPosition(defenderPosition.x, defenderPosition.y + rowDirection);
                result.Remove(gridIndex);
            }
        }

        return result;
    }

    public override List<int> FindMoveableTiles(BoardPiece[] boardPieces)
    {
        var result = new List<int>();

        // As long as there is one valid path within 2 spaces, it is valid. (NOTE cannot land on enemy 2 spaces far)
        var currentPieceIndex = boardPieces[BoardPosition];
        Vector2Int gridPosition = GameHelper.BoardToGridPosition(BoardPosition);
        int newCol, newRow;

        for (var i = -Movement; i <= Movement; ++i)
        {
            newCol = gridPosition.x + i;
            if (newCol < 0 || newCol >= GameHelper.SHORT_DIMENSIONS) continue;
            for (int j = -Movement; j <= Movement; ++j)
            {
                if (i == 0 && j == 0) continue;
                newRow = gridPosition.y + j;
                if (newRow >= 0 && newRow < GameHelper.SHORT_DIMENSIONS)
                {
                    var index = GameHelper.GridToBoardPosition(newCol, newRow);
                    if (index >= boardPieces.Length) continue;

                    // If this is 2 spaces away and there's an enemy, it is invalid
                    if (boardPieces[index].Piece != null && boardPieces[index].Piece.PlayerOwner != PlayerOwner && (j == -MaxMovement || j == MaxMovement)) continue;

                    // make sure there's a path from here to the token piece
                    if (GameHelper.TryAStarFunction(newCol, newRow, gridPosition.x, gridPosition.y, this, boardPieces, Movement, true))
                        result.Add(index);
                }
            }
        }
        return result;
    }
}

