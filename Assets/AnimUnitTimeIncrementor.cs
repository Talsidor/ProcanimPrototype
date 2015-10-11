using UnityEngine;
using System.Collections;

public class AnimUnitTimeIncrementor : MonoBehaviour {

	Animator anim;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		anim.SetFloat("UnitTime", Time.time % 1);
	}
}
