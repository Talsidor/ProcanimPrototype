using UnityEngine;
using System.Collections;

public class ProcAnimValueDriver : MonoBehaviour {

	Animator anim;
	CharacterController cc;

	public Vector3 vel;

	float moveSpeed = 3;
	float speedIncrementor;
	float animSpeedMultiplier = 0.6f;

	float weightChangeSpeed = 5;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
		cc = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () {
		// Player Input
		vel.x = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
		vel.z = Input.GetAxis("Vertical")   * Time.deltaTime * moveSpeed;
		// Gravity
		vel.y += Physics.gravity.y * Time.deltaTime;
		if (cc.isGrounded) vel.y = 0;
		
		cc.Move(vel);

		// if moving
		if(cc.velocity.magnitude > 0.1f)
		{
			transform.forward = Vector3.RotateTowards(transform.forward, cc.velocity, Time.deltaTime * 10, 0);
			// Set MoveWeight to 1
			//anim.SetFloat("MoveWeight", Mathf.MoveTowards(anim.GetFloat("MoveWeight"), 1, Time.deltaTime * weightChangeSpeed));
		}
		// else not moving
		else
		{
			// Set MoveWeight to 0
			//anim.SetFloat("MoveWeight", Mathf.MoveTowards(anim.GetFloat("MoveWeight"), 0, Time.deltaTime * weightChangeSpeed));
		}

		// Set MoveWeight to move velocity
		anim.SetFloat("MoveWeight", 
			Mathf.MoveTowards(anim.GetFloat("MoveWeight"), 
			Mathf.Min(1, cc.velocity.magnitude), 
			Time.deltaTime * weightChangeSpeed));
		
		speedIncrementor += cc.velocity.magnitude * Time.deltaTime * animSpeedMultiplier;
		anim.SetFloat("MoveSpeed", speedIncrementor % 1);
	}
}
