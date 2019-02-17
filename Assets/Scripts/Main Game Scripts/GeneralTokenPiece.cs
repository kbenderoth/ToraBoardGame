using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GeneralTokenPiece : TokenPiece
{
    public override ID TokenID { get { return ID.ID_GENERAL; } }

    protected override int MaxMoves { get { return 3; } }   // General can move 2 and attack 1
    protected override int MaxAttack { get { return 1; } }
    protected override int MaxMovement { get { return 2; } }

    // TODO: this is currently not being called
    public override bool CompleteMove()
    {
        if(PlayerOwner.PlayerState == PLAYERSTATE.ATTACK)
        {
            // cannot move after attacking
            _movement = 0;
            _attack = 0;
            return true;
        }
        else if(PlayerOwner.PlayerState == PLAYERSTATE.MOVE)
        {
            --_movement;
            if (_attack == 0 && _movement == 0)
                return true;
            return false;
        }
        else
        {
            throw new NotImplementedException("PLayer State is not in Attack or Move");
        }
    }

    public override void EndAction()
    {
        IsCannonFriend = false;
        CanOpenMenu = _attack == 0 && _movement == 0;
        DestroySubMenu();

        GameHelper.RemoveHighlights(this);
    }

    public override List<int> CalculateJeopardy(BoardPiece[] boardPieces)
    {
        // TODO: again, not taking into account of 2 turns...

        // Remember, General can move THEN attack. So the threat should be up to 3 instead of 2
        // Though the general cannot fire over someone's head (I don't think....)

        // APPROACH
        // 1) find moveable tiles
        // 2) grab the 'end' of each direction
        // 3) find attackable tiles from each of those positions
        // 4) any tiles that weren't already added from moveable tiles, add threat


        return new List<int>();
    }

    public override List<int> FindAttackableTiles(BoardPiece[] boardPieces)
    {
        // Should match Soldier's implementation
        var result = new List<int>();

        var currentPieceIndex = boardPieces[BoardPosition];
        Vector2Int gridPosition = GameHelper.BoardToGridPosition(BoardPosition);
        int newCol, newRow;
        for (var i = -Attack; i <= Attack; ++i)
        {
            newCol = gridPosition.x + i;
            if (newCol < 0)
                continue;
            for (int j = -Attack; j <= Attack; ++j)
            {
                newRow = gridPosition.y + j;
                if (newRow >= 0)
                {
                    var index = GameHelper.GridToBoardPosition(newCol, newRow);
                    if (index >= boardPieces.Length) continue;
                    if (boardPieces[index].Piece != null && boardPieces[index].Piece.PlayerOwner == PlayerOwner) continue;

                    // Since attack is only 1, simply return the result
                    result.Add(index);
                }
            }
        }
        return result;
    }

    public override List<int> FindMoveableTiles(BoardPiece[] boardPieces)
    {
        // Should match Archer's implementation
        var result = new List<int>();

        // As long as there is one valid path within 2 spaces, it is valid.
        var currentPieceIndex = boardPieces[BoardPosition];
        Vector2Int gridPosition = GameHelper.BoardToGridPosition(BoardPosition);
        int newCol, newRow;

        for (var i = -Movement; i <= Movement; ++i)
        {
            newCol = gridPosition.x + i;
            if (newCol < 0 || newCol >= GameHelper.SHORT_DIMENSIONS) continue;
            for (int j = -Movement; j <= Movement; ++j)
            {
                // ignore (0,0) since that's you
                if (i == 0 && j == 0) continue;
                newRow = gridPosition.y + j;
                if (newRow >= 0 && newRow < GameHelper.SHORT_DIMENSIONS)
                {
                    var index = GameHelper.GridToBoardPosition(newCol, newRow);                    

                    // make sure there's a path from here to the token piece
                    if (GameHelper.TryAStarFunction(newCol, newRow, gridPosition.x, gridPosition.y, this, boardPieces, Movement, true))
                        result.Add(index);
                }
            }
        }
        return result;
    }
}

