using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HumanoidProceduralAnimation_TestController : SuperBehaviour 
{
	// Controller Variables
	Vector3 vel;
	float baseMoveSpeed = 3;
	float moveSpeed;
	float groundTimer, groundDelay = 0.3f;
	bool isGrounded;
	float bounce; // Blend this much of the Crouch layer to represent squash and followthrough in movements

	// Procedural Parameters
	List<ProcanimObject> procanims = new List<ProcanimObject>();
	ProcanimParam walkParam;
	ProcanimLayer crouchLayer;

	void Start () 
	{
		///	Construct Params:			Name				maxWeight	speedMultiplier
		walkParam	= new ProcanimParam("Walk",		this,	1,			0.5f);
		procanims.Add(walkParam);

		/// Construct Layers:			Name				maxWeight
		crouchLayer	= new ProcanimLayer("Crouch",	this,	0.8f);
		procanims.Add(crouchLayer);
	}
	
	void Update () 
	{
		// Dampen bounce every frame
		bounce = Mathf.Lerp(bounce, 0, Time.deltaTime * 2);

		// Grounded
		if(cc.isGrounded)
		{
			if (isGrounded == false && vel.y < 0)
			{
				Debug.Log("Landing bounce " + vel.y);
				bounce = Mathf.Abs(vel.y / 10);
			}
				
			groundTimer = groundDelay;
			isGrounded = true;
		}
		else
		{
			groundTimer -= Time.deltaTime;
			if (groundTimer < 0)
				isGrounded = false;
		}

		// Movement
		moveSpeed = baseMoveSpeed * (1 - (Input.GetAxis("Crouch") / 2));
		// Capture Player Input
		vel.x = Input.GetAxis("Horizontal") * moveSpeed;
		vel.z = Input.GetAxis("Vertical")   * moveSpeed;
		
		// Gravity
		vel += Physics.gravity * Time.deltaTime;
		if (cc.isGrounded)
		{
			vel.y = 0;
		}

		// Jumping
		if (isGrounded && Input.GetButtonDown("Jump"))
			vel.y = 5;

		// Drive Controller
		cc.Move(vel * Time.deltaTime);

		// Face Player towards movement direction
		if (cc.velocity.sqrMagnitude > 0.1f)
			transform.forward = Flatten(Vector3.RotateTowards(transform.forward, cc.velocity, Time.deltaTime * 15, 0));

		ProceduralAnimUpdate();
	}

	void ProceduralAnimUpdate()
	{
		// Walk Param
		walkParam.targetWeight	= Flatten(cc.velocity).magnitude;
		walkParam.speed			= Flatten(cc.velocity).magnitude;

		// Crouch Param
		if (isGrounded)
			crouchLayer.targetWeight = bounce + Input.GetAxis("Crouch");
		else
			crouchLayer.targetWeight = bounce;

		// Run Update logic on all params
		foreach (ProcanimObject p in procanims)
			p.Update();
	}

	// ==========================================================
	// PROCANIM OBJECTS BELOW - TODO: Break out to seperate class
	// ==========================================================

	class ProcanimParam : ProcanimObject
	{
		// MEMBER VARIABLES =====================

		// Weight Vars
		public float targetWeight;
		float maxWeight;
		const float weightLerpMultiplier = 5;
		float weight
		{
			get { return controller.anim.GetFloat(name + "Weight"); }
			set { controller.anim.SetFloat(name + "Weight", value); }
		}

		// Speed/Step Vars
		public float speed;
		float speedMultiplier;
		float step
		{
			get { return controller.anim.GetFloat(name + "Speed"); }
			set { controller.anim.SetFloat(name + "Speed", value); }
		}

		// MEMBER FUNCTIONS =====================

		// Constructor
		public ProcanimParam(string name, HumanoidProceduralAnimation_TestController controller, float maxWeight, float speedMultiplier) : base(name, controller)
		{
			this.name = name;
			this.controller = controller;
			this.maxWeight = maxWeight;
			this.speedMultiplier = speedMultiplier;
		}

		// Run this once per frame to update weight and speed params
		override public void Update()
		{
			// Update anim weight
			targetWeight = Mathf.Min(targetWeight, maxWeight);
			weight = Mathf.MoveTowards(weight, targetWeight, Time.deltaTime * weightLerpMultiplier);

			// Update anim speed
			step += speed * Time.deltaTime * speedMultiplier;
			step %= 1; // Unit wrap the anim step, so the blend tree repeats
		}
	}

	class ProcanimLayer : ProcanimObject
	{
		// MEMBER VARIABLES =====================

		int layerIndex;

		// Weight Vars
		public float targetWeight;
		float maxWeight;
		const float weightLerpMultiplier = 5;
		float weight
		{
			get { return controller.anim.GetLayerWeight(layerIndex); }
			set { controller.anim.SetLayerWeight(layerIndex, value); }
		}

		// MEMBER FUNCTIONS =====================

		// Constructor
		public ProcanimLayer(string name, HumanoidProceduralAnimation_TestController controller, float maxWeight) : base(name, controller)
		{
			this.name = name;
			this.controller = controller;
			this.maxWeight = maxWeight;

			layerIndex = controller.anim.GetLayerIndex(name);
		}

		// Run this once per frame to update weight and speed params
		override public void Update()
		{
			// Update anim weight
			targetWeight = Mathf.Min(targetWeight, maxWeight);
			weight = Mathf.MoveTowards(weight, targetWeight, Time.deltaTime * weightLerpMultiplier);
		}
	}

	abstract class ProcanimObject
	{
		protected HumanoidProceduralAnimation_TestController controller;
		protected string name;

		// Constructor
		public ProcanimObject(string name, HumanoidProceduralAnimation_TestController controller)
		{
			this.name = name;
			this.controller = controller;
		}

		abstract public void Update();
	}
}