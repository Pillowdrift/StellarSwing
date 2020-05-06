using UnityEngine;
using System.Collections;

public class ShowHighscoresButton : MonoBehaviour
{
	void Update()
	{
		if (LevelSelectGUI.menuState == LevelSelectGUI.MenuState.LEVEL_SELECT && LevelSelectGUI.currentLevel != null && LevelSelectGUI.currentLevel.highscores)
		{
			GetComponent<Renderer>().enabled = true;
			GetComponent<Collider>().enabled = true;
		}
		else
		{
			GetComponent<Renderer>().enabled = false;
			GetComponent<Collider>().enabled = false;
		}
		
		GameObject.Find("HighscoresArea").GetComponent<GUITexture>().enabled = HighScoresGUI.enable;			
	}
	
	void OnMouseDown()
	{
		if (!HighScoresGUI.enable)
		{
			HighScoresGUI.enable = true;
		}
		else
		{
			HighScoresGUI.enable = false;
		}
	}
}
