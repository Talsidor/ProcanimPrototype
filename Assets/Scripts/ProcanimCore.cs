using UnityEngine;
using System.Collections;

namespace CardboardKeep.Procanim
{
	/// <summary>
	/// Base abstract class of Procanim objects
	/// </summary>
	abstract class ProcanimObject
	{
		protected Animator anim;
		protected string name;

		// Constructor
		public ProcanimObject(string name, Animator anim)
		{
			this.name = name;
			this.anim = anim;
		}

		abstract public void Update();
	}

	/// <summary>
	/// Extension class for controlling Mecanim Parameters
	/// </summary>
	class ProcanimParam : ProcanimObject
	{
		// MEMBER VARIABLES =====================

		// Weight Vars
		public float targetWeight;
		float maxWeight;
		const float weightLerpMultiplier = 5;
		float weight
		{
			get { return anim.GetFloat(name + "Weight"); }
			set { anim.SetFloat(name + "Weight", value); }
		}

		// Speed/Step Vars
		public float speed;
		float speedMultiplier;
		float step
		{
			get { return anim.GetFloat(name + "Speed"); }
			set { anim.SetFloat(name + "Speed", value); }
		}

		// MEMBER FUNCTIONS =====================

		// Constructor
		public ProcanimParam(string name, Animator anim, float maxWeight, float speedMultiplier)
			: base(name, anim)
		{
			this.name = name;
			this.anim = anim;
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

	/// <summary>
	/// Extension class for controlling Mecanim Layers
	/// </summary>
	class ProcanimLayer : ProcanimObject
	{
		// MEMBER VARIABLES =====================

		int layerIndex;

		// Weight Vars
		public float targetWeight;
		public float maxWeight { get; private set; }
		const float weightLerpMultiplier = 5;
		float weight
		{
			get { return anim.GetLayerWeight(layerIndex); }
			set { anim.SetLayerWeight(layerIndex, value); }
		}

		// MEMBER FUNCTIONS =====================

		// Constructor
		public ProcanimLayer(string name, Animator anim, float maxWeight)
			: base(name, anim)
		{
			this.name = name;
			this.anim = anim;
			this.maxWeight = maxWeight;

			layerIndex = anim.GetLayerIndex(name);
		}

		// Run this once per frame to update weight and speed params
		override public void Update()
		{
			// Update anim weight
			targetWeight = Mathf.Min(targetWeight, maxWeight);
			weight = Mathf.MoveTowards(weight, targetWeight, Time.deltaTime * weightLerpMultiplier);
		}
	}
}