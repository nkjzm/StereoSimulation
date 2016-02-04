using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

	[SerializeField]
	private Transform speakers;

	void Start()
	{
		transform.position = speakers.position + Vector3.up * 2.5f;
	}

	void Update () 
	{
		speakers.rotation = Quaternion.Euler (0,transform.eulerAngles.y, 0);
		speakers.position = new Vector3 (
			transform.position.x,
			0,
			transform.position.z
		);
	}
}
