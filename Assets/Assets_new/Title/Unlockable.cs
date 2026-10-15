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

  // False for things that are earned rather than bought (the speedrun Crown)
  public bool Purchasable = true;

  private Button equipButton;
  private Button unequipButton;
  private Button buyButton;

  private int price = 0;

  private Graphic[] buyGraphics;
  private Color[] buyColours;

  private AchievementUnlocker unlockUnlocker;
  private AchievementUnlocker hatStackUnlocker;

  private static bool triedHatStack = false;

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

    if (!Purchasable || SaveManager.save.picolinium < price || SaveManager.save.unlocks.Contains(gameObject.name))
      return;

    SaveManager.save.picolinium -= price;
    SaveManager.save.unlocks.Add(gameObject.name);
    SaveManager.save.Write();

    // Put it straight on (a second hat stacks on top of the first, which is half the fun)
    EquipClicked();

    foreach (Unlockable unlockable in FindObjectsOfType<Unlockable>())
      unlockable.UpdateState();

    // Everything in the shop bought
    bool all = true;
    foreach (Unlockable unlockable in FindObjectsOfType<Unlockable>())
      if (unlockable.Purchasable && !SaveManager.save.unlocks.Contains(unlockable.gameObject.name))
        all = false;
    if (all)
    {
      if (unlockUnlocker == null)
        unlockUnlocker = AchievementUnlocker.MakeUnlocker("allunlocks");
      unlockUnlocker.UnlockAchievement();
    }
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

    if (SaveManager.save.currentHats.Count > 1)
    {
      if (hatStackUnlocker == null)
      {
        hatStackUnlocker = AchievementUnlocker.MakeUnlocker("deckedout");
      }
      if (!triedHatStack)
      {
        hatStackUnlocker.UnlockAchievement();
        triedHatStack = true;
      }
    }

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
      buyButtonEnabled = Purchasable;

    equipButton.gameObject.SetActive(equipButtonEnabled);
    unequipButton.gameObject.SetActive(unequipButtonEnabled);
    buyButton.gameObject.SetActive(buyButtonEnabled);

    // Grey out the buy button if we can't afford it
    bool affordable = SaveManager.save.picolinium >= price;
    buyButton.interactable = affordable;
    if (buyGraphics == null)
    {
      buyGraphics = buyButton.GetComponentsInChildren<Graphic>(true);
      buyColours = new Color[buyGraphics.Length];
      for (int i = 0; i < buyGraphics.Length; i++)
        buyColours[i] = buyGraphics[i].color;
    }
    for (int i = 0; i < buyGraphics.Length; i++)
      buyGraphics[i].color = affordable ? buyColours[i] : buyColours[i] * new Color(0.4f, 0.4f, 0.4f, 0.7f);
  }
}
