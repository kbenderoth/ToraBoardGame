using UnityEngine;
using System.Collections;

public class Fading : MonoBehaviour 
{
    public Texture2D FadeOutTexture;        // Texture to overlay the screen
    public float FadeSpeed = 0.8f;          // how fast (in seconds) the fade is

    private int _drawDepth = -1000;         // the texture's order in the draw hierarchy
    private float _alpha = 1f;              // the texture's alpha value
    private int _fadeDir = -1;              // the direction to fade: in = -1 out = 1
    private bool _isfading = false;         // let the game know if we're fading

    void OnGUI()
    {
        // fade out/in the alpha using a direction, speed, and delta time
        _alpha += _fadeDir * FadeSpeed * Time.deltaTime;

        // clamp the number between 0 and 1
        _alpha = Mathf.Clamp01(_alpha);

        // set the color of the texture
        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, _alpha);           // set the alpha
        GUI.depth = _drawDepth;                                                         // make the texture on top
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), FadeOutTexture);   // draw the texture to fit the screen
    }
	
    // sets fadeDir to the param making the scene fade
    public float BeginFade(int direction)
    {
        _isfading = true;
        _fadeDir = direction;
        return FadeSpeed;                   // return the fadespeed variable so it's easy to time the loading
    }

    void OnLevelWasLoaded()
    {
        if(_isfading)
        {
            BeginFade(-1);  // Fade In
            _isfading = false;
        }
            
    }
}
