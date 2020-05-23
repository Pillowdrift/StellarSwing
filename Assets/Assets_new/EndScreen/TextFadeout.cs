using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFadeout : MonoBehaviour
{
  public bool Go = false;
  public float FadeSpeed = 1.0f;
  public float MoveSpeed = 100.0f;
  public string Text;

  public RectTransform _rectTransform;
  public Text _text;

  private void Update()
  {
    if (!Go)
      return;

    if (_text.text != Text)
      _text.text = Text;

    var textCol = _text.color;
    textCol.a -= FadeSpeed * Time.deltaTime;
    _text.color = textCol;

    var pos = _rectTransform.anchoredPosition;
    pos.y += MoveSpeed * Time.deltaTime;
    _rectTransform.anchoredPosition = pos;
  }
}
