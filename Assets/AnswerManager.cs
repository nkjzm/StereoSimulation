using UnityEngine;
using System.Collections;

public class AnswerManager : MonoBehaviour 
{
	[SerializeField]
	private Transform speakers;

	[SerializeField]
	private float _radius;

	public int posNum;

	private float angleDiff;
	private int childCount;

	private Transform pointer;

	void Start () 
	{
		pointer = transform.GetChild (0);

		UpdatePosition ();

		childCount = speakers.childCount;

		//オブジェクト間の角度差
		angleDiff = 360f / (float)childCount;

	}	

	void UpdatePosition () 
	{
		transform.rotation = Quaternion.Euler(0,0,0);

		Vector3 childPostion = speakers.position;

		float angle = (90 - angleDiff * posNum) * Mathf.Deg2Rad;
		childPostion.x += _radius * Mathf.Cos (angle);
		childPostion.y += 1.8f;
		childPostion.z += _radius * Mathf.Sin (angle);

		pointer.position = childPostion;
		pointer.LookAt (transform.position);

		transform.rotation = speakers.rotation;
	}

	void Update () 
	{
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			posNum = (++posNum % childCount);
		}
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			posNum = (--posNum % childCount);
		}
		UpdatePosition ();
	}
}
