using UnityEngine;
using System.Collections;

public delegate void OnClick(GameObject sender);

public class ClickHandler : MonoBehaviour
{
    public OnClick OnPressed;
    public OnClick OnReleased;
    public OnClick OnHeld;
    public OnClick OnRPressed;
    public OnClick OnRReleased;
    public OnClick OnRHeld;
    public OnClick OnMPressed;
    public OnClick OnHover;

    void OnMouseUp()
    {
        if (OnReleased != null)
        {
            if (Input.GetMouseButton(0))
                OnReleased(gameObject);
        }
        if (OnRReleased != null)
        {
            if (Input.GetMouseButton(1))
                OnRReleased(gameObject);
        }
    }

    //void OnMouseOver()
    //{
    //    if (OnHover != null)
    //    {
    //        OnHover(gameObject);
    //    }

    //    if (_lockClick) return;

    //    if (OnPressed != null && Input.GetMouseButton(0))
    //    {
    //        _lockClick = true;
    //        OnPressed(gameObject);
    //    }
    //    else if (OnRPressed != null && Input.GetMouseButton(1))
    //    {
    //        _lockClick = true;
    //        OnRPressed(gameObject);
    //    }
    //    else if (OnMPressed != null && Input.GetMouseButtonDown(2))
    //    {
    //        _lockClick = true;
    //        OnMPressed(gameObject);
    //    }
    //}

    void OnMouseDown()
    {
        if (OnPressed != null && Input.GetMouseButton(0))
            OnPressed(gameObject);
        else if (OnRPressed != null && Input.GetMouseButton(1))
            OnRPressed(gameObject);
        else if (OnMPressed != null && Input.GetMouseButtonDown(2))
            OnMPressed(gameObject);
    }
}