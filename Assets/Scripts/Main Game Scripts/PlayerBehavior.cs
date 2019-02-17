using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum PLAYERSTATE { IDLE, MOVE, ATTACK, SPECIAL }

/// <summary>
/// Player behavior.
/// </summary>
public class PlayerBehavior : MonoBehaviour
{
    #region Fields
    public List<TokenPiece> Tokens;                         // This player's pieces
    public GameObject MenuPrefab;                           // Container for a token sub-menu

    public PLAYERSTATE PlayerState;                         // What state the player is currently in

    private List<TokenPiece> _removedTokens;                // Pieces removed from the board
    private List<TokenPiece> _removedTokensAtTurn;          // Pieces removed from the board at the current turn

    private TokenPiece _initialActionToken;                 // The token that was played first (may not have finished its action, should remove action if player moves again)
    private TokenPiece _activeToken;                        // This player's currently active token
    private BoardManager _boardManager;                     // Reference to the board manager
    private bool _isMenuActive;                             // True if a menu is currently active
    #endregion

    #region Properties
    [HideInInspector]
    public TokenPiece ActiveToken                           // Active Token needs to be accessed, but not mutated anywhere else
    {
        get { return _activeToken; }
    }
    [HideInInspector]
    public TokenPiece InitialActionToken
    {
        set { _initialActionToken = value; }
        get { return _initialActionToken; }
    }
    [HideInInspector]
    public BoardManager BoardManager                        // BoardManager Needs to be set, but not accessed anywhere else
    {
        set { _boardManager = value; }
    }
    #endregion

    #region Initialization
    void Start()
    {
        foreach (TokenPiece token in Tokens)
        {
            token.PlayerOwner = this;
            token.OnTokenSelected += HandleTokenSelected;
            token.OnTokenAttacked += HandleTokenAttacked;
            token.OnMoveSelected = HandleMovementSelected;
            token.OnAttackSelected = HandleAttackSelected;
            token.OnCancelSelected = HandleCancelSelected;
            if (token.TokenID == ID.ID_SOLDIER)
                token.OnAbilityTriggered += HandleSoldierStanceSwitch;
            else if (token.TokenID == ID.ID_CANNON)
                token.OnAbilityTriggered += HandleCannonAbility;
        }
        _removedTokens = new List<TokenPiece>();
        _removedTokensAtTurn = new List<TokenPiece>();
        PlayerState = PLAYERSTATE.IDLE;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Moves the currently active token to the provided position
    /// </summary>
    public void MoveActiveTokenToPosition(Vector3 position)
    {
        // TODO
        // any/all animations are also handled here

        //ActiveToken.gameObject.transform.position = new Vector3(position.x, ActiveToken.transform.position.y, position.z);
        ActiveToken.MoveToken(position);
    }

    /// <summary>
    /// Ends the current action
    /// </summary>
    public void EndAction()
    {
        if (_activeToken != null)
            _activeToken.EndAction();
        _activeToken = null;
        _isMenuActive = false;
    }

    /// <summary>
    /// Resets the tokens.
    /// </summary>
    public void ResetTokens()
    {
        foreach (TokenPiece token in Tokens) token.Reset();
    }

    /// <summary>
    /// Prevents all tokens from moving/attacking
    /// </summary>
    public void DisableAllTokens()
    {
        foreach (var token in Tokens)
        {
            token.CanOpenMenu = false;
            token.EndAction();
        }
    }

    /// <summary>
    /// Sets the tokens to the start of the game (all becoming re-enabled)
    /// </summary>
    public void RestartTokens()
    {
        PlayerState = PLAYERSTATE.IDLE;
        foreach (var token in Tokens)
            token.gameObject.SetActive(true);
        _removedTokens.Clear();
        _removedTokensAtTurn.Clear();
    }

    /// <summary>
    /// Resets all tokens to their original position and restarts the turn
    /// </summary>
    public void ResetTurn()
    {
        // Restore all removed tokens from this turn, then clear the _removedTokensAtTurn
        foreach (var token in _removedTokensAtTurn)
        {
            _removedTokens.Remove(token);
            token.gameObject.SetActive(true);
        }
        _removedTokensAtTurn.Clear();

        // Go through all tokens and return them to their StartTurnPosition
        foreach (var token in Tokens)
        {
            token.ResetTokenToStartTurn();
            // Make sure this wasn't a completely removed token
            if (token.BoardPosition != -1)
                token.MoveToken(_boardManager.Pieces[token.BoardPosition].transform.position);
        }
    }

    public void SetBoardPositions()
    {
        foreach (var token in Tokens)
            token.SetTokenStartPosition();

        // We're done, so make the removed tokens official now
        _removedTokensAtTurn.Clear();
    }
    #endregion

    #region Event Handlers
    #region Move Button
    public void HandleMovementSelected()
    {
        // Destroy the submenu
        _activeToken.DestroySubMenu();
        _isMenuActive = false;

        // Set the state to move
        PlayerState = PLAYERSTATE.MOVE;

        // Determine all the possible moves
        var pieces = _boardManager.Pieces;
        var possibleMoves = _activeToken.FindMoveableTiles(pieces);
        _activeToken.PossibleMoves.Clear();
        foreach (var moveIndex in possibleMoves)
        {
            pieces[moveIndex].ToggleSelectable(true, false);
            _activeToken.PossibleMoves.Add(pieces[moveIndex]);
        }
    }
    #endregion

    #region Attack Button
    public void HandleAttackSelected()
    {
        // Destroy the submenu
        _activeToken.DestroySubMenu();
        _isMenuActive = false;

        // Set the state to move
        PlayerState = PLAYERSTATE.ATTACK;

        // Determine all the possible moves
        var pieces = _boardManager.Pieces;
        var possibleMoves = _activeToken.FindAttackableTiles(pieces);
        _activeToken.PossibleMoves.Clear();
        foreach(var attackIndex in possibleMoves)
        {
            pieces[attackIndex].ToggleSelectable(true, true);
            _activeToken.PossibleMoves.Add(pieces[attackIndex]);
        }
    }
    #endregion

    #region Cancel Button
    public void HandleCancelSelected()
    {
        _isMenuActive = false;
        _activeToken.DestroySubMenu();
        _activeToken = null;
        GameHelper.RemoveHighlights(_activeToken);

        // TO ADD LATER:
        // if we are a cannon, we need to revert the friendlies we selected back to normal (give them actions back)
    }
    #endregion

    #region Soldier Stance Button
    public void HandleSoldierStanceSwitch(TokenPiece token)
    {
        _isMenuActive = false;
        (token as SoldierTokenPiece).SwitchStance();
        _boardManager.EndActionKeepTokenTurn();
    }
    #endregion

    #region Token Selection
    /// <summary>
    /// Handles the token selected.
    /// </summary>
    /// <param name="token">The selected token.</param>
    public void HandleTokenSelected(TokenPiece token)
    {
        if (_boardManager.ActivePlayer == this && token.IsSelectable)
            HandleFriendlyTokenSelected(token);
        else
            HandleEnemyTokenSelected(token);
    }

    public void HandleFriendlyTokenSelected(TokenPiece token)
    {
        // Open the menu
        if (_activeToken != token && _activeToken != null)
        {
            // If it's the last action and we moved already, do not allow the player to select a different token (unless we are a cannon looking for a friend)
            if (_activeToken != null && _boardManager.ActionsRemaining == 1 && _activeToken.HasMoved())
                return;

            // If we have a menu active, remove it
            _activeToken.DestroySubMenu();


            if (_activeToken.TokenID == ID.ID_CANNON)
            {
                var cannon = _activeToken as CannonTokenPiece;

                // If we already have friendlies, we may be trying to cancel
                if (cannon.FriendliesUsed.Count == 2 && (cannon.CurrentState == CANNONSTATE.ATTACK || cannon.CurrentState == CANNONSTATE.MOVE))
                {
                    cannon.OnCancelActionSelected();
                    return;
                }

                // If the active token is trying to find a friend, he may have found one
                if (_boardManager.Pieces[token.BoardPosition].AbilitySelectable)
                {
                    Debug.Log("We have friends!");
                    HandleCannonFriendSelected(token, cannon);
                    return;
                }
            }

        } // We don't want multiple menus
        else if (_isMenuActive) return;

        // Clear any highlights
        GameHelper.RemoveHighlights(_activeToken);

        // The new token is the active one now (or the same as before)
        _activeToken = token;

        // Create a submenu for the newly active token
        var menu = Instantiate(MenuPrefab, _activeToken.transform.position, MenuPrefab.transform.rotation) as GameObject;
        _activeToken.CreateSubMenu(menu);
        if (menu != null)
            _isMenuActive = true;
    }

    public void HandleEnemyTokenSelected(TokenPiece token)
    {
        // We are the enemy. Is the player moving to/attacking us?
        if (_boardManager.ActivePlayer.ActiveToken != null && _boardManager.Pieces[token.BoardPosition].Selectable)
            _boardManager.OnBoardPieceSelected(_boardManager.Pieces[token.BoardPosition].gameObject);
    }
    #endregion

    /// <summary>
    /// Handles when the player is trying to select friends
    /// </summary>
    /// <param name="token">The potential friend</param>
    public void HandleCannonFriendSelected(TokenPiece token, CannonTokenPiece cannon)
    {
        GameHelper.RemoveHighlights(cannon);

        if (cannon.OnFriendlySelected(token))
        {
            // We have enough to ready/move/attack!
            // Set the cannon to ready/move/attack! (depending on our state)
            switch (cannon.CurrentState)
            {
                case CANNONSTATE.UNREADY:
                    {
                        // The cannon is now ready
                        cannon.CurrentState = CANNONSTATE.READY;

                        // Take away our friends' actions (because we're mean like that)
                        foreach (var friend in cannon.FriendliesUsed)
                        {
                            friend.CanOpenMenu = false;
                            friend.EndAction();
                        }

                        cannon.FriendliesUsed.Clear();

                        // This uses an action
                        _boardManager.EndAction();
                        break;
                    }
                case CANNONSTATE.MOVE:
                    {
                        // We can move now!
                        HandleMovementSelected();
                        //cannon.CurrentState = CANNONSTATE.LOADED;
                        break;
                    }
                case CANNONSTATE.ATTACK:
                    {
                        // We can attack now!
                        HandleAttackSelected();
                        break;
                    }
            }

        }
        else
            GameHelper.FindAdjacentFriendlyUnits(cannon, ref _boardManager.Pieces, Tokens);
    }

    /// <summary>
    /// Handles the enemy token attacked.
    /// </summary>
    /// <param name="token">The token being attacked.</param>
    public void HandleTokenAttacked(TokenPiece token)
    {
        // We are being attacked...
        // If this method is called, there is no mistaking that you are to be removed immediately!

        _boardManager.Pieces[token.BoardPosition].Piece = null;
        token.BoardPosition = -1;   // this may be a problem if I don't check that BoardPosition is valid...
        token.gameObject.SetActive(false);
        _removedTokens.Add(token);
        _removedTokensAtTurn.Add(token);
    }

    public void HandleCannonAbility(TokenPiece token)
    {
        var cannon = token as CannonTokenPiece;

        // Remove the submenu
        token.DestroySubMenu();
        _isMenuActive = false;

        //if (cannon.CurrentState == CANNONSTATE.LOADED) return;

        // remove highlights of active tokens
        GameHelper.RemoveHighlights(_activeToken);

        // handle picking friendly units
        GameHelper.FindAdjacentFriendlyUnits(token, ref _boardManager.Pieces, Tokens);

        // let the cannon know we're readying up
        cannon.ReadyUpCannon();

        // the cannon is the active token (the game should know we're looking for units)
        _activeToken = token;
    }


    public void HandleGeneralAttacked(TokenPiece token)
    {
        // I think this is already handled...

    }
    #endregion

    #region Private Methods
    #endregion
}
