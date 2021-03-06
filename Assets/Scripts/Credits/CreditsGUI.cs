using UnityEngine;
using System.Collections;

public class CreditsGUI : MonoBehaviour
{
    public GameObject textObj;

	private Vector3 initialPosition;
	
	private const string credits = 	  "Credits\n\n"
									+ "� 2020 Pillowdrift Games\n"
									+ "\n"
									+ "Remaster by Caitlin Wilks & Luci Grey\n"
									+ "\n"
									+ "Original pillowdrift crew:\n"
									+ "Caitlin Wilks\n"
									+ "Jitesh Rawal\n"
									+ "Richard Webster-Noble\n"
									+ "Grant Livingston\n"
									+ "\n"
									+ "Original artwork by:\n"
									+ "Tom Duke\n"
									+ "Joe Brammer\n"
									+ "\n"
									+ "Original soundtrack by:\n"
									+ "Campbell Logan\n"
									+ "\n"
									+ "Sound effects\n"
									+ "Freesound.org\n"
									+ "Black Boe\n"
									+ "DJ Chronos\n"
									+ "sandyrb\n"
                  + "\n"
                  + "Shout out to luci, luki, and the rest of #rena\n"
                  + "\n"
                  + "pillowdrift.com"
                  + "\n"
                  + "Thank you for playing!";

  // rate of scrolling
  private const float rate = 0.1f;

	private bool canSkip = false;
	
	// Use this for initialization
	void Start()
	{
		WaitAndAllowSkip();
		//initialPosition = textObj.transform.position;
		//textObj.GetComponent<GUIText>().text = credits;
	}

	void WaitAndAllowSkip() => StartCoroutine(WaitAndAllowSkip_());

	IEnumerator WaitAndAllowSkip_()
	{
		yield return new WaitForSeconds(5.0f);
		canSkip = true;
	}
	
	// Update is called once per frame
	void Update()
	{
		//textObj.transform.Translate(0, rate * Time.deltaTime, 0);
		
		//if (textObj.transform.position.y > 5.0f)
			//textObj.transform.position = initialPosition;
		
		// tap to get to menu
		if (canSkip && Input.GetMouseButton(0))
		{
			if (SaveManager.save != null &&
				SaveManager.save.worldUnlocked == 5)
			{
				// World5 so it'll show THE NEXT world (6)
				SaveManager.save.worldUnlocked = 6;
				SaveManager.save.levelUnlocked = 1;

				LevelSelectGUI.worldToShow = "World5";
				LevelSelectGUI.levelToShow = 0;
				LevelSelectGUI.worldTransition = true;
				Application.LoadLevel("Title_new");
			}
			else
			{
				Loading.Load("Title_new");
			}
		}
	}
}
