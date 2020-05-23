using UnityEngine;
using UnityEngine.UI;

public class PicoliniumCounter : MonoBehaviour
{
  public float CountRate = 0.1f;

  private float _count = 0;

  private Text _text;

  void Start()
  {
    _text = GetComponent<Text>();
    _count = (float)Settings.Current.PlayerStats.Picolinium;
  }

  void Update()
  {
    float actualCount = (float)Settings.Current.PlayerStats.Picolinium;
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
