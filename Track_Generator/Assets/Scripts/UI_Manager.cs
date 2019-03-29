using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour {


	public GameObject Menu; 

	public Slider slider_XRadius;
	public Slider slider_ZRadius;
	public Slider slider_Perlin_FRQ;
	public Slider slider_Perlin_MGT;
	public Slider slider_Sin_FRQ;
	public Slider slider_Sin_MGT;

	public Road road;



	// Use this for initialization
	void Start () {

		Update_Slider();

		Menu.SetActive(false);
		
	}
	
	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Toggle_Menu();
		}
	}

	public void Toggle_Menu()
	{
			Menu.SetActive(!Menu.activeInHierarchy);
			Update_Slider();
	}

	public void Randomize()
	{
		road.Randomize_Parameters(Random.Range(0,255));
		Update_Slider();
	}

	public void Generate()
	{
		Update_Road_Values();
		road.Refresh();
		Debug.Log("Road Refreshed");
	}

	void Update_Slider()
	{
		slider_XRadius.value = road.Cos_MGT;
		slider_ZRadius.value = road.Sin_MGT;
		slider_Perlin_FRQ.value = road.Amplitude_Perlin_Zoom;
		slider_Perlin_MGT.value = road.Amplitude_Perlin_MGT;
		slider_Sin_FRQ.value = road.Amplitude_Offset_FRQ;
		slider_Sin_MGT.value = road.Amplitude_Offset_MGT;
	}

	void Update_Road_Values()
	{
		road.Cos_MGT = slider_XRadius.value ;
		road.Sin_MGT = slider_ZRadius.value ;
		road.Amplitude_Perlin_Zoom = slider_Perlin_FRQ.value;
		road.Amplitude_Perlin_MGT = slider_Perlin_MGT.value;
		road.Amplitude_Offset_FRQ = slider_Sin_FRQ.value;
		road.Amplitude_Offset_MGT = slider_Sin_MGT.value;
	}




}
