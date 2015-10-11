using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CardboardKeep;
using CardboardKeep.Procanim;

public class Procanim_TestController : SuperBehaviour 
{
	// IK Variables
	public bool headIK, handIK, footIK;
	public Transform lHandGrab, rHandGrab, lookTarget;
	GameObject lFootTarget, rFootTarget;
	LayerMask levelGeom;
	Quaternion lFootLocalRot, rFootLocalRot;
	float footYOffset = 0.1134f;

	// Controller Variables
	Vector3 vel;
	float baseMoveSpeed = 3;
	float moveSpeed;
	float groundTimer, groundDelay = 0.3f;
	bool isGrounded;
	float bounce; // Blend this much of the Crouch layer to represent squash and followthrough in movements

	// Procanim Parameters
	List<ProcanimObject> procanims = new List<ProcanimObject>();
	ProcanimParam walkParam;
	ProcanimLayer crouchLayer;

	void Start () 
	{
		// Construct params:			Name		Animator	maxWeight	speedMultiplier
		walkParam	= new ProcanimParam("Walk",		anim,		1,			0.5f);
		procanims.Add(walkParam);

		// Construct layers:			Name		Animator	maxWeight
		crouchLayer	= new ProcanimLayer("Crouch",	anim,		0.8f);
		procanims.Add(crouchLayer);

		// IK Init
		levelGeom = LayerMask.GetMask("LevelGeom");
		lFootLocalRot = anim.GetBoneTransform(HumanBodyBones.LeftFoot).localRotation;
		rFootLocalRot = anim.GetBoneTransform(HumanBodyBones.RightFoot).localRotation;
		lFootTarget = new GameObject("lFootTarget_RUNTIME");
		rFootTarget = new GameObject("rFootTarget_RUNTIME");
	}
	
	void Update () 
	{
		// Dampen bounce every frame
		bounce = Mathf.Lerp(bounce, 0, Time.deltaTime * 2);

		// Grounded
		if(cc.isGrounded)
		{
			// If this is the frame going from non-grounded to grounded, turn the current falling speed into anim bounce
			if (isGrounded == false && vel.y < 0)
			{
				bounce = Mathf.Min(crouchLayer.maxWeight, Mathf.Abs(vel.y / 5));
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

		ProcanimUpdate();
	}

	void ProcanimUpdate ()
	{
		// Walk Param
		walkParam.targetWeight	= isGrounded ? Flatten(cc.velocity).magnitude : 0;
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

	void OnAnimatorIK(int layerIndex)
	{
		// Head IK ===============================
		if (headIK)
		{
			anim.SetLookAtPosition(lookTarget.position);
			anim.SetLookAtWeight(1);
		}

		// Hand IK ===============================
		if (handIK)
		{
			anim.SetIKPosition(AvatarIKGoal.LeftHand, lHandGrab.position);
			anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);

			anim.SetIKPosition(AvatarIKGoal.RightHand, rHandGrab.position);
			anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
		}
		
		// Foot IK ===============================
		if (footIK)
		{
			// Left Foot =============

			// Visualize ray
			Debug.DrawLine(
				anim.GetBoneTransform(HumanBodyBones.LeftFoot).position + (Vector3.up * 0.25f),
				anim.GetBoneTransform(HumanBodyBones.LeftFoot).position + (Vector3.down * 0.5f));

			// If grounded test foot ray
			if (isGrounded && Physics.Raycast(anim.GetBoneTransform(HumanBodyBones.LeftFoot).position + (Vector3.up * 0.25f), Vector3.down, out hit, 0.5f, levelGeom))
			{
				// Position
				anim.SetIKPosition(AvatarIKGoal.LeftFoot, hit.point + (Vector3.up * footYOffset));
				//lFootTarget.transform.position = Vector3.MoveTowards(lFootTarget.transform.position, hit.point, Time.deltaTime * Vector3.Distance(lFootTarget.transform.position, hit.point));
				anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);

				// Rotation
				//anim.SetIKRotation(AvatarIKGoal.LeftFoot, lFootLocalRot * Quaternion.Euler(hit.normal));
				anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
			}
			else
			{
				anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
				anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
			}

			// Set left foot to target pos
			//anim.SetIKPosition(AvatarIKGoal.LeftFoot, lFootTarget.transform.position);

			// Right Foot ==============

			// Visualize ray
			Debug.DrawLine(
				anim.GetBoneTransform(HumanBodyBones.RightFoot).position + (Vector3.up * 0.25f),
				anim.GetBoneTransform(HumanBodyBones.RightFoot).position + (Vector3.down * 0.5f));

			// If grounded test foot ray
			if (isGrounded && Physics.Raycast(anim.GetBoneTransform(HumanBodyBones.RightFoot).position + (Vector3.up * 0.25f), Vector3.down, out hit, 0.5f, levelGeom))
			{
				// Position
				anim.SetIKPosition(AvatarIKGoal.RightFoot, hit.point + (Vector3.up * footYOffset));
				//rFootTarget.transform.position = Vector3.MoveTowards(rFootTarget.transform.position, hit.point, Time.deltaTime * Vector3.Distance(rFootTarget.transform.position, hit.point));
				anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

				// Rotation
				//anim.SetIKRotation(AvatarIKGoal.RightFoot, rFootLocalRot * Quaternion.Euler(hit.normal));
				anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
			}
			else
			{
				anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
				anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
			}
			// Set right foot to target pos
			//anim.SetIKPosition(AvatarIKGoal.RightFoot, rFootTarget.transform.position);
		}
	}
}