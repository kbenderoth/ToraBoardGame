using UnityEngine;
using System.Collections;

public class UIController : MonoBehaviour {

	public BoardManager Manager;

    private bool _canRotate = true;

	// for now, we'll use GUI buttons
	void OnGUI()
	{
		// todo: convert these over to the new UI????????
        // Handle game over
		if (Manager.GameOver) 
		{
			if(!Manager._isOppositionGeneralDead)
				GUI.TextField(new Rect(Screen.width*0.5f-50,250,100,30),"Opposition Wins!");
			else if(!Manager._isChallengerGeneralDead)
				GUI.TextField(new Rect(Screen.width*0.5f-50,250,100,30),"Challenger Wins!");
			else
				GUI.TextField(new Rect(Screen.width*0.5f-50,250,100,30),"Tie Game!");

			if(GUI.Button(new Rect(Screen.width*0.5f-50,550,150,100),"Play Again"))
				Manager.ResetGame();
		}

        // Handle End Action and End Turn
        if (GUI.Button(new Rect(Screen.width * 0.25f, Screen.height * 0.82f, 150, 50), "END ACTION"))
            Manager.EndAction();
        if (GUI.Button(new Rect(Screen.width * 0.72f, Screen.height * 0.82f, 150, 50), "END TURN"))
            Manager.EndTurn();
        if (GUI.Button(new Rect(Screen.width * 0.45f, Screen.height * 0.82f, 200, 50), "SURRENDER"))
            Manager.Surrender();
        // TODO: Set stance based on decision in main menu (alternating, random, player choice)????
	}
	// Use this for initialization
	void Start () {
		Screen.fullScreen = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void EndAction()
	{
		Manager.EndAction ();
	}

	public void EndTurn()
	{
		Manager.EndTurn ();
	}

    public void RotateCamera90Degrees()
    {
        if (!_canRotate) return;
        _canRotate = false;

        // round to the nearest 90 degree
        int numQuarterRotations = (int)Mathf.Round(transform.rotation.z / 90f)+1;
        float zRotationInDegrees = numQuarterRotations * 90f;
        zRotationInDegrees -= zRotationInDegrees % 90;
        iTween.RotateTo(gameObject, iTween.Hash("z", zRotationInDegrees , "space", Space.Self, "oncompletetarget", gameObject, "oncomplete", "RestoreRotationButton", "tiime", 1f));
    }

    public void MoveCameraToSeat()
    {

    }

    public void RestoreRotationButton()
    {
        _canRotate = true;
    }

    public void UndoCurrentTurn()
    {
        // Return the pieces of the current player to their original positions

        // Return the pieces of the other player to the game, if destroyed

        // If a general was attacked, undo this (may need to change a bit of code, because right now the general being attacked immediately ends the game, should we allow this option?)

        // Reset everything

        Manager.ResetTurn();
        
    }
}
