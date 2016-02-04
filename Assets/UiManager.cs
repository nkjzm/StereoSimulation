using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UiManager : MonoBehaviour 
{
	public bool isStart= false;


	//sound
	[SerializeField]
	private Dropdown soundName;
	[SerializeField]
	private Toggle iid;
	[SerializeField]
	private Slider iidcof;
	[SerializeField]
	private Toggle itd;



	//sim
	[SerializeField]
	private Slider MoveSpeed;
	[SerializeField]
	private Toggle onnatsu;
	[SerializeField]
	private Toggle highsound;


	//test
	[SerializeField]
	private Slider ViewAngle;
	[SerializeField]
	private Slider ViewResolution;
	[SerializeField]
	private Slider TestCount;
	[SerializeField]
	private Slider PrepareCount;


	[SerializeField]
	private Button testStart;

	[SerializeField]
	private Button simStart;

	void Start () {

		soundName.ClearOptions ();
		var list = new List<string> ();
		foreach (SoundManager.SoundName s in System.Enum.GetValues(typeof(SoundManager.SoundName))) {
			list.Add (s.ToString ());
		}
		soundName.AddOptions (list);
		soundName.value = soundName.options.Count;

		testStart.onClick.AddListener (() => {
			FindObjectOfType<TestManager>().InspectorAdapt(
				(int)ViewAngle.value,
				(int)ViewResolution.value,
				(int)PrepareCount.value,
				(int)TestCount.value);	
			Init(false);
		});
		simStart.onClick.AddListener (() => {
			FindObjectOfType<SoundManager>().SpeedAdapt(
				MoveSpeed.value,
				onnatsu.isOn,
				highsound.isOn
			);	
			Init(true);
		});
	}

	public void Init(bool isSim){
		
		FindObjectOfType<SoundManager>().InspectorAdapt(
			(SoundManager.SoundName)System.Enum.Parse(typeof(SoundManager.SoundName),soundName.captionText.text),
			iid.isOn,
			iidcof.value,
			itd.isOn,
			isSim
		);	
		isStart = true;
		transform.GetChild (0).gameObject.SetActive (false);
	}

	void Update () 
	{
		if (Input.GetKeyDown (KeyCode.Escape)) {
			UnityEngine.SceneManagement.SceneManager.LoadScene ("Main");
		}
	}
}
