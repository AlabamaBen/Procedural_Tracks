using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {

    public Road road;

    public GameObject Car;

    public float Start_Offset = 1.5f;

    private int i = 0;

    public void Start()
    {
        road.portal = this;

        ResetPosition();
    }


    public void ResetPosition()
    {
        transform.position = road.points[i] + road.transform.position + new Vector3(0f, road.point1H, 0f);
        transform.right = road.points[i + 1] - road.points[i];

        Car.transform.position = new Vector3(transform.position.x, transform.position.y + Start_Offset, transform.position.z);
        Car.transform.forward = transform.right;
        Car.transform.Translate(-Car.transform.forward * 4f);

        Car.GetComponent<Rigidbody>().velocity = Vector3.zero;

        Debug.Log("Reset_Position");

    }

}
