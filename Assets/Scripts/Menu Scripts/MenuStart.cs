using UnityEngine;
using System.Collections;

public class MenuStart : MonoBehaviour 
{
    public GameObject MenuBG;
    public GameObject CompanyBG;
    public GameObject Window;
    public Transform WindowEndPosition;

	void Start() 
    {
        iTween.FadeTo(MenuBG, 0f, 0f);

        Invoke("FadeCompanyIn", 1f);
	}

    private void FadeCompanyIn()
    {
        iTween.FadeTo(CompanyBG, iTween.Hash("alpha", 1f, "time", 1f));
        Invoke("FadeCompanyOut", 4f);
    }

    private void FadeCompanyOut()
    {
        iTween.FadeTo(CompanyBG, iTween.Hash("alpha", 0f, "time", 1f, "oncomplete", "FadeMenuIn", "oncompletetarget", gameObject));
    }

    private void FadeMenuIn()
    {
        iTween.FadeTo(MenuBG, iTween.Hash("alpha", 1f, "time", 1f, "oncomplete", "StartGame", "oncompletetarget", gameObject));
    }

    private void StartGame()
    {
        iTween.MoveTo(Window, iTween.Hash("position", WindowEndPosition.position, "time", 1f));
    }
}
