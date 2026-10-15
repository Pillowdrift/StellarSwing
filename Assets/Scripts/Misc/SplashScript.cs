using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Shows the splash scene for a moment, fades to black, then loads the title.
// The fade is a runtime uGUI overlay so this scene needs no UI objects of its own.
public class SplashScript : MonoBehaviour
{
	public float fadeStartsIn = 1.0f;
	public float fadeDuration = 1.0f;
	public string nextScene = "Title_new";

	private bool startedFade = false;
	private Image fadeImage;

	void Start()
	{
		CreateFadeOverlay();
		StartCoroutine(FadeStart());
	}

	void Update()
	{
		// Skip straight to the fade if the user clicks
		if (!startedFade && Input.GetMouseButtonDown(0))
			StartCoroutine(FadeOutAndLoad());
	}

	IEnumerator FadeStart()
	{
		yield return new WaitForSeconds(fadeStartsIn);
		if (!startedFade)
			StartCoroutine(FadeOutAndLoad());
	}

	IEnumerator FadeOutAndLoad()
	{
		startedFade = true;

		float t = 0.0f;
		while (t < fadeDuration)
		{
			t += Time.deltaTime;
			fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(t / fadeDuration));
			yield return null;
		}

		yield return new WaitForSeconds(0.2f);
		SceneManager.LoadScene(nextScene);
	}

	private void CreateFadeOverlay()
	{
		var canvasObject = new GameObject("SplashFade");
		canvasObject.transform.SetParent(transform, false);

		var canvas = canvasObject.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 1000;

		var imageObject = new GameObject("Black");
		imageObject.transform.SetParent(canvasObject.transform, false);

		fadeImage = imageObject.AddComponent<Image>();
		fadeImage.color = new Color(0, 0, 0, 0);
		fadeImage.raycastTarget = false;

		var rect = fadeImage.rectTransform;
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
	}
}
