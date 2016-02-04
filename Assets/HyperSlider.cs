using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HyperSlider : MonoBehaviour {

	private Slider s;
	[SerializeField]
	private Text t;

	void Start () {
		s = GetComponent<Slider> ();
	}
	
	void Update () 
	{
		t.text = s.value.ToString();
	}
}
