using System.Collections;
using UnityEngine;

// Scrolls the credits up once the epilogue story has finished. Hold click (or Space)
// to fast-forward, Esc to leave; at the end it waits for a click.
public class CreditsRoll : MonoBehaviour
{
  public float Speed = 60.0f;
  public float FastMultiplier = 5.0f;
  public float StartDelay = 1.0f;

  private RectTransform rect;

  void Start()
  {
    rect = (RectTransform)transform;
    StartCoroutine(Roll());
  }

  IEnumerator Roll()
  {
    var text = GetComponent<UnityEngine.UI.Text>();
    var parent = (RectTransform)rect.parent;
    rect.sizeDelta = new Vector2(rect.sizeDelta.x, text.preferredHeight);
    rect.anchoredPosition = Vector2.zero;

    // Nothing to wait for in the credits, so the epilogue carries straight on when it's done
    var story = StoryNarrator.Instance;
    if (story != null)
      story.ReadyToContinue = true;
    while (story != null && !story.Continued)
      yield return null;
    yield return new WaitForSeconds(StartDelay);

    // Scroll until the last line sits in the middle of the screen
    float end = rect.sizeDelta.y + parent.rect.height * 0.5f - text.fontSize;
    while (rect.anchoredPosition.y < end)
    {
      if (Input.GetKeyDown(KeyCode.Escape))
      {
        Exit();
        yield break;
      }
      bool fast = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
      float y = rect.anchoredPosition.y + Speed * (fast ? FastMultiplier : 1) * Time.deltaTime;
      rect.anchoredPosition = new Vector2(0, Mathf.Min(y, end));
      yield return null;
    }

    // Let go of any fast-forward click before asking for a new one
    while (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
      yield return null;
    yield return new WaitForSeconds(0.5f);
    if (story != null)
      story.PromptOverride = story.ContinueHint;

    while (!(Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape)))
      yield return null;
    Exit();
  }

  void Exit()
  {
    var gui = FindAnyObjectByType<CreditsGUI>();
    if (gui != null)
      gui.Exit();
    else
      Loading.Load("Title_new");
  }
}
