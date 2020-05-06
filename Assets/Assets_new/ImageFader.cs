using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageFader : MonoBehaviour
{
  private Image image;

  void Awake()
  {
    image = GetComponent<Image>();
    image.enabled = true;
    StartCoroutine(FadeIn());
  }

  IEnumerator FadeIn()
  {
    yield return new WaitForSeconds(0.5f);
    image.CrossFadeAlpha(0.1f, 0.4f, false);
  }
}
