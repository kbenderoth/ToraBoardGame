using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public enum ID { ID_ARCHER, ID_SOLDIER, ID_CANNON, ID_GENERAL };
public delegate void OnSelectedTokenPiece(TokenPiece Token);

public class TokenPiece : MonoBehaviour
{
    [HideInInspector]
    public int BoardPosition;
    public ID TokenID;

    protected Texture _DefaultImage;

    protected int _maxAttack;
    protected int _maxMovement;

    [HideInInspector]
    public bool IsCannonFriend;

    [HideInInspector]
    public bool CanOpenMenu;

    private int _attack;
    [HideInInspector]
    public int Attack
    {
        get { return _attack; }
        set { _attack = value; } // hopefully won't need this!!!!!
    }

    private int _movement; // player must move before attacking, player cannot attack before moving
    [HideInInspector]
    public int Movement
    {
        get { return _movement; }
        set { _movement = value; } // hopefully won't need this!!!!!
    }


    private List<BoardPiece> _possibleMoves = new List<BoardPiece>();
    [HideInInspector]
    public List<BoardPiece> PossibleMoves
    {
        get { return _possibleMoves; }
        set { _possibleMoves = value; }
    }


    protected GameObject _menuContainer;

    [HideInInspector]
    public Vector3 StartGamePosition;       // This is an actual Vector3 Position

    [HideInInspector]
    public int StartTurnPosition;           // This is a board position

    public OnSelectedTokenPiece OnTokenSelected;
    public OnSelectedTokenPiece OnTokenAttacked;
    public OnSelectedTokenPiece OnAbilityTriggered;

    public Action OnMoveSelected;
    public Action OnAttackSelected;
    public Action OnCancelSelected;

    protected ClickHandler ClickerHandler;

    void Start()
    {
        InitializeToken();
    }

    protected virtual void InitializeToken()
    {
        ClickerHandler = GetComponent<ClickHandler>();
        ClickerHandler.OnPressed += OnSelected;
        ClickerHandler.OnRPressed += OnAttackActionSelected;
        ClickerHandler.OnMPressed += OnTriggered;

        StartGamePosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);


        switch (TokenID)
        {
            case ID.ID_ARCHER:
                _maxMovement = _maxAttack = 2;
                break;
            case ID.ID_CANNON:
                // movement and attacking is only applicable if 2 friendly pieces are adjacent to it
                _maxMovement = 1;
                _maxAttack = 9;
                break;
            case ID.ID_GENERAL:
                _maxMovement = 2;
                _maxAttack = 1;
                break;
            case ID.ID_SOLDIER:
                // Soldier can only attack through movement
                _maxMovement = 1;
                _maxAttack = 0;
                break;
        }
        _attack = _maxAttack;
        _movement = _maxMovement;

        _DefaultImage = GetComponent<Renderer>().material.mainTexture;
        IsCannonFriend = false;
        CanOpenMenu = true;
        SetTokenStartPosition();
    }

    public void Reset()
    {
        _attack = _maxAttack;
        _movement = _maxMovement;
        IsCannonFriend = false;
        CanOpenMenu = true;
    }

    /// <summary>
    /// Completes the current move by this token [attack, movement or ability]
    /// </summary>
    /// <returns>True if this token can no longer move or attack. False otherwise.</returns>
    public bool CompleteMove()
    {
        --_attack;
        --_movement;
        if (_attack == 0 && _movement == 0)
            return true;
        return false;
    }

    /// <summary>
    /// Checks if this token has made a move already
    /// </summary>
    /// <returns>
    ///     <True>Player moved this action</True>
    ///     <False>Player did not move this action</False>
    /// </returns>
    public virtual bool HasMoved()
    {
        return _movement != _maxMovement && _attack != _maxAttack;
    }

    public virtual void EndAction()
    {
        _attack = 0;
        _movement = 0;
        IsCannonFriend = false;
        DestroySubMenu();

        GameHelper.RemoveHighlights(this);
    }

    public void RestartToPosition()
    {
        _attack = _maxAttack;
        _movement = _maxMovement;
        transform.position = new Vector3(StartGamePosition.x, StartGamePosition.y, StartGamePosition.z);
    }

    public void MoveToken(Vector3 position)
    {
        transform.position = new Vector3(position.x, transform.position.y, position.z);
    }

    public virtual void SetTokenStartPosition()
    {
        // If Undo is pressed, this token will return to the set BoardPosition
        var newBoardPos = BoardPosition;
        StartTurnPosition = newBoardPos;
    }

    public virtual void ResetTokenToStartTurn()
    {
        Reset();
        GameHelper.RemoveHighlights(this);
        BoardPosition = StartTurnPosition;
    }

    public void BefriendCannon() { IsCannonFriend = true; CanOpenMenu = false; }

    public void SwitchReadyPosition()
    {
        transform.Rotate(Vector3.up, 180f);
    }

    public virtual void CreateSubMenu(GameObject menuContainer)
    {
        if (!CanOpenMenu)
        {
            DestroyImmediate(menuContainer);
            menuContainer = null;
            return;
        }
        _menuContainer = menuContainer;
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
        if (_movement <= 0)
            moveButton.interactable = false;
        else
            moveButton.onClick.AddListener(() => OnMoveActionSelected());
        #endregion

        #region Attack
        var attackButton = _menuContainer.transform.Find("SecondOption").GetComponent<Button>();
        // Change text to "ATTACK"
        attackButton.gameObject.GetComponentInChildren<Text>().text = "ATTACK";
        // Disable this button if there aren't enough attacks
        if (_attack <= 0)
            attackButton.interactable = false;
        else
            attackButton.onClick.AddListener(() => OnAttackActionSelected());
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

    public void DestroySubMenu()
    {
        if (_menuContainer != null)
        {
            DestroyImmediate(_menuContainer);
            _menuContainer = null;
        }
    }

    /// <summary>
    /// Method to handle when the player chooses to MOVE via Token Menu
    /// </summary>
    public virtual void OnMoveActionSelected()
    {
        // Tell the Player we're ready to move
        if (OnMoveSelected != null)
            OnMoveSelected();
    }

    /// <summary>
    /// Method to handle when the player chooses to ATTACK via Token Menu
    /// </summary>
    public virtual void OnAttackActionSelected(GameObject sender = null)
    {
        // Tell the Player we're ready to attack
        if (OnAttackSelected != null)
            OnAttackSelected();
    }

    /// <summary>
    /// Method to handle when the player chooses to CANCEL via Token Menu
    /// </summary>
    public virtual void OnCancelActionSelected()
    {
        if (OnCancelSelected != null)
            OnCancelSelected();
    }

    void OnSelected(GameObject sender)
    {
        if (OnTokenSelected != null)
            OnTokenSelected(this);
    }

    void OnAttacked(GameObject sender)
    {
        if (OnTokenAttacked != null)
            OnTokenAttacked(this);
    }

    void OnTriggered(GameObject sender)
    {
        if (OnAbilityTriggered != null)
            OnAbilityTriggered(this);
    }
}
