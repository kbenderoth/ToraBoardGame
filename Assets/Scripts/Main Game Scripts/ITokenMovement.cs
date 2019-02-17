using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITokenMovement
{
    #region Properties
    // number of attacks
    int NumAttacks { get; }
    // number of moves
    int NumMoves { get; }
    // is ranged
    bool IsRanged { get; }
    #endregion Properties

    #region Methods
    // Find Moveable
    List<int> FindMoveableTiles();
    // Find Attackable
    List<int> FindAttackableTiles();

    #endregion Methods
}
