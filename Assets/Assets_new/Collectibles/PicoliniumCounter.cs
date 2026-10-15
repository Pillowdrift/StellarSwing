using UnityEngine;
using UnityEngine.UI;

public class PicoliniumCounter : MonoBehaviour
{
  // Minimum counting speed (per second), so small changes still tick visibly
  public float CountRate = 0.1f;
  // How quickly the gap closes: bigger changes count faster, settling in about a second
  public float Smoothing = 4.0f;

  // if set it'll count to this target
  public int Target = -1;

  private float _count = 0;

  private Text _text;

  void Start()
  {
    _text = GetComponent<Text>();
    _count = (float)SaveManager.save?.picolinium;
  }

  void Update()
  {
    float actualCount = Target >= 0 ? Target : (float)(SaveManager.save?.picolinium ?? 0);
    float diff = actualCount - _count;
    float step = diff * (1.0f - Mathf.Exp(-Smoothing * Time.unscaledDeltaTime));
    float minStep = CountRate * Time.unscaledDeltaTime;
    if (Mathf.Abs(step) < minStep)
      step = Mathf.Sign(diff) * minStep;

    if (Mathf.Abs(step) >= Mathf.Abs(diff) || Mathf.Abs(diff) < 0.5f)
      _count = actualCount;
    else
      _count += step;

    _text.text = $"{(int)_count}";
  }
}
