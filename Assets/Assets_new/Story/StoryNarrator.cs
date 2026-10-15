using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Plays a story script one beat at a time, visual novel style.
// Script format: each line is a beat that fades in, a blank line starts a new page.
// Click (or Space/Enter) finishes the current fade or moves to the next beat; Esc or
// right click skips the rest. Whoever owns the scene decides what happens when it ends.
public class StoryNarrator : MonoBehaviour
{
  public static StoryNarrator Instance { get; private set; }

  public TextAsset Script;
  public Text Body;
  public CanvasGroup BodyGroup;
  public CanvasGroup ScrimGroup;
  public Text Prompt;
  public RectTransform TopBar;
  public RectTransform BottomBar;

  [Header("Pacing")]
  public float StartDelay = 1.0f;
  public float FadeTime = 0.7f;
  public float SecondsPerWord = 0.28f;
  public float MinHold = 1.6f;
  public float PageFadeTime = 0.6f;
  public float PreviousBeatAlpha = 0.75f;

  // False for the credits: once ReadyToContinue is set it carries straight on
  public bool ClickToContinue = true;

  [Header("Prompt")]
  public string AdvanceHint = "Click to advance     Esc to skip";
  public string ContinueHint = "Click to continue";

  // Story finished, either read through or skipped
  public bool Finished { get; private set; }
  public bool Skipped { get; private set; }
  // Rough seconds of reading left, so the ship can time its warp to land with the last line
  public float EstimatedRemaining { get; private set; }
  // Set by the scene once it is ready to move on (eg. the ship has warped out)
  public bool ReadyToContinue { get; set; }
  // The player clicked "continue" (or skipped, so we don't make them click twice)
  public bool Continued { get; private set; }
  // Shown pulsing in place of the usual hints, eg. at the end of the credits roll
  public string PromptOverride { get; set; }

  private List<List<string>> pages;
  private float topHeight, bottomHeight;
  private float bars;
  private bool advance;
  private float promptAlpha;

  void Awake()
  {
    Instance = this;
    pages = Parse(Script != null ? Script.text : "");
    EstimatedRemaining = Estimate(0, 0);
    topHeight = TopBar.sizeDelta.y;
    bottomHeight = BottomBar.sizeDelta.y;
    SetBars(0);
    Body.text = "";
    BodyGroup.alpha = 0;
    ScrimGroup.alpha = 0;
    Prompt.text = AdvanceHint;
    Prompt.color = new Color(1, 1, 1, 0);
  }

  void OnDestroy()
  {
    if (Instance == this)
      Instance = null;
  }

  void Start()
  {
    StartCoroutine(Run());
  }

  void Update()
  {
    bool click = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
    bool skip = Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1);

    if (!Finished)
    {
      if (skip)
        Skip();
      else if (click)
        advance = true;
    }
    else if (ReadyToContinue && !Continued && (click || skip || Skipped || !ClickToContinue))
    {
      Continued = true;
      StartCoroutine(Fade(BodyGroup, 0, PageFadeTime));
    }

    UpdatePrompt();

    // Bars slide in with the story and out once the player moves on (or skips)
    bool show = ScrimGroup.alpha > 0 && !Continued && !Skipped;
    bars = Mathf.MoveTowards(bars, show ? 1 : 0, Time.deltaTime / 0.9f);
    SetBars(Mathf.SmoothStep(0, 1, bars));
  }

  void SetBars(float t)
  {
    TopBar.sizeDelta = new Vector2(0, topHeight * t);
    BottomBar.sizeDelta = new Vector2(0, bottomHeight * t);
  }

  void UpdatePrompt()
  {
    float target;
    if (PromptOverride != null)
      target = 0.65f + 0.35f * Mathf.Sin(Time.time * 3.0f);
    else if (Continued || Skipped || (Finished && !ClickToContinue))
      target = 0;
    else if (Finished)
      target = ReadyToContinue ? 0.65f + 0.35f * Mathf.Sin(Time.time * 3.0f) : 0;
    else
      target = Time.timeSinceLevelLoad > StartDelay + 1.5f ? 0.4f : 0;

    string text = PromptOverride ?? (Finished ? ContinueHint : AdvanceHint);
    if (Prompt.text != text)
    {
      // Fade out the old hint before swapping text
      promptAlpha = Mathf.MoveTowards(promptAlpha, 0, Time.deltaTime * 3);
      if (promptAlpha <= 0)
        Prompt.text = text;
    }
    else
    {
      promptAlpha = Mathf.MoveTowards(promptAlpha, target, Time.deltaTime * (Finished || PromptOverride != null ? 6 : 1.5f));
    }
    Prompt.color = new Color(1, 1, 1, promptAlpha);
  }

  public void Skip()
  {
    if (Finished)
      return;
    StopAllCoroutines();
    Skipped = true;
    Finished = true;
    EstimatedRemaining = 0;
    StartCoroutine(Fade(BodyGroup, 0, PageFadeTime));
  }

  IEnumerator Run()
  {
    yield return new WaitForSeconds(StartDelay);
    ScrimGroup.alpha = 1;
    yield return new WaitForSeconds(0.6f);

    for (int p = 0; p < pages.Count; p++)
    {
      var beats = pages[p];
      float[] alpha = new float[beats.Count];
      Body.text = Compose(beats, alpha);
      // Left aligned so a half-revealed paragraph reads naturally; lone lines get centre stage
      Body.alignment = beats.Count == 1 ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
      BodyGroup.alpha = 1;

      for (int b = 0; b < beats.Count; b++)
      {
        advance = false;

        // Fade the beat in; a click finishes the fade
        for (float t = 0; t < FadeTime && !advance; t += Time.deltaTime)
        {
          SetAlphas(alpha, b, t / FadeTime);
          Body.text = Compose(beats, alpha);
          yield return null;
        }
        SetAlphas(alpha, b, 1);
        Body.text = Compose(beats, alpha);
        advance = false;

        // Hold long enough to read it; a click moves on
        float hold = Hold(beats[b]);
        for (float t = 0; t < hold && !advance; t += Time.deltaTime)
        {
          EstimatedRemaining = Estimate(p, b + 1) + (hold - t);
          yield return null;
        }
        EstimatedRemaining = Estimate(p, b + 1);
      }

      // Last page stays up; others fade out before the next
      if (p < pages.Count - 1)
        yield return Fade(BodyGroup, 0, PageFadeTime);
    }

    EstimatedRemaining = 0;
    Finished = true;
  }

  // The newest beat fades in while the one before it settles back to PreviousBeatAlpha,
  // so the line being read is always the brightest
  void SetAlphas(float[] alpha, int current, float t)
  {
    t = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t));
    alpha[current] = t;
    for (int i = 0; i < current - 1; i++)
      alpha[i] = PreviousBeatAlpha;
    if (current > 0)
      alpha[current - 1] = Mathf.Lerp(1, PreviousBeatAlpha, t);
  }

  // Every beat is laid out from the start (hidden ones fully transparent) so the
  // paragraph never reflows as lines appear
  static string Compose(List<string> beats, float[] alpha)
  {
    var sb = new StringBuilder();
    for (int i = 0; i < beats.Count; i++)
    {
      if (i > 0)
        sb.Append(' ');
      int a = Mathf.RoundToInt(Mathf.Clamp01(alpha[i]) * 255);
      sb.Append("<color=#FFFFFF").Append(a.ToString("X2")).Append('>').Append(beats[i]).Append("</color>");
    }
    return sb.ToString();
  }

  float Hold(string beat)
  {
    int words = beat.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
    return Mathf.Max(MinHold, words * SecondsPerWord);
  }

  float Estimate(int page, int beat)
  {
    float total = 0;
    for (int p = page; p < pages.Count; p++)
    {
      for (int b = p == page ? beat : 0; b < pages[p].Count; b++)
        total += FadeTime + Hold(pages[p][b]);
      if (p < pages.Count - 1)
        total += PageFadeTime;
    }
    return total;
  }

  static List<List<string>> Parse(string text)
  {
    var result = new List<List<string>>();
    var page = new List<string>();
    foreach (var raw in text.Replace("\r", "").Split('\n'))
    {
      var line = raw.Trim();
      if (line.Length == 0)
      {
        if (page.Count > 0)
          result.Add(page);
        page = new List<string>();
      }
      else
      {
        page.Add(line);
      }
    }
    if (page.Count > 0)
      result.Add(page);
    return result;
  }

  static IEnumerator Fade(CanvasGroup group, float to, float time)
  {
    float from = group.alpha;
    for (float t = 0; t < time; t += Time.deltaTime)
    {
      group.alpha = Mathf.Lerp(from, to, t / time);
      yield return null;
    }
    group.alpha = to;
  }
}
