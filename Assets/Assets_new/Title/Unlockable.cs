using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unlockable : MonoBehaviour
{
  public enum Type
  {
    Upgrade,
    Hat
  }

  public Type UnlockType = Type.Upgrade;

  private Button equipButton;
  private Button unequipButton;
  private Button buyButton;

  private int price = 0;

  public void Start()
  {
    equipButton = transform.Find("EquipButton")?.GetComponent<Button>();
    unequipButton = transform.Find("UnequipButton")?.GetComponent<Button>();
    buyButton = transform.Find("BuyButton")?.GetComponent<Button>();
    price = int.Parse(buyButton?.gameObject?.GetComponentInChildren<Text>()?.text ?? "0");
    UpdateState();
  }

  public void BuyClicked()
  {
    if (SaveManager.save == null)
      return;

    if (SaveManager.save.picolinium < price || SaveManager.save.unlocks.Contains(gameObject.name))
      return;

    SaveManager.save.picolinium -= price;
    SaveManager.save.unlocks.Add(gameObject.name);
    SaveManager.save.Write();

    foreach (Unlockable unlockable in FindObjectsOfType<Unlockable>())
      unlockable.UpdateState();
  }

  public void EquipClicked()
  {
    if (SaveManager.save == null)
      return;

    if (UnlockType == Type.Upgrade)
      SaveManager.save.currentUpgrade = gameObject.name;
    else if (UnlockType == Type.Hat)
      SaveManager.save.currentHats.Add(gameObject.name);

    SaveManager.save.Write();

    foreach (Unlockable unlockable in FindObjectsOfType<Unlockable>())
      unlockable.UpdateState();
  }

  public void UnequipClicked()
  {
    if (SaveManager.save == null)
      return;

    if (UnlockType == Type.Upgrade && SaveManager.save.currentUpgrade == gameObject.name)
      SaveManager.save.currentUpgrade = "";
    else if (UnlockType == Type.Hat && SaveManager.save.currentHats.Contains(gameObject.name))
      SaveManager.save.currentHats.Remove(gameObject.name);

    SaveManager.save.Write();

    foreach (Unlockable unlockable in FindObjectsOfType<Unlockable>())
      unlockable.UpdateState();
  }

  public void UpdateState()
  {
    if (SaveManager.save == null)
      return;

    bool equipButtonEnabled = false;
    bool unequipButtonEnabled = false;
    bool buyButtonEnabled = false;

    // Get current unlock
    string currentUpgrade = SaveManager.save.currentUpgrade;

    // Enable buttons depending on whether we have it unlocked etc
    if (currentUpgrade == gameObject.name || SaveManager.save.currentHats.Contains(gameObject.name))
      unequipButtonEnabled = true;
    else if (SaveManager.save.unlocks.Contains(gameObject.name))
      equipButtonEnabled = true;
    else
      buyButtonEnabled = true;

    equipButton.gameObject.SetActive(equipButtonEnabled);
    unequipButton.gameObject.SetActive(unequipButtonEnabled);
    buyButton.gameObject.SetActive(buyButtonEnabled);

    // Disable buy button if we don't have enough picolinium
    if (SaveManager.save.picolinium < price)
    {
      buyButton.interactable = false;
    }
  }
}
