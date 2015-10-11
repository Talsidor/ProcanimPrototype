using UnityEngine;
using System.Collections;

public class SimpleCameraFollow : MonoBehaviour {

	public Transform target;

	Vector3 offset;

	// Use this for initialization
	void Start () {
		offset = transform.position - target.position;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = Vector3.MoveTowards(transform.position, target.position + offset, Vector3.Distance(transform.position, target.position + offset) * Time.deltaTime);
	}
}
