using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

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

    // TODO: need some sort of indicator that a cannon is unready! (Kevin's video shows it just being flipped upside down, blank)

	void Start () 
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
        foreach(var friendly in FriendliesUsed)
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
        var readyButton = _menuContainer.transform.FindChild("FirstOption").GetComponent<Button>();
        // Change text to "READY"
        readyButton.gameObject.GetComponentInChildren<Text>().text = "READY";
        
        // Disable this button if we already ready
        if (CurrentState != CANNONSTATE.UNREADY)
            readyButton.interactable = false;
        else
            readyButton.onClick.AddListener(() => OnReadyActionSelected()); 
        #endregion

        #region Move
        var moveButton = _menuContainer.transform.FindChild("SecondOption").GetComponent<Button>();
        // Change text to "MOVE"
        moveButton.gameObject.GetComponentInChildren<Text>().text = "MOVE";
        
        // Disable this button if we aren't ready
        if (CurrentState == CANNONSTATE.UNREADY || Movement == 0)
            moveButton.interactable = false;
        else
            moveButton.onClick.AddListener(() => OnMoveActionSelected());
        #endregion

        #region Attack
        var attackButton = _menuContainer.transform.FindChild("ThirdOption").GetComponent<Button>();
        // Change text to "ATTACK"
        attackButton.gameObject.GetComponentInChildren<Text>().text = "ATTACK";
        // Disable this button if we aren't ready
        if (CurrentState == CANNONSTATE.UNREADY || Attack == 0)
            attackButton.interactable = false;
        else
            attackButton.onClick.AddListener(() => OnAttackActionSelected());
        #endregion

        #region Cancel
        var cancelButton = _menuContainer.transform.FindChild("FourthOption").GetComponent<Button>();
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
}
