using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    #region Fields
    public Material TransparentMaterial;
    public BoardPiece[] Pieces;					// 1d array to represent a 2d array
    public GameObject ChallengerPrefab;			// Prefab
    public GameObject OppositionPrefab; 		// Prefab
    public GameObject ColliderCover;

    [HideInInspector]
    public int ActionsRemaining = 2; 			// when this number hits zero, the next player takes a turn

    private const int NUMROWS = 9;
    private const int NUMCOLS = 9;

    [HideInInspector]
    public bool _isChallengerGeneralDead = false;
    [HideInInspector]
    public bool _isOppositionGeneralDead = false;
    [HideInInspector]
    public bool GameOver = false;

    public PlayerBehavior Opposition;
    public PlayerBehavior Challenger;

    private PlayerBehavior _activePlayer;

    public Text CurrentPlayerText;
    public Text CurrentActionText;


    private System.Random _randomGenerator;
    #endregion

    #region Properties
    [HideInInspector]
    public PlayerBehavior ActivePlayer
    {
        get { return _activePlayer; }
    }
    #endregion

    #region Initialization
    void Start()
    {
        _randomGenerator = new System.Random((int)DateTime.Now.Ticks & 0x0000FFFF);

        // by default, Challenger will be first
        _activePlayer = Challenger;

        foreach (BoardPiece piece in Pieces)
        {
            piece.Init();
            piece.TransparentMaterial = TransparentMaterial;
            piece.OnPieceSelected += OnBoardPieceSelected;
        }

        Opposition.BoardManager = Challenger.BoardManager = this;

        // Select stance based on menu option (alternating, random, loser choice)
        // For now, randomize
        GameHelper.SetGameTokensStartPosition(ref Opposition.Tokens, ref Pieces, _randomGenerator.NextDouble() > 0.5, true);
        GameHelper.SetGameTokensStartPosition(ref Challenger.Tokens, ref Pieces, _randomGenerator.NextDouble() > 0.5, false);

        // Set the active player text
        CurrentPlayerText.text = _activePlayer.gameObject.name.ToUpper();
        CurrentPlayerText.CrossFadeColor(Color.red, 0f, true, false);

        // Set the current action text
        CurrentActionText.text = (Math.Min(3 - ActionsRemaining, 2)).ToString();

        // Determine initial threat levels
        GameHelper.DetermineThreatLevel(Opposition.Tokens, Challenger.Tokens, ref Pieces);
    }
    #endregion

    /// <summary>
    /// Raises the board piece selected event.
    /// </summary>
    /// <param name="piece">Piece GameObject passed in the delegate..</param>
    public void OnBoardPieceSelected(GameObject piece)
    {
        if (_activePlayer.ActiveToken == null)
        {
            Debug.Log("Error: No token selected");
            return;
        }
        if ((_activePlayer.PlayerState == PLAYERSTATE.MOVE && _activePlayer.ActiveToken.Movement == 0) || (_activePlayer.PlayerState == PLAYERSTATE.ATTACK && _activePlayer.ActiveToken.Attack == 0))
        {
            Debug.Log("Game: Not Enough Moves");
            return;
        }

        // Find the board position the player selected
        int newBoardPosition = Array.FindIndex(Pieces, p => p.gameObject == piece);

        // Did we select an empty attacking piece?
        if(_activePlayer.PlayerState == PLAYERSTATE.ATTACK && !Opposition.Tokens.Exists(p => p.BoardPosition == newBoardPosition) && !Challenger.Tokens.Exists(p => p.BoardPosition == newBoardPosition))
            return;

        // remove the board highlights
        GameHelper.RemoveHighlights(_activePlayer.ActiveToken);

        
        // Move to the new location if we are moving
        if (_activePlayer.PlayerState == PLAYERSTATE.MOVE)
            MoveActiveToken(newBoardPosition);

        // Attack if we landed on an enemy or chose to attack (Cannon cannot land on an enemy)
        if((_activePlayer.PlayerState == PLAYERSTATE.MOVE && _activePlayer.ActiveToken.TokenID != ID.ID_CANNON) || _activePlayer.PlayerState == PLAYERSTATE.ATTACK)
            AttackWithActiveToken(newBoardPosition);

        // Did we already have an initial action?
        if (_activePlayer.InitialActionToken != null && ActionsRemaining == 2 && _activePlayer.ActiveToken != _activePlayer.InitialActionToken)
        {
            // We just lost the action from the initial token
            _activePlayer.InitialActionToken.EndAction();
            _activePlayer.InitialActionToken = null;
            ActionsRemaining--;
        }
        // remove the next action if we are out of moves
        if (Math.Max(_activePlayer.ActiveToken.Attack, _activePlayer.ActiveToken.Movement) <= 0)
            EndAction();
        else
        {
            // Is this our first move? Did we use our entire action?
            if (ActionsRemaining == 2)
            {
                // Since this token used a move, add it as the first token action
                _activePlayer.InitialActionToken = _activePlayer.ActiveToken;
            }
            GameHelper.DetermineSelectableTiles(_activePlayer.ActiveToken, _activePlayer.Tokens, (_activePlayer == Challenger) ? Opposition.Tokens : Challenger.Tokens, ref Pieces);
        }
    }

    // HELPERS
    void ResetActions()
    {
        _activePlayer.ResetTokens();
    }

    /// <summary>
    /// Ends the turn.
    /// </summary>
    public void EndTurn()
    {
        if (_activePlayer.ActiveToken != null)
            _activePlayer.EndAction();
        ActionsRemaining = 2;

        _activePlayer = (_activePlayer == Opposition) ? Challenger : Opposition;


        // Memorize the board positions
        Opposition.SetBoardPositions();
        Challenger.SetBoardPositions();

        ResetActions();

        // If a general is dead and it's the end of the opposition's turn, it's game over. If both are dead it's a tie
        if (_activePlayer == Challenger && (_isChallengerGeneralDead || _isOppositionGeneralDead))
            GameOverScreen();

        // Set the active player text
        CurrentPlayerText.text = _activePlayer.gameObject.name.ToUpper();
        CurrentPlayerText.CrossFadeColor(_activePlayer == Opposition ? Color.white : Color.red, 0f, true, false);

        // Set the current action text
        CurrentActionText.text = (Math.Min(3 - ActionsRemaining, 2)).ToString();
    }

    public void ResetTurn()
    {
        // Call the active player first to restore the removed tokens
        _activePlayer.ResetTurn();

        if (_activePlayer == Opposition) Challenger.ResetTurn(); else Opposition.ResetTurn(); 

        // Give back the actions
        ActionsRemaining = 2;
        
        // Set the current action text
        CurrentActionText.text = (Math.Min(3 - ActionsRemaining, 2)).ToString();
    }

    /// <summary>
    /// Prevents all tokens from moving/attacking
    /// </summary>
    public void DisableAllTokens()
    {
        ActionsRemaining = 0;
        _activePlayer.DisableAllTokens();
    }

    /// <summary>
    /// Ends the action.
    /// </summary>
    public void EndAction()
    {
        ActionsRemaining--;
        _activePlayer.EndAction();
        if (ActionsRemaining <= 0)
            DisableAllTokens();

        // Set the current action text
        CurrentActionText.text = (Math.Min(3 - ActionsRemaining, 2)).ToString();
    }

    /// <summary>
    /// Ends the action without affecting the 
    /// token's number of attacks and moves.
    /// </summary>
    public void EndActionKeepTokenTurn()
    {
        ActionsRemaining--;
        if (ActionsRemaining <= 0)
            DisableAllTokens();

        // Set the current action text
        CurrentActionText.text = (Math.Min(3 - ActionsRemaining, 2)).ToString();
    }

    /// <summary>
    /// Resets the game.
    /// </summary>
    public void ResetGame()
    {
        // reset the game
        GameOver = false;													// may be able to remove this?
        _isOppositionGeneralDead = false;
        _isChallengerGeneralDead = false;

        // GameHelper.RemoveHighlights(_selectedToken);                  // may be able to move this?
        ActionsRemaining = 2;

        if (_activePlayer.ActiveToken != null)
            GameHelper.RemoveHighlights(_activePlayer.ActiveToken);

        Challenger.ResetTokens();
        Opposition.ResetTokens();

        ColliderCover.SetActive(false);

        // TODO: handle stance setup based on their decision in the main menu (alternating, random, loser choice)???

        // For now, randomize
        GameHelper.SetGameTokensStartPosition(ref Challenger.Tokens, ref Pieces, _randomGenerator.NextDouble() > 0.5, false);
        GameHelper.SetGameTokensStartPosition(ref Opposition.Tokens, ref Pieces, _randomGenerator.NextDouble() > 0.5, true);

        _activePlayer = Challenger;

        // Set the active player text
        CurrentPlayerText.text = _activePlayer.gameObject.name.ToUpper();
        CurrentPlayerText.CrossFadeColor(Color.red, 0f, true, false);

        // Set the current action text
        CurrentActionText.text = (Math.Min(3 - ActionsRemaining,2)).ToString();

    }

    /// <summary>
    /// Sets the Game Over screen.
    /// </summary>
    public void GameOverScreen()
    {
        // make a collider in front of the camera so nobody accidentally presses a button
        // GUI will show game over screen
        // GUI will reset game on keypress
        GameOver = true;
        ColliderCover.SetActive(true);
    }

    /// <summary>
    /// Forces the active player to surrender
    /// </summary>
    public void Surrender()
    {
        if (_activePlayer == Opposition) _isOppositionGeneralDead = true;
        else _isChallengerGeneralDead = true;

        GameOver = true;
        ColliderCover.SetActive(true);
    }

    private void MoveActiveToken(int newBoardPosition)
    {
        int newRow = newBoardPosition / NUMCOLS;
        int newCol = newBoardPosition % NUMCOLS;

        int row = _activePlayer.ActiveToken.BoardPosition / NUMCOLS;
        int col = _activePlayer.ActiveToken.BoardPosition % NUMCOLS;
        int distance = GameHelper.CalculateAbsoluteDistance(newCol, newRow, col, row);

        // unless we're the cannon, reduce the distance to attack)
        if(_activePlayer.ActiveToken.TokenID != ID.ID_CANNON)
            _activePlayer.ActiveToken.Attack -= distance;
        _activePlayer.ActiveToken.Movement -= distance;
        _activePlayer.ActiveToken.BoardPosition = newBoardPosition;
        Vector3 positionedPiece = Pieces[_activePlayer.ActiveToken.BoardPosition].transform.position;

        // If the active piece is a cannon, use up the friendlies' actions
        ClearFriendlyActions();

        // PERFORM A MOVEMENT ANIMATION
        _activePlayer.MoveActiveTokenToPosition(positionedPiece);
    }

    private void AttackWithActiveToken(int enemyPosition)
    {
        var enemy = (Opposition == _activePlayer) ? Challenger : Opposition;
        var enemyAtNewPosition = enemy.Tokens.Find(p => p.BoardPosition == enemyPosition);
        if (enemyAtNewPosition != null)
        {
            // If the active piece is a cannon, use up the friendlies' actions
            ClearFriendlyActions();            

            // check if there's a pesky soldier in defense in the way!
            if (DefendingSoldierBlocking(enemyAtNewPosition, enemy))
                return;
            else
            {
                // If we attacked a general, we need to find out who's general and if we need to remove it or not
                if (enemyAtNewPosition.TokenID == ID.ID_GENERAL)
                    AttackGeneral();
                else    											// Even if the general is attacked, he can still move freely
                    enemy.HandleTokenAttacked(enemyAtNewPosition);

                // If the token attacked, it cannot do anything anymore (and you lose your action, HA)
                _activePlayer.ActiveToken.Attack = 0;
                _activePlayer.ActiveToken.Movement = 0;
            }
        }
    }

    private bool DefendingSoldierBlocking(TokenPiece target, PlayerBehavior targetOwner)
    {
        // We can only block arrows...
        if (target.TokenID != ID.ID_SOLDIER)
        {
            // Check for defending soldiers
            var defendingSoldier = GameHelper.IsDefendingSoldierBlocking(_activePlayer.ActiveToken, target, targetOwner.Tokens);
            if (defendingSoldier != null)
            {
                // Destroy the defending soldier INSTEAD of the target!
                targetOwner.HandleTokenAttacked(defendingSoldier);

                // If the token attacked, it cannot do anything anymore (and you lose your action, HA)
                _activePlayer.ActiveToken.Attack = 0;
                _activePlayer.ActiveToken.Movement = 0;

                return true;
            }

        }
        return false;
    }

    private void ClearFriendlyActions()
    {
        if (_activePlayer.ActiveToken.TokenID == ID.ID_CANNON)
        {
            var cannonPiece = _activePlayer.ActiveToken as CannonTokenPiece;
            foreach (var friendly in cannonPiece.FriendliesUsed)
            {
                friendly.EndAction();
                friendly.CanOpenMenu = false;
                if (friendly.TokenID == ID.ID_SOLDIER)
                    (friendly as SoldierTokenPiece).CanSwitchStance = false;
                
            }
            cannonPiece.FriendliesUsed.Clear();

            // Attacking unreadies a cannon
            if (cannonPiece.CurrentState == CANNONSTATE.ATTACK)
                cannonPiece.CurrentState = CANNONSTATE.UNREADY;
        }
    }

    private void AttackGeneral()
    {
        if (Opposition == _activePlayer) 					// If the Challenger's General dies, It is officially Game Over
        {
            _isChallengerGeneralDead = true;
            GameOverScreen();
        }
        else   												// If the Opposition's General dies, Opposition has one last chance to get a Draw
            _isOppositionGeneralDead = true;
    }
}
