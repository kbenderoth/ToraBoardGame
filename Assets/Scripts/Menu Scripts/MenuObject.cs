using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public delegate void OnMenuObjectSelected(MENU_ID ID);

public class MenuObject : MonoBehaviour 
{

	public MENU_ID 				MenuType;					// What menu this object will transition to
	public Action 				OnTransitionComplete;		// Action to trigger when the transition is complete
	public OnMenuObjectSelected	OnMenuSelected;				// Delegate to trigger when this MenuObject is selected
	private ClickHandler		_clickController;			// Handles clicking on this object
	
	// Use this for initialization
	void Start () 
	{
		_clickController = gameObject.GetComponent<ClickHandler>();
		_clickController.OnPressed = OnMenuObjectClicked;
	}

	public void OnMenuObjectClicked(GameObject sender)
	{
		OnMenuSelected (MenuType);
	}

	public void TransitionObject(Vector2 toPosition)
	{
		iTween.MoveTo (gameObject, iTween.Hash ("position", toPosition, "time", 1f, "oncomplete", "OnComplete"));
	}

	void OnComplete()
	{
		OnTransitionComplete();
	}
}
