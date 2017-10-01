using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Game helper.
/// </summary>
public class GameHelper
{
    #region Fields
    public static readonly int SHORT_DIMENSIONS = 9;

    public static readonly Dictionary<int, ID> OPPOSITION_ATTACK = new Dictionary<int, ID> 
                                                                                            { 
                                                                                                {26, ID.ID_ARCHER},
                                                                                                {34,ID.ID_SOLDIER},
                                                                                                {42, ID.ID_ARCHER},
                                                                                                {43, ID.ID_CANNON},
                                                                                                {44, ID.ID_GENERAL},
                                                                                                {52, ID.ID_SOLDIER},
                                                                                                {62, ID.ID_ARCHER}
                                                                                            };
    public static readonly Dictionary<int, ID> OPPOSITION_DEFENSE = new Dictionary<int, ID> 
                                                                                            {
                                                                                                {33, ID.ID_ARCHER},
                                                                                                {34, ID.ID_SOLDIER},
                                                                                                {42, ID.ID_ARCHER},
                                                                                                {43, ID.ID_CANNON},
                                                                                                {44, ID.ID_GENERAL},
                                                                                                {51, ID.ID_ARCHER},
                                                                                                {52, ID.ID_SOLDIER}
                                                                                            };
    public static readonly Dictionary<int, ID> CHALLENGER_ATTACK = new Dictionary<int, ID> 
                                                                                            { 
                                                                                                {18, ID.ID_ARCHER},
                                                                                                {28, ID.ID_SOLDIER},
                                                                                                {38, ID.ID_ARCHER},
                                                                                                {37, ID.ID_CANNON},
                                                                                                {36, ID.ID_GENERAL},
                                                                                                {46, ID.ID_SOLDIER},
                                                                                                {54, ID.ID_ARCHER}
                                                                                            };
    public static readonly Dictionary<int, ID> CHALLENGER_DEFENSE = new Dictionary<int, ID> 
                                                                                            { 
                                                                                                {29, ID.ID_ARCHER},
                                                                                                {28, ID.ID_SOLDIER},
                                                                                                {38, ID.ID_ARCHER},
                                                                                                {37, ID.ID_CANNON},
                                                                                                {36, ID.ID_GENERAL},
                                                                                                {47, ID.ID_ARCHER},
                                                                                                {46, ID.ID_SOLDIER}
                                                                                            };
    #endregion

    /// <summary>
    /// Sets the token position to board position.
    /// </summary>
    /// <param name="token">Token.</param>
    /// <param name="boardPieces">Board pieces.</param>
    public static void SetTokenPositionToBoardPosition(TokenPiece token, ref BoardPiece[] boardPieces)
    {
        Vector3 boardPositionVector = boardPieces[token.BoardPosition].transform.position;
        token.transform.position = new Vector3(boardPositionVector.x, boardPositionVector.y + 0.1f, boardPositionVector.z);
        // Make sure the initial location is correct
        token.SetTokenStartPosition();
    }

    /// <summary>
    /// Calculates the absolute distance.
    /// </summary>
    /// <returns>The absolute distance.</returns>
    /// <param name="Ax">Ax.</param>
    /// <param name="Ay">Ay.</param>
    /// <param name="Bx">Bx.</param>
    /// <param name="By">By.</param>
    public static int CalculateAbsoluteDistance(int Ax, int Ay, int Bx, int By)
    {
        return Math.Max(Math.Abs(Bx - Ax), Math.Abs(By - Ay));
    }

    /// <summary>
    /// Removes the highlights.
    /// </summary>
    /// <param name="selectedToken">Selected token.</param>
    public static void RemoveHighlights(TokenPiece selectedToken)
    {
        if (selectedToken != null)
        {
            foreach (BoardPiece piece in selectedToken.PossibleMoves)
            {
                piece.ToggleSelectable(false);
            }
        }
    }

    /// <summary>
    /// Determines valid positions for the game pieces to select [move or attack]
    /// </summary>
    /// <param name="activeToken">The game piece trying to move or attack</param>
    /// <param name="boardPieces">The board</param>
    /// <param name="isAttacking">If the player is attacking or moving</param>
    public static void DetermineSelectableTiles(TokenPiece activeToken, List<TokenPiece> tokens, List<TokenPiece> enemyTokens, ref BoardPiece[] boardPieces, bool isAttacking = false)
    {
        // with the number of possible moves/attacks, find the valid possible moves
        // TODO: once we see that jeopardy colors are working, prevent the pieces from going into jeopardy (wait...just the general?)
        var validTiles = FindApproachableTiles(activeToken, tokens, enemyTokens, boardPieces, isAttacking);
        foreach (var tile in validTiles)
        {
            boardPieces[tile].ToggleSelectable(true, isAttacking);
            activeToken.PossibleMoves.Add(boardPieces[tile]);
        }


        //if (activeToken == null)
        //    throw new NullReferenceException();

        //int currentPosition = activeToken.BoardPosition;
        //int row = currentPosition / SHORT_DIMENSIONS;
        //int col = currentPosition % SHORT_DIMENSIONS;
        //int newRow, newCol;
        //int maxMoves = isAttacking ? activeToken.Attack : activeToken.Movement;

        //for (int i = -maxMoves; i <= maxMoves; ++i)
        //{
        //    newCol = col + i;
        //    if (newCol < 0 || newCol >= SHORT_DIMENSIONS)
        //        continue;
        //    for (int j = -maxMoves; j <= maxMoves; ++j)
        //    {
        //        newRow = row + j;
        //        if (newRow >= 0 && newRow < SHORT_DIMENSIONS)
        //        {
        //            var index = newRow * SHORT_DIMENSIONS + newCol;
        //            if (Array.Exists(tokens.ToArray(), p => p.BoardPosition == index))
        //                continue;

        //            // TODO: there is an issue with finding a valid locations
        //            // Right now, if there's someone in the way, it won't matter

        //            // Make sure the path to this location is valid
        //            if (TryAStarFunction(newCol, newRow, col, row, tokens, enemyTokens, maxMoves, true))
        //            {
        //                boardPieces[index].ToggleSelectable(true, isAttacking);
        //                activeToken.PossibleMoves.Add(boardPieces[index]);
        //            }
        //            else
        //            {
        //                // Do NOT proceed forward
        //               // break;
        //            }
        //        }
        //    }
        //}
    }

    public static void DetermineThreatLevel(List<TokenPiece> oppositionTokens, List<TokenPiece> challengerTokens, ref BoardPiece[] boardPieces)
    {
        // Go through EACH piece
        foreach (var oppositionToken in oppositionTokens)
        {
            // Ignore the cannon for now, that one is special with its threat
            if (oppositionToken.TokenID == ID.ID_CANNON) continue;
            // Find the possible locations
            var validTiles = FindApproachableTiles(oppositionToken, oppositionTokens, challengerTokens, boardPieces, true); // NOTE: I think it should always be true to check THREAT
            
            foreach(var tile in validTiles)
            {
                // Add to the threat level
                boardPieces[tile].OppositionThreatLevel++;                
            }
            // Add to the game piece so we don't repeat this tile
            oppositionToken.ThreateningPieces = validTiles;            
        }

        foreach (var challengerToken in challengerTokens)
        {
            // Find the possible locations
            var validTiles = FindApproachableTiles(challengerToken, challengerTokens, oppositionTokens, boardPieces, true); // NOTE: I think it should always be true to check THREAT

            foreach (var tile in validTiles)
            {
                // Add to the threat level
                boardPieces[tile].ChallengerThreatLevel++;
            }
            // Add to the game piece so we don't repeat this tile
            challengerToken.ThreateningPieces = validTiles; 
        }
    }

    public static List<int> FindApproachableTiles(TokenPiece currentToken, List<TokenPiece> friendlyTokens, List<TokenPiece> enemyTokens, BoardPiece[] boardPieces, bool isAttacking = false)
    {
        // with the number of possible moves/attacks, find the valid possible moves
        if (currentToken == null)
            throw new NullReferenceException();

        var result = new List<int>();
        int currentPosition = currentToken.BoardPosition;
        int row = currentPosition / SHORT_DIMENSIONS;
        int col = currentPosition % SHORT_DIMENSIONS;
        int newRow, newCol;
        int maxMoves = isAttacking ? currentToken.Attack : currentToken.Movement;

        for (int i = -maxMoves; i <= maxMoves; ++i)
        {
            newCol = col + i;
            if (newCol < 0 || newCol >= SHORT_DIMENSIONS)
                continue;
            for (int j = -maxMoves; j <= maxMoves; ++j)
            {
                newRow = row + j;
                if (newRow >= 0 && newRow < SHORT_DIMENSIONS)
                {
                    var index = newRow * SHORT_DIMENSIONS + newCol;
                    if (Array.Exists(friendlyTokens.ToArray(), p => p.BoardPosition == index))
                        continue;

                    // TODO: there is an issue with finding a valid locations
                    // Right now, if there's someone in the way, it won't matter

                    // Make sure the path to this location is valid
                    if (TryAStarFunction(newCol, newRow, col, row, friendlyTokens, enemyTokens, maxMoves, true))
                        result.Add(index);                    
                }
            }
        }
        return result;
    }

    public static bool TryAStarFunction(int xPos, int yPos, int xDestination, int yDestination, List<TokenPiece> friendlyTokens, List<TokenPiece> enemyTokens,
                                        int movesRemaining, bool isInitialCheck = false)
    {
        var currentPos = GridToBoardPosition(xPos, yPos);
        var destinationPos = GridToBoardPosition(xDestination, yDestination);

        // if currentPos is invalid, bail
        if (destinationPos < 0 || destinationPos >= SHORT_DIMENSIONS * SHORT_DIMENSIONS)
            return false;

        // If the distance is greater than our remaining moves, bail
        if (xDestination - xPos > movesRemaining || yDestination - yPos > movesRemaining)
            return false;

        // If we're at the source, we did it!
        if (currentPos == destinationPos)
            return true;

        // If there's a token here at all or we're out of moves, this path isn't valid (exception if we just started and an enemy is there)
        if (Array.Exists(friendlyTokens.ToArray(), p => p.BoardPosition == currentPos) ||
            (Array.Exists(enemyTokens.ToArray(), p => p.BoardPosition == currentPos) && !isInitialCheck) || movesRemaining < 0)
            return false;

        // Looks like this tile is empty. Keep moving through all the paths
        //var xDir = (xDestination - xPos == 0) ? 0 : (int)((xDestination - xPos) / (xDestination - xPos));
        //var yDir = (yDestination - yPos == 0) ? 0 : (int)((yDestination - yPos) / (yDestination - yPos));
        // Move along X (if applicable)
        if (TryAStarFunction(xPos+1 , yPos, xDestination, yDestination, friendlyTokens, enemyTokens, movesRemaining - 1))
            return true;
        if (TryAStarFunction(xPos - 1, yPos, xDestination, yDestination, friendlyTokens, enemyTokens, movesRemaining - 1))
            return true;
        // Move along Y (if applicable)
        if (TryAStarFunction(xPos, yPos + 1, xDestination, yDestination, friendlyTokens, enemyTokens, movesRemaining - 1))
            return true;
        // Move diagonaly in all directions
        if (TryAStarFunction(xPos, yPos - 1, xDestination, yDestination, friendlyTokens, enemyTokens, movesRemaining - 1))
            return true;
        if (TryAStarFunction(xPos + 1, yPos + 1, xDestination, yDestination, friendlyTokens, enemyTokens, movesRemaining - 1))
            return true;
        if (TryAStarFunction(xPos - 1, yPos - 1, xDestination, yDestination, friendlyTokens, enemyTokens, movesRemaining - 1))
            return true;
        if (TryAStarFunction(xPos + 1, yPos - 1, xDestination, yDestination, friendlyTokens, enemyTokens, movesRemaining - 1))
            return true;
        if (TryAStarFunction(xPos - 1, yPos + 1, xDestination, yDestination, friendlyTokens, enemyTokens, movesRemaining - 1))
            return true;

        // We exhausted the search, abort (it shouldn't hit this)
        return false;
    }

    public static void DetermineCannonSelectableAttackTiles(CannonTokenPiece activeToken, List<TokenPiece> friendlyTokens, List<TokenPiece> enemyTokens, ref BoardPiece[] boardPieces)
    {
        // The cannon is a little different
        // Instead of within a set number of tiles in any direction,
        // the cannon may fire across the board in any direction at a straight line
        // Rules of Cannon Firing:
        //      Cannon cannot fire a direction if the first hit is a friendly unit
        //      Cannon can fire a direction with no game pieces in the path (obviously this is a bad move for players though)
        if (activeToken == null)
            throw new NullReferenceException();

        int currentPosition = activeToken.BoardPosition;
        int row = currentPosition / SHORT_DIMENSIONS;
        int col = currentPosition % SHORT_DIMENSIONS;
        int newRow = row;
        int newCol = col;

        // Go each direction
        for (int i = -1; i <= 1; ++i)
        {
            for (int j = -1; j <= 1; ++j)
            {
                // Skip if we hit our position
                if (i == 0 && j == 0) continue;

                // Check each tile, moving in a straight direction
                int forwardPos = 1;
                while (true)
                {
                    newCol = col + (i * forwardPos);
                    newRow = row + (j * forwardPos);

                    // If we've reached the end of the board and haven't found an enemy or friend, stop checking this direction
                    if ((newCol < 0 || newCol >= SHORT_DIMENSIONS) || (newRow < 0 || newRow >= SHORT_DIMENSIONS)) break;

                    // Highlight this position
                    boardPieces[GridToBoardPosition(newCol, newRow)].ToggleSelectable(true, true);

                    // Is there someone at this position?
                    // If it's an enemy, stop checking
                    if (Array.Exists(enemyTokens.ToArray(), p => p.BoardPosition == GridToBoardPosition(newCol, newRow)))
                    {
                        activeToken.PossibleMoves.Add(boardPieces[GridToBoardPosition(newCol, newRow)]);
                        break;
                    }
                    // If it's a friend, stop checking and remove all highlights going forward, we can't attack this direction
                    if (Array.Exists(friendlyTokens.ToArray(), p => p.BoardPosition == GridToBoardPosition(newCol, newRow)))
                    {
                        for (int reversePosition = forwardPos; reversePosition >= 0; --reversePosition)
                        {
                            var reverseCol = col + (i * reversePosition);
                            var reverseRow = row + (j * reversePosition);
                            boardPieces[GridToBoardPosition(reverseCol, reverseRow)].ToggleSelectable(false);
                        }
                        break;
                    }
                    ++forwardPos;

                    activeToken.PossibleMoves.Add(boardPieces[GridToBoardPosition(newCol, newRow)]);
                }
            }
        }
    }

    /// <summary>
    /// Sets the starting positions of the game pieces depending on the stance
    /// </summary>
    /// <param name="tokens">The game pieces for a player (Challenger or Opposition)</param>
    /// <param name="boardPieces">The board</param>
    /// <param name="isAttacking">Are we agressive or defensive?</param>
    /// <param name="isOpposition">Challenger or Opposition?</param>
    public static void SetGameTokensStartPosition(ref List<TokenPiece> tokens, ref BoardPiece[] boardPieces, bool isAttacking, bool isOpposition)
    {
        Dictionary<int, ID> pieceLayout;
        if (isOpposition)
            pieceLayout = isAttacking ? OPPOSITION_ATTACK : OPPOSITION_DEFENSE;
        else
            pieceLayout = isAttacking ? CHALLENGER_ATTACK : CHALLENGER_DEFENSE;

        var archers = tokens.FindAll(p => p.TokenID == ID.ID_ARCHER);
        var soldiers = tokens.FindAll(p => p.TokenID == ID.ID_SOLDIER);
        var cannon = tokens.Find(p => p.TokenID == ID.ID_CANNON);
        var general = tokens.Find(p => p.TokenID == ID.ID_GENERAL);
        List<int> archerPositions = new List<int>();
        List<int> soldierPositions = new List<int>();

        foreach (KeyValuePair<int, ID> pair in pieceLayout)
        {
            switch (pair.Value)
            {
                case ID.ID_ARCHER:
                    archerPositions.Add(pair.Key);
                    break;
                case ID.ID_CANNON:
                    cannon.BoardPosition = pair.Key;
                    SetTokenPositionToBoardPosition(cannon, ref boardPieces);
                    break;
                case ID.ID_GENERAL:
                    general.BoardPosition = pair.Key;
                    SetTokenPositionToBoardPosition(general, ref boardPieces);
                    break;
                case ID.ID_SOLDIER:
                    soldierPositions.Add(pair.Key);
                    break;
            }
        }
        for (var i = 0; i < archerPositions.Count; ++i)
        {
            archers[i].BoardPosition = archerPositions[i];
            SetTokenPositionToBoardPosition(archers[i], ref boardPieces);
        }
        for (var i = 0; i < soldierPositions.Count; ++i)
        {
            soldiers[i].BoardPosition = soldierPositions[i];
            SetTokenPositionToBoardPosition(soldiers[i], ref boardPieces);
        }
    }


    /// <summary>
    /// Finds and highlights adjacent friendly tokens the cannon can use
    /// </summary>
    /// <param name="cannon">The cannon looking for friendly units</param>
    /// <param name="boardPieces">the board</param>
    /// <param name="friendlyTokens">all friendlies on the board</param>
    /// <returns><true>There are at least 2 friendly units</true><false>There aren't enough friendly units on the board adjacent</false></returns>
    public static bool FindAdjacentFriendlyUnits(TokenPiece cannon, ref BoardPiece[] boardPieces, List<TokenPiece> friendlyTokens)
    {
        // Find any/all friendly units nearby
        var friendlies = new List<TokenPiece>();
        for (int i = -1; i <= 1; ++i)
        {
            for (int j = -1; j <= 1; ++j)
            {
                if (j == 0 && i == 0) continue;
                var offset = i * SHORT_DIMENSIONS + j;
                var friendly = friendlyTokens.Find(p => p.BoardPosition == cannon.BoardPosition + offset);
                if (friendly != null && friendly.Movement != 0 && !friendly.IsCannonFriend) // the friendly unit has to have its action still
                {
                    cannon.PossibleMoves.Add(boardPieces[friendly.BoardPosition]);
                    friendlies.Add(friendly);
                }
            }
        }

        // Did we find a friend?
        if (friendlies.Count == 0) return false;

        // Highlight the board pieces for the friendly units we found
        foreach (var friendly in friendlies)
            boardPieces[friendly.BoardPosition].ToggleAbilitySelectable(true);

        return true;
    }

    /// <summary>
    /// Determines if defending soldier is blocking the specified defendingToken.
    /// </summary>
    /// <returns><c>TokenPiece</c> if defending soldier is blocking the specifieddefendingToken; otherwise, <c>null</c>.</returns>
    /// <param name="attackingToken">Attacking token.</param>
    /// <param name="defendingToken">Defending token.</param>
    /// <param name="enemyPieces">Enemy pieces.</param>
    public static TokenPiece IsDefendingSoldierBlocking(TokenPiece attackingToken, TokenPiece defendingToken, List<TokenPiece> enemyPieces)
    {
        TokenPiece result;
        int NUMCOLS = SHORT_DIMENSIONS;
        int distanceX = (defendingToken.BoardPosition % NUMCOLS) - (attackingToken.BoardPosition % NUMCOLS);
        int distanceY = (defendingToken.BoardPosition / NUMCOLS) - (attackingToken.BoardPosition / NUMCOLS);
        int i = 0;
        int j = 0;
        int iSign = Math.Sign(distanceX);
        int jSign = Math.Sign(distanceY);
        int checkingBoardPosition = attackingToken.BoardPosition + GridToBoardPosition(i, j);
        while (checkingBoardPosition != defendingToken.BoardPosition)
        {
            i = (distanceX != i) ? i + iSign : i;
            j = (distanceY != j) ? j + jSign : j;
            checkingBoardPosition = attackingToken.BoardPosition + GridToBoardPosition(i, j);
            result = enemyPieces.Find(p => p.BoardPosition == checkingBoardPosition);
            if (result != null && result.TokenID == ID.ID_SOLDIER && (result as SoldierTokenPiece).IsDefense)
                return result;
        }
        return null;
    }

    /// <summary>
    /// Converts 2D position to 1D array position
    /// </summary>
    /// <returns>The to board position.</returns>
    /// <param name="Column">Column.</param>
    /// <param name="Row">Row.</param>
    public static int GridToBoardPosition(int Column, int Row)
    {
        return Row * SHORT_DIMENSIONS + Column;
    }

    /// <summary>
    /// Converts 1D array position to 2D position
    /// </summary>
    /// <param name="position">Position in board space</param>
    /// <returns><X>Column</X><Y>Row</Y></returns>
    public static Vector2 BoardToGridPosition(int position)
    {
        return new Vector2(position % SHORT_DIMENSIONS, position / SHORT_DIMENSIONS);
    }
}
