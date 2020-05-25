using UnityEngine;
using UnityEngine.UI;

public class PicoliniumCounter : MonoBehaviour
{
  public float CountRate = 0.1f;

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
    if (Mathf.Abs(_count - actualCount) < 0.1f)
    {
      _count = actualCount;
    }
    else
    {
      float sign = Mathf.Sign(actualCount - _count);
      var newCount = _count + sign * CountRate * Time.deltaTime;
      newCount = sign >= 0.0f ? Mathf.Min(newCount, actualCount) : Mathf.Max(newCount, actualCount);
      _count = newCount;
    }

    _text.text = $"{(int)_count}";
  }
}
