using UnityEngine;
using System.Collections;

public class CreditsGUI : MonoBehaviour
{
    public GameObject textObj;

	private Vector3 initialPosition;
	
	private const string credits = 	  "Credits\n\n"
									+ "© 2020 Pillowdrift Games\n"
									+ "\n"
									+ "Remaster by cat (Caitlin Wilks)\n"
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
	
	// Use this for initialization
	void Start()
	{
		initialPosition = textObj.transform.position;
		textObj.GetComponent<GUIText>().text = credits;
	}
	
	// Update is called once per frame
	void Update()
	{
		textObj.transform.Translate(0, rate * Time.deltaTime, 0);
		
		if (textObj.transform.position.y > 5.0f)
			textObj.transform.position = initialPosition;
		
		// tap to get to menu
		if (Input.GetMouseButton(0))
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
				Application.LoadLevel("Title");
			}
			else
			{
				Loading.Load("Title");
			}
		}
	}
}
