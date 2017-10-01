using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public enum MENU_ID {MAIN, PLAY, TUTORIAL, OPTIONS, STORE, SINGLE_PLAYER, MULTI_PLAYER};

[System.Serializable]
public class MenuPage
{
	public string 				Title;
	public List<MenuObject> 	MenuOptions;
	[HideInInspector]
	public Action				OnAllOptionTransitionsComplete;
	[HideInInspector]
	public OnMenuObjectSelected	OnMenuSelectionPressed;

	private int 				_completedTransitions = 0;

	public void OnOptionTransitionComplete()
	{
		_completedTransitions++;
		if (_completedTransitions == MenuOptions.Count) 
		{
			OnAllOptionTransitionsComplete();
			_completedTransitions = 0;
		}
	}

}

public class MainMenuController : MonoBehaviour 
{

	public Text Title;					// Title 
	public List<MenuPage> Menus;		// All menus in the game. This is sored by the MENU ID.
	private MenuPage _activeMenu;		// Menu currently on screen

	// Use this for initialization
	void Start () 
	{
		// Initialize all Menu Pages
		foreach (var menu in Menus) 
		{
			foreach(var menuOption in menu.MenuOptions)
			{
				//menuOption.OnMenuSelected 
			}
		}

		// by default, main menu will be selected, unless otherwise stated in the command arguments (when returning from a specific mode)
		_activeMenu = Menus[(int)MENU_ID.MAIN];


	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void OnMenuOptionSelected(MENU_ID ID)
	{
	}

	public void OnTransitionComplete()
	{

	}

	public void TransitionFromMenu()
	{
		// Make sure the menu isn't already transitioning
	}

	public void TransitionToMenu()
	{
		// Make sure the menu isn't already transitioning
	}

}
