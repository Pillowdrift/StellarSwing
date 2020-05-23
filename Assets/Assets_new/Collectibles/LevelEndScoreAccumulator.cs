using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndScoreAccumulator : MonoBehaviour
{
  public AudioClip Clip;
  public int PlayerCount = 10;
  public float Step = 0.5f;
  public bool PlayOnSpace = false;

  private bool _playing = false;

  private List<AudioSource> _sources = new List<AudioSource>();

  private void Start()
  {
    for (int i = 0; i < PlayerCount; ++i)
    {
      var obj = new GameObject();
      obj.transform.parent = transform;
      var source = obj.AddComponent<AudioSource>();
      source.playOnAwake = false;
      source.clip = Clip;
      source.pitch += i * Step;
      _sources.Add(source);
    }
  }

  private void Update()
  {
    if (PlayOnSpace && Input.GetKeyDown(KeyCode.Space))
    {
      Play();
    }
  }

  private void Play()
  {
    if (_playing)
      return;

    _playing = true;
    StartCoroutine(PlayCoroutine());
  }

  private IEnumerator PlayCoroutine()
  {
    for (int i = 0; i < PlayerCount; ++i)
    {
      _sources[i].Play();
      yield return new WaitForSeconds(0.1f);
    }
    _playing = false;
  }
}
