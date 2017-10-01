using UnityEngine;
using System.Collections;

public class PlayButtonScript : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
    void Update()
    {

    }

    public void OnPlayButtonPressed()
    {
        var fadeTime = Camera.main.GetComponent<Fading>().BeginFade(1);
        Invoke("LoadGame", fadeTime);
    }

    private void LoadGame()
    {
        Application.LoadLevel(Application.loadedLevel + 1);
    }
}
