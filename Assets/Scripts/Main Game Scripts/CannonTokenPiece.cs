using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public enum CANNONSTATE { UNREADY, READY, MOVE, ATTACK }
public class CannonTokenPiece : TokenPiece
{
    // Cannon token action rules:
    // UNREADY - cannon needs to be readied with 2 friendlies
    // READY - cannon uses 2 friendlies to ready; friendlies' actions are drained immediately; uses 1 action
    // MOVE - cannon uses 2 friendlies to move; friendlies' actions are drained if the player actually moves; uses 1 action; does NOT unready cannon
    // ATTACK - cannon uses 2 friendlies to attack; friendlies' actions are drained if the player actually attacks; uses 1 action; unreadies cannon

    [HideInInspector]
    public CANNONSTATE CurrentState;
    [HideInInspector]
    public CANNONSTATE PreviousState;
    [HideInInspector]
    public CANNONSTATE StartTurnState;

    [HideInInspector]
    public List<TokenPiece> FriendliesUsed;

    public override ID TokenID { get { return ID.ID_CANNON; } }

    protected override int MaxMoves { get { return MaxAttack; } }

    protected override int MaxAttack { get { return 9; } } // TODO: make this the size of the board

    protected override int MaxMovement { get { return 1; } }

    public override bool IsSelectable { get { return true; } }

    // TODO: need some sort of indicator that a cannon is unready! (Kevin's video shows it just being flipped upside down, blank)

    void Start()
    {
        InitializeToken();
    }

    protected override void InitializeToken()
    {
        base.InitializeToken();
        FriendliesUsed = new List<TokenPiece>();
        // Cannon is ready at the start of the game
        CurrentState = CANNONSTATE.READY;
        StartTurnState = CurrentState;
    }

    public void CancelReadyUp()
    {
        // Put the cannon back to the previous state
        CurrentState = PreviousState;

        // reset the friendlies
        foreach (var friendly in FriendliesUsed)
            friendly.Reset();

        // clear FriendliesUsed list
        FriendliesUsed.Clear();
    }

    public override void CreateSubMenu(GameObject menuContainer)
    {
        if (!CanOpenMenu)
        {
            DestroyImmediate(menuContainer);
            menuContainer = null;
            return;
        }
        // Ready
        // Move
        // Attack
        // Cancel

        _menuContainer = menuContainer;
        _menuContainer.transform.Translate(Vector2.up * 1.5f, Space.World);
        _menuContainer.transform.parent = transform;

        // Cannon - Ready, Move, Attack, Cancel
        #region Ready Button
        var readyButton = _menuContainer.transform.Find("FirstOption").GetComponent<Button>();
        // Change text to "READY"
        readyButton.gameObject.GetComponentInChildren<Text>().text = "READY";

        // Disable this button if we already ready
        if (CurrentState != CANNONSTATE.UNREADY)
            readyButton.interactable = false;
        else
            readyButton.onClick.AddListener(() => OnReadyActionSelected());
        #endregion

        #region Move
        var moveButton = _menuContainer.transform.Find("SecondOption").GetComponent<Button>();
        // Change text to "MOVE"
        moveButton.gameObject.GetComponentInChildren<Text>().text = "MOVE";

        // Disable this button if we aren't ready
        if (CurrentState == CANNONSTATE.UNREADY || Movement == 0)
            moveButton.interactable = false;
        else
            moveButton.onClick.AddListener(() => OnMoveActionSelected());
        #endregion

        #region Attack
        var attackButton = _menuContainer.transform.Find("ThirdOption").GetComponent<Button>();
        // Change text to "ATTACK"
        attackButton.gameObject.GetComponentInChildren<Text>().text = "ATTACK";
        // Disable this button if we aren't ready
        if (CurrentState == CANNONSTATE.UNREADY || Attack == 0)
            attackButton.interactable = false;
        else
            attackButton.onClick.AddListener(() => OnAttackActionSelected());
        #endregion

        #region Cancel
        var cancelButton = _menuContainer.transform.Find("FourthOption").GetComponent<Button>();
        // Change text to "CANCEL"
        cancelButton.gameObject.GetComponentInChildren<Text>().text = "CANCEL";
        cancelButton.onClick.AddListener(() => OnCancelActionSelected());
        #endregion
    }

    public override void OnAttackActionSelected(GameObject sender = null)
    {
        // Set the state to ATTACK
        CurrentState = CANNONSTATE.ATTACK;

        // We need to find friendly units
        OnReadyActionSelected();
    }

    public override void OnMoveActionSelected()
    {
        // Set the state to MOVE
        CurrentState = CANNONSTATE.MOVE;

        // We need to find friendly units
        OnReadyActionSelected();
    }

    public override void OnCancelActionSelected()
    {
        // If we had friendlies ready to use their action for us, revert them
        foreach (var friend in FriendliesUsed)
        {
            friend.IsCannonFriend = false;
            friend.CanOpenMenu = true;
        }
        FriendliesUsed.Clear();

        base.OnCancelActionSelected();
    }

    public override void EndAction()
    {
        GameHelper.RemoveHighlights(this);
    }

    // The cannon can operate so long as there are enough friendly units nearby
    public override bool HasMoved()
    {
        return false;
    }

    // Make sure the StartTurnState is set
    public override void SetTokenStartPosition()
    {
        base.SetTokenStartPosition();
        StartTurnState = CurrentState;
    }

    public override void ResetTokenToStartTurn()
    {
        base.ResetTokenToStartTurn();
        GameHelper.RemoveHighlights(this);

        foreach (var friend in FriendliesUsed)
        {
            friend.IsCannonFriend = false;
            friend.CanOpenMenu = true;
        }
        FriendliesUsed.Clear();
        CurrentState = StartTurnState;
    }

    public void OnReadyActionSelected()
    {
        // We need to find two friends...

        // When we find two friends, we are ready

        if (OnAbilityTriggered != null)
            OnAbilityTriggered(this);
    }

    public void ReadyUpCannon()
    {
        // set the previous state
        // PreviousState = CurrentState;

        // set the state to ready up
        //CurrentState = CANNONSTATE.READYUP;

        // TODO: we need to highlight friendlies
        // we need two in order to ready the cannon
        // only two that haven't used an action may help ready the cannon
    }

    public void UnreadyCannon()
    {

    }

    public bool OnFriendlySelected(TokenPiece token)
    {
        // add the token to the list
        FriendliesUsed.Add(token);

        // we're friends now!
        token.BefriendCannon();

        // do we have enough friends?
        return FriendliesUsed.Count == 2;
    }

    public override List<int> CalculateJeopardy(BoardPiece[] boardPieces)
    {
        // APPROACH
        // Again, ignore 'taking into account 2 actions'
        
        // 1) If we are READY, we can move/attack immediately. If we're UNREADY, we need to see if we have 2 turns...(jesus...)
        // 2) Check to make sure we have enough friendly units to use
        // 3) Add threat to ATTACKABLE locations (you can't really 'move' to an enemy so it shouldn't be threat)



        return new List<int>();
    }

    public override List<int> FindMoveableTiles(BoardPiece[] boardPieces)
    {
        // Immediate grid around (as long as there are no friendlies)
        var result = new List<int>();
        var currentPieceIndex = boardPieces[BoardPosition];
        Vector2Int gridPosition = GameHelper.BoardToGridPosition(BoardPosition);
        int newCol, newRow;
        for (var i = -Movement; i <= Movement; ++i)
        {
            newCol = gridPosition.x + i;
            if (newCol < 0)
                continue;
            for (int j = -Movement; j <= Movement; ++j)
            {
                newRow = gridPosition.y + j;
                if (newRow >= 0)
                {
                    var index = GameHelper.GridToBoardPosition(newCol, newRow);
                    if (index >= boardPieces.Length) continue;
                    if (boardPieces[index].Piece != null && boardPieces[index].Piece.PlayerOwner == PlayerOwner) continue;

                    // Since movement is only 1, simply return the result
                    result.Add(index);
                }
            }
        }
        return result;
    }

    public override List<int> FindAttackableTiles(BoardPiece[] boardPieces)
    {
        // Full direction horizontal, vertical, diagonal
        var SHORT_DIMENSIONS = 9; // TODO: make this static accessible
        var result = new List<int>();
        int currentPosition = BoardPosition;
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

                    var piecePosition = GameHelper.GridToBoardPosition(newCol, newRow);
                    var boardPiece = boardPieces[piecePosition];
                    // Highlight this position
                    boardPiece.ToggleSelectable(true, true);

                    // Is there someone at this position?                    
                    var gamePiece = boardPiece.Piece;
                    if (gamePiece != null)
                    {
                        // If it's an enemy, stop checking
                        if (gamePiece.PlayerOwner != PlayerOwner)
                        {
                            result.Add(piecePosition);
                            break;
                        }
                        // If it's a friend, stop checking and remove all highlights going forward, we can't attack this direction
                        else
                        {
                            for (int reversePosition = forwardPos; reversePosition >= 0; --reversePosition)
                            {
                                var reverseCol = col + (i * reversePosition);
                                var reverseRow = row + (j * reversePosition);
                                boardPieces[GameHelper.GridToBoardPosition(reverseCol, reverseRow)].ToggleSelectable(false);
                            }
                            break;
                        }
                    }                    
                    ++forwardPos;
                    result.Add(piecePosition);
                }
            }
        }
        return result;
    }
}
