using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.F1))
		{
			PlayerPrefs.SetString("FirstTurn", "Player");
			Application.LoadLevel (1);
		}
		if(Input.GetKeyDown(KeyCode.F2))
		{
				PlayerPrefs.SetString("FirstTurn", "AI");
				Application.LoadLevel (1);
		}
	
	}

	public void PlayerFirst()
	{
		PlayerPrefs.SetString("FirstTurn", "Player");
		Application.LoadLevel (1);
	}

	public void AIFirst()
	{
		PlayerPrefs.SetString("FirstTurn", "AI");
		Application.LoadLevel (1);
	}
}
