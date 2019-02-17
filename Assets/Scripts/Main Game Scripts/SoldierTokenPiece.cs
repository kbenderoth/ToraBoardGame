using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class SoldierTokenPiece : TokenPiece
{

    // The Soldier's stance; Attack by default
    [HideInInspector]
    public bool IsDefense;

    [HideInInspector]
    public bool StartTurnDefense;

    // If the soldier can still switch stances (even if its move is used up)
    [HideInInspector]
    public bool CanSwitchStance;

    public Texture DefenseTexture; // The texture for the soldier to show when in defensive position (this may need to just be on the bottom of the token????)

    public override ID TokenID { get { return ID.ID_SOLDIER; } }

    protected override int MaxAttack
    {
        get { return 1; } // TODO: may need to be 0. need to see if they have to move when they attack?
    }

    protected override int MaxMovement { get { return 1; } }

    protected override int MaxMoves { get { return MaxMovement; } }

    public override bool IsSelectable
    {
        get
        {
            return _attack != 0 || _movement != 0 || CanSwitchStance;
        }
    }

    // Use this for initialization
    void Start()
    {
        InitializeToken();
    }

    protected override void InitializeToken()
    {
        base.InitializeToken();
        IsDefense = false;
        CanSwitchStance = true;
        StartTurnDefense = IsDefense;
    }

    public override void EndAction()
    {
        CanSwitchStance = true;
        base.EndAction();
    }

    public override void CreateSubMenu(GameObject menuContainer)
    {
        if (!CanOpenMenu)
        {
            DestroyImmediate(menuContainer);
            menuContainer = null;
            return;
        }
        _menuContainer = menuContainer;
        //_menuContainer.GetComponent<Canvas>().worldCamera = Camera.main;
        _menuContainer.transform.Translate(Vector2.up * 1.5f, Space.World);
        _menuContainer.transform.parent = transform;

        // Archer & General - Move, Attack, Cancel
        // Soldier - Move, Switch Stance, Cancel
        // Cannon - Ready/Load, Move, Attack, Cancel
        #region Move
        var moveButton = _menuContainer.transform.Find("FirstOption").GetComponent<Button>();
        // Change text to "MOVE"
        moveButton.gameObject.GetComponentInChildren<Text>().text = "MOVE";
        // Disable this button if there aren't enough moves
        if (Movement <= 0)
            moveButton.interactable = false;
        else
            moveButton.onClick.AddListener(() => OnMoveActionSelected());
        #endregion

        #region Switch Stance
        var stanceButton = _menuContainer.transform.Find("SecondOption").GetComponent<Button>();
        // Change text to "SWITCH STANCE"
        stanceButton.gameObject.GetComponentInChildren<Text>().text = "SWITCH STANCE";
        stanceButton.onClick.AddListener(() => OnAbilityTriggered(this));
        #endregion

        #region Cancel
        var cancelButton = _menuContainer.transform.Find("ThirdOption").GetComponent<Button>();
        // Change text to "CANCEL"
        cancelButton.gameObject.GetComponentInChildren<Text>().text = "CANCEL";
        cancelButton.onClick.AddListener(() => OnCancelActionSelected());
        #endregion

        // Remove the Fourth Option
        _menuContainer.transform.Find("FourthOption").GetComponent<Button>().gameObject.SetActive(false);
    }

    public override void ResetTokenToStartTurn()
    {
        base.ResetTokenToStartTurn();
        CanSwitchStance = true;
        if (IsDefense != StartTurnDefense) SwitchStance();
    }

    public override void SetTokenStartPosition()
    {
        base.SetTokenStartPosition();
        StartTurnDefense = IsDefense;
    }

    public override List<int> CalculateJeopardy(BoardPiece[] boardPieces)
    {
        var threatPositions = new List<int>();

        // TODO: need to take into account of the fact that neighboring pieces may be moveable
        // For now, just get what's within the vicinity
        if (IsDefense) return threatPositions; // No threat when in defense

        // Mimick the FindMoveableTiles for now
        threatPositions = FindMoveableTiles(boardPieces);

        return threatPositions;
    }

    public void SwitchStance()
    {
        if (!CanSwitchStance) return;

        // switch the soldier's stance
        // make sure to set the MAX ATTACK and MAX MOVEMENT to 0 in defense, and 1 in offense!
        IsDefense = !IsDefense;
        if (IsDefense)
        {
            // switch textures
            gameObject.GetComponent<Renderer>().material.mainTexture = DefenseTexture;

            //// set movement to 0
            //MaxMovement = 0;
            //Movement = MaxMovement;
        }
        else
        {
            // switch textures
            gameObject.GetComponent<Renderer>().material.mainTexture = _DefaultImage;

            //// set attack and movement to default (1 for movement)
            //MaxMovement = 1;
            //Movement = MaxMovement;
        }
        DestroySubMenu();
    }

    public override List<int> FindMoveableTiles(BoardPiece[] boardPieces)
    {
        // If in defence, cannot move
        var result = new List<int>();
        if (!IsDefense)
        {
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
        }        
        return result;
    }

    public override List<int> FindAttackableTiles(BoardPiece[] boardPieces)
    {
        // Same thing as moving
        return FindMoveableTiles(boardPieces);
    }
}
