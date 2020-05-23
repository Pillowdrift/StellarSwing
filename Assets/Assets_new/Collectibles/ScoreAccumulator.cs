using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreAccumulator : MonoBehaviour
{
  public AudioClip Clip;
  public int PlayerCount = 10;
  public float Step = 0.5f;
  public bool PlayOnSpace = false;
  public float ResetTime = 3.0f;

  private bool _playing = false;

  private int _current = 0;

  private List<AudioSource> _sources = new List<AudioSource>();

  private IEnumerator _coroutine;

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

    StartCoroutine(RestartCurrentAfterAsync(0.0f));
  }

  private void Update()
  {
    if (PlayOnSpace && Input.GetKeyDown(KeyCode.Space))
    {
      Play();
    }
  }

  public void Play()
  {
    var player = _sources[_current];
    player.Play();
    _current = (_current + 1) % PlayerCount;
    ResetCurrentAfter(ResetTime);
  }

  private void ResetCurrentAfter(float seconds)
  {
    if (_coroutine != null)
      StopCoroutine(_coroutine);
    _coroutine = RestartCurrentAfterAsync(seconds);
    StartCoroutine(_coroutine);
  }

  private IEnumerator RestartCurrentAfterAsync(float seconds)
  {
    yield return new WaitForSeconds(seconds);
    _current = Random.Range(0, 2);
  }
}
