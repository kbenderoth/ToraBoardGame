using UnityEngine;
using System.Collections;

public class LookAtCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    // Make sure the menu ALWAYS looks at the camera
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                          Camera.main.transform.rotation * Vector3.up);
	}
}
