using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Follow : MonoBehaviour {

	public Transform target_position;
    public Transform target_lookat;

    public bool look_at = true; 

	public float smoothSpeed = 0.125f;
	public Vector3 offset;

	void FixedUpdate ()
	{
		Vector3 desiredPosition = target_position.position + offset;
		Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
		transform.position = smoothedPosition;
		if(look_at)
		{
			transform.LookAt(target_lookat);
		}
	}
}
