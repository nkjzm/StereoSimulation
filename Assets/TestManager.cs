using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestManager : MonoBehaviour 
{
	[SerializeField]
	private SoundManager soundManager;

	[SerializeField]
	private Transform speakers;
	private int speakerNum;

	[SerializeField,Range(0,180)]
	private int ViewAngle;
	[SerializeField,Range(0,90)]
	private int ViewResolution;

	[SerializeField]
	private int TestCount;

	private int currentTestCount = 0;

	[SerializeField]
	private int PrepareCount;

	private int posNum;
	private int angleDiff;

	private bool isTest = false;

	private AudioSource audioSource;

	public struct AnswerLog
	{
		public int correctAnswer;
		public int userAnswer;
		public AnswerLog(int c,int u){
			correctAnswer = c;
			userAnswer = u;
		}
	}

	private List<AnswerLog> answerLogs;

	public void InspectorAdapt(int a,int r,int p,int t)
	{
		ViewAngle = a;
		ViewResolution = r;
		PrepareCount = p;
		TestCount = t;
	}

	private UiManager ui;

	void Start () 
	{
		ui = FindObjectOfType<UiManager> ();

		speakerNum = speakers.childCount;
		angleDiff = 360 / speakerNum;

		answerLogs = new List<AnswerLog> ();

		audioSource = GetComponent<AudioSource> ();
	}

	public void Execute()
	{
		List<int> list = new List<int> ();
		int tmp = 0;
		list.Add (tmp);
		while (true) {
			++tmp;
			list.Add (tmp);
			list.Add (-tmp);
			if (tmp*ViewResolution > ViewAngle) {
				break;
			}
		}

		int index = Random.Range (0, list.Count);
		posNum = ((list[index]+speakerNum) % speakerNum);

		float angle = (angleDiff * posNum);
		soundManager.Play (angle);
	}

	public void Answer()
	{
		int answerPosNum = FindObjectOfType<AnswerManager> ().posNum;
		answerPosNum = (answerPosNum + speakerNum) % speakerNum;
		answerLogs.Add (new AnswerLog(posNum, answerPosNum));
		if (++currentTestCount >= TestCount) {
			soundManager.Stop ();
			GetComponent<AudioSource> ().Play ();
			WriteCsv ();
			isTest = false;
			return;
		}
		Execute ();
	}

	void Update () {

		if (ui != null && !ui.isStart) {
			return;
		}


		if (Input.GetKeyDown (KeyCode.Space)) {
			if (isTest) {
				Answer ();
			} else {
				StartCoroutine (Init ());
			}
		}
	}

	private IEnumerator Init()
	{
		yield return StartCoroutine(Prepare());
		audioSource.clip = Resources.Load<AudioClip> ("Sound/begin");
		audioSource.Play ();
		yield return new WaitForSeconds(1);
		Execute ();
		isTest = true;
	}

	private IEnumerator Prepare()
	{
		//子を取得
		List<Transform> childList = new List<Transform> ();
		foreach(Transform child in speakers) {
			childList.Add (child);
		}
		int count = 0;
		while (count < PrepareCount) {
			++count;
			Execute ();
			Debug.Log (posNum);
			float timer = 0;
			while (timer <= 1) {
				timer += Time.deltaTime;
				childList [posNum].localScale = Vector3.one * ((Mathf.Sin (timer*10)*0.2f)+1);
				yield return null;
			}
			childList [posNum].localScale = Vector3.one;
		}
		soundManager.Stop ();
	}


	private void WriteCsv()
	{
		try
		{
			// appendをtrueにすると，既存のファイルに追記
			//         falseにすると，ファイルを新規作成する
			var append = false;
			// 出力用のファイルを開く
			string date = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
			using (var sw = new System.IO.StreamWriter(@"log_"+date + @".csv", append))
			{
				sw.WriteLine("{0}, {1}, {2},", "CorrectAnswer","UserAnswer","Error");
				foreach(AnswerLog log in answerLogs)
				{
					int correctAnswer = FormatAngle(log.correctAnswer);
					int userAnswer = FormatAngle(log.userAnswer);
					int error = Mathf.Abs(correctAnswer-userAnswer);
					sw.WriteLine("{0}, {1}, {2},",correctAnswer,userAnswer,error);
				}
			}
		}
		catch (System.Exception e)
		{
			// ファイルを開くのに失敗したときエラーメッセージを表示
			System.Console.WriteLine(e.Message);
		}
	}

	private int FormatAngle(int converPosNum)
	{
		int angle = converPosNum * angleDiff;
		if(angle > 180){
			angle -= 360;
		}
		return angle;
	}

}
