using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectiblePickles : MonoBehaviour
{
  public int Count = 10;
  public GameObject ParticleSystem;

  private ScoreAccumulator _scoreAccumulator;

  private void Start()
  {
    _scoreAccumulator = FindObjectOfType<ScoreAccumulator>();
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.gameObject.name == "Player")
    {
      Debug.Log("Player touched crystal");
      var particleSystem = GameObject.Instantiate(ParticleSystem);
      particleSystem.transform.position = transform.position;
      particleSystem.GetComponent<ParticleSystem>().maxParticles = Count;
      SaveManager.save?.IncrementPicolinium(Count);
      _scoreAccumulator.Play();
      gameObject.SetActive(false);
    }
  }
}
