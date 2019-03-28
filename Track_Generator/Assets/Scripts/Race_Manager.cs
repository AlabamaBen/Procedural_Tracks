using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Race_Manager : MonoBehaviour {



	public Road _road ; 

	// Use this for initialization
	void Start () {

		_road.Seed = Random.Range(1, 255);

		_road.Refresh();
	}
	
	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown(KeyCode.R))
		{
			_road.Seed = Random.Range(1, 255);
			_road.Refresh();
			Debug.Log("Road Refreshed");
		}
		
	}

}
