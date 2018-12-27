using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SoldierTokenPiece : TokenPiece {

    // The Soldier's stance; Attack by default
    [HideInInspector]
    public bool IsDefense;

    [HideInInspector]
    public bool StartTurnDefense;

    // If the soldier can still switch stances (even if its move is used up)
    [HideInInspector]
    public bool CanSwitchStance;

    public Texture DefenseTexture; // The texture for the soldier to show when in defensive position (this may need to just be on the bottom of the token????)
    
	// Use this for initialization
	void Start () 
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


    public void SwitchStance()
    {
        if (!CanSwitchStance) return;

        // switch the soldier's stance
        // make sure to set the MAX ATTACK and MAX MOVEMENT to 0 in defense, and 1 in offense!
        IsDefense = !IsDefense;
        if(IsDefense)
        {
            // switch textures
            gameObject.GetComponent<Renderer>().material.mainTexture = DefenseTexture;

            // set movement to 0
            _maxMovement = 0;
            Movement = _maxMovement;
        }
        else
        {
            // switch textures
            gameObject.GetComponent<Renderer>().material.mainTexture = _DefaultImage;

            // set attack and movement to default (1 for movement)
            _maxMovement = 1;
            Movement = _maxMovement;
        }
        DestroySubMenu();
    }
}
