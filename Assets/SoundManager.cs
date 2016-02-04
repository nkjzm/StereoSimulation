using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SoundManager : MonoBehaviour 
{
	public enum SoundName{
		WN_20_40Hz,
		WN_40_80Hz,
		WN_80_160Hz,
		WN_160_315Hz,
		WN_315_630Hz,
		WN_630_1250Hz,
		WN_1250_2500Hz,
		WN_2500_5kHz,
		WN_5k_10kHz,
		WN_10k_20kHz,
		WN_20k_40kHz,
		VOICE
	}

	public class SoundPreset
	{
		public float minFrequency;
		public float maxFrequency;
		public float aveFrequency;
		public SoundPreset(float min,float max)
		{
			minFrequency = min;
			maxFrequency = max;
			aveFrequency = (min+max)/2;
		}
	}

	private Dictionary<SoundName,SoundPreset> presets;

	[SerializeField]
	private SoundName soundName;

	[SerializeField]
	private bool isIid;

	[SerializeField, Range(0f,2f)]
	private float iidCoefficient;

	[SerializeField]
	private bool isItd;

	[SerializeField]
	private AudioSource right;
	[SerializeField]
	private AudioSource left;

	[SerializeField]
	private bool isSimulation;

	[SerializeField]
	private Transform SimulationSpeaker;
	private Transform camera;
	private Transform player;

	[SerializeField]
	private float moveSpeed;

	[SerializeField]
	private bool isVolumeRottof;
	[SerializeField]
	private bool isHighRottof;

	[SerializeField,Range(0,22000)]
	private float CotoffFrequency;
	[SerializeField,Range(1f,10f)]
	private float LowPassResonance;

	private List<AudioLowPassFilter> lowPassFilters;

	private float rollof = 1;

	private float frequency;

	private UiManager ui;

	public void InspectorAdapt(SoundName sn,bool isiid,float iidcoe,bool isitd,bool issim)
	{
		soundName = sn;
		isIid = isiid;
		iidCoefficient = iidcoe;
		isItd = isitd;
		isSimulation = issim;
	}

	public void SpeedAdapt(float m,bool v,bool h)
	{
		moveSpeed = m;
		isVolumeRottof = v;
		isHighRottof = h;
	}

	IEnumerator Start () 
	{

		presets = new Dictionary<SoundName,SoundPreset> ();
		presets.Add (SoundName.WN_20_40Hz,		new SoundPreset (20, 	40));
		presets.Add (SoundName.WN_40_80Hz, 		new SoundPreset (40, 	80));
		presets.Add (SoundName.WN_80_160Hz,		new SoundPreset (80, 	160));
		presets.Add (SoundName.WN_160_315Hz,	new SoundPreset (160, 	315));
		presets.Add (SoundName.WN_315_630Hz,	new SoundPreset (315,	630));
		presets.Add (SoundName.WN_630_1250Hz,	new SoundPreset (630, 	1250));
		presets.Add (SoundName.WN_1250_2500Hz,	new SoundPreset (1250,	2500));
		presets.Add (SoundName.WN_2500_5kHz,	new SoundPreset (2500,	5000));
		presets.Add (SoundName.WN_5k_10kHz, 	new SoundPreset (5000,	10000));
		presets.Add (SoundName.WN_10k_20kHz,	new SoundPreset (10000,	20000));
		presets.Add (SoundName.WN_20k_40kHz,	new SoundPreset (20000,	40000));
		presets.Add (SoundName.VOICE,			new SoundPreset (100,	400));

		var tmp = GetComponentsInChildren<AudioLowPassFilter> ();
		lowPassFilters = new List<AudioLowPassFilter> (tmp);

		camera = FindObjectOfType<Camera> ().transform;
		player = camera.parent;

		ui = FindObjectOfType<UiManager> ();
		if (ui != null) {
			while(!ui.isStart){
				yield return null;
			}
		}

		SetClip ();

		if (isSimulation) {
			FindObjectOfType<TestManager> ().gameObject.SetActive (false);
			Play (0);
			player.position = Vector3.back * 3;
			StartCoroutine (anim ());
			isItd = false;
		} else {
			SimulationSpeaker.gameObject.SetActive (false);
		}

	}
	
	void Update () 
	{
		if (ui != null && !ui.isStart) {
			return;
		}

		foreach (var filter in lowPassFilters) {
			filter.cutoffFrequency = CotoffFrequency;
			filter.lowpassResonanceQ = LowPassResonance;
		}


		if (!isSimulation) {
			return;
		}

		if (Input.GetKey (KeyCode.RightArrow)) {
			var dic = camera.right;
			dic.y = 0;
			player.position += dic.normalized * moveSpeed * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.LeftArrow)) {
			var dic = camera.right;
			dic.y = 0;
			player.position -= dic.normalized * moveSpeed * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.UpArrow)) {
			var dic = camera.forward;
			dic.y = 0;
			player.position += dic.normalized * moveSpeed * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.DownArrow)) {
			var dic = camera.forward;
			dic.y = 0;
			player.position -= dic.normalized * moveSpeed * Time.deltaTime;
		}


		Vector2 direct = new Vector2(camera.forward.x,camera.forward.z);
		Vector3 distance = SimulationSpeaker.position - player.position;
		Vector2 dicDirect = new Vector2 (distance.x, distance.z);

		float angle = Vector2.Angle(direct,dicDirect);

		if ((direct.normalized - dicDirect.normalized).x <= 0) {
				angle = -angle;
			}

		angle = (angle + 360) % 360;

		if (isVolumeRottof) {
			rollof = 1f / (5 * (distance.magnitude - 0.8f));
		}

		if (isHighRottof) {
			CotoffFrequency = (-distance.magnitude * 2200) + 22000;
		}
		SetIid (-angle);
	}

	private IEnumerator anim()
	{
		float timer = 0;
		while (true) {
			timer += Time.deltaTime;
			SimulationSpeaker.localScale = Vector3.one * ((Mathf.Sin (timer*10)*0.2f)+1);
			yield return null;
		}
	}

	public void SetClip()
	{
		right.Stop ();
		left.Stop ();

		AudioClip clip = Resources.Load<AudioClip> ("Sound/"+soundName.ToString());

		right.clip = clip;
		left.clip = clip;
	}

	public void Play(float angle)
	{

		if (isIid) {
			SetIid (angle);
		}

		float delay = 0;

		if (isItd) {
			StartCoroutine(ItdPlay (angle));
		} else {
			left.Play ();
			right.Play ();
		}
	}

	private IEnumerator ItdPlay(float angle)
	{
		float radian = (angle / 180f) * Mathf.PI;
		float delay = 0.09f * (Mathf.Sin(radian) + radian) / 340f;
		delay *= (-presets [soundName].aveFrequency / 20000) + 1f;
		if (delay > 0) {
			right.Play ();
			yield return new WaitForSeconds (Mathf.Abs(delay) / 1000f);
			left.Play ();
		} else {
			left.Play ();
			yield return new WaitForSeconds (Mathf.Abs(delay) / 1000f);
			right.Play ();
		}
	}

	public void Stop()
	{
		left.Stop ();
		right.Stop ();
	}

	private void SetIid(float angle)
	{
		//Debug.Log (angle%360f);
		float maxVolume = rollof / (1f + iidCoefficient);
		float radian = ((angle + 270f) / 180f) * Mathf.PI;
		float tmp = (Mathf.Cos (radian) * iidCoefficient) * ((presets [soundName].aveFrequency / 20000f)+0.5f);
		right.volume = (1f + tmp) * maxVolume;
		left.volume = (1f - tmp) * maxVolume;
	}



}