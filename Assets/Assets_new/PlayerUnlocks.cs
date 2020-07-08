using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerUnlocks : MonoBehaviour
{
  public enum Powerup
  {
    None,
    Jump,
    Jetpack,
    Boost,
    BulletTime
  }

  public float JumpForce = 10.0f;
  public float JetForce = 5.0f;
  public float BoostForce = 0.01f;
  public float BoostMaxSpeed = 100.0f;
  public float BulletTime = 0.5f;

  private Powerup currentPowerup = Powerup.None;

  private Rigidbody _rigidbody;

  private bool available = true;

  private float hatHeight = 0.0f;

  private void Start()
  {
    _rigidbody = GetComponent<Rigidbody>();
    if (SaveManager.save != null)
    {
      foreach (var hat in SaveManager.save.currentHats)
        EnableHat(hat);
      EnablePowerup(SaveManager.save.currentUpgrade);
    }
  }

  private void Update()
  {
    if (!Input.GetMouseButton((int)MouseButton.RightMouse))
      available = true;

    if (currentPowerup == Powerup.Jump)
    {
      if (available && Input.GetMouseButton((int)MouseButton.RightMouse))
      {
        available = false;
        _rigidbody.AddForce(Vector3.up * JumpForce);
      }
    }
    else if (currentPowerup == Powerup.Jetpack)
    {
      if (Input.GetMouseButton((int)MouseButton.RightMouse))
      {
        _rigidbody.AddForce(Vector3.up * JetForce);
        if (_rigidbody.velocity.y > 0.0f)
        {
          var vel = _rigidbody.velocity;
          vel.y = 0.0f;
          _rigidbody.velocity = vel;
        }
      }
    }
    else if (currentPowerup == Powerup.Boost)
    {
      var direction = _rigidbody.velocity.normalized;
      if (Input.GetMouseButton((int)MouseButton.RightMouse))
      {
        _rigidbody.AddForce(direction * BoostForce);
        var speed = _rigidbody.velocity.magnitude;
        if (speed > BoostMaxSpeed)
          _rigidbody.velocity = direction * BoostMaxSpeed;
      }
    }
    else if (currentPowerup == Powerup.BulletTime)
    {
      if (Input.GetMouseButton((int)MouseButton.RightMouse))
      {
        Time.timeScale = BulletTime;
      }
      else
      {
        Time.timeScale = 1.0f;
      }
    }
  }

  private void EnableHat(string hatToEnable)
  {
    var hats = transform.Find("Hats");

    for (int i = 0; i < hats.childCount; ++i)
    {
      var hat = hats.GetChild(i).gameObject;
      if (hatToEnable == hat.name)
      {
        hat.SetActive(true);

        // Stack em up except the mustache
        if (hat.name != "Mustache")
        {
          float height = hat.GetComponent<MeshFilter>().mesh.bounds.size.y * hat.transform.lossyScale.y;
          var pos = hat.transform.position;
          pos.y += hatHeight;
          hat.transform.position = pos;
          hatHeight += height * 0.5f * 0.7f;
        }
      }
    }
  }

  private void EnablePowerup(string powerupToEnable)
  {
    switch (powerupToEnable)
    {
      case "Jump":
        currentPowerup = Powerup.Jump;
        break;
      case "Jetpack":
        currentPowerup = Powerup.Jetpack;
        break;
      case "Boost":
        currentPowerup = Powerup.Boost;
        break;
      case "BulletTime":
        currentPowerup = Powerup.BulletTime;
        break;
      default:
        currentPowerup = Powerup.None;
        break;
    }
  }
}
