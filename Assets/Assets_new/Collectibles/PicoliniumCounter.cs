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
    if (_count < actualCount)
    {
      var newCount = _count + CountRate * Time.deltaTime;
      newCount = Mathf.Min(newCount, actualCount);
      int added = (int)newCount - (int)_count;
      _count = newCount;
    }

    _text.text = $"{(int)_count}";
  }
}
