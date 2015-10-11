using UnityEngine;
using System.Collections;

public class GoToMouseHit : SuperBehaviour 
{
	void Update () 
	{
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
			transform.position = hit.point;
	}
}