using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.UI;

public class DailyGift : MonoBehaviour
{
    [SerializeField] private Button[] chestButtons; // Масив кнопок-сундуків
    [SerializeField] private TMP_Text statusText; // Текст статусу ("Choose Your Gift" або таймер)
    [SerializeField] private GameObject rewardPopup; // Popup нагороди
    [SerializeField] private GameObject screen;
    [SerializeField] private TMP_Text rewardText; // Текст нагороди
    [SerializeField] private int minCoins = 10; // Мінімальна нагорода
    [SerializeField] private int maxCoins = 100; // Максимальна нагорода

    private const string LastClaimKey = "LastGiftClaim";
    private const float CooldownHours = 24f;
    private DateTime lastClaimTime;
    private bool canClaim;

    public void Show()
    {
        screen.SetActive(true);
        LoadClaimTime();
        UpdateUI();
    }

    private void LoadClaimTime()
    {
        string lastClaimString = PlayerPrefs.GetString(LastClaimKey, "");
        if (!string.IsNullOrEmpty(lastClaimString))
        {
            lastClaimTime = DateTime.Parse(lastClaimString);
        }
        else
        {
            lastClaimTime = DateTime.MinValue;
        }

        CheckClaimAvailability();
        StartCoroutine(UpdateTimer());
    }

    private void CheckClaimAvailability()
    {
        if ((DateTime.Now - lastClaimTime).TotalHours >= CooldownHours)
        {
            canClaim = true;
            statusText.text = "Choose Your Gift";
            EnableChests(true);
        }
        else
        {
            canClaim = false;
            EnableChests(false);
        }
    }

    private IEnumerator UpdateTimer()
    {
        while (!canClaim)
        {
            TimeSpan timeLeft = TimeSpan.FromHours(CooldownHours) - (DateTime.Now - lastClaimTime);
            if (timeLeft.TotalSeconds > 0)
            {
                statusText.text = $"Next gift in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            }
            else
            {
                canClaim = true;
                statusText.text = "Choose Your Gift";
                EnableChests(true);
            }
            yield return new WaitForSeconds(1);
        }
    }

    private void EnableChests(bool enable)
    {
        foreach (Button chest in chestButtons)
        {
            chest.interactable = enable;
        }
    }

    public void ClaimReward(Button selectedChest)
    {
        if (!canClaim) return;

        int rewardAmount = UnityEngine.Random.Range(minCoins, maxCoins);
        GiveCoins(rewardAmount);

        rewardText.text = $"+{rewardAmount}";
        rewardPopup.SetActive(true);

        lastClaimTime = DateTime.Now;
        PlayerPrefs.SetString(LastClaimKey, lastClaimTime.ToString());
        PlayerPrefs.Save();

        canClaim = false;
        EnableChests(false);
        StartCoroutine(UpdateTimer());
    }

    private void GiveCoins(int amount)
    {
        int coins = PlayerPrefs.GetInt("PlayerCoins", 0);
        coins += amount;
        PlayerPrefs.SetInt("PlayerCoins", coins);
        PlayerPrefs.Save();
        Debug.Log($"Received {amount} coins. Total: {coins}");
    }
    public void CloseScreen()
    {
        screen.SetActive(false);
        rewardPopup.SetActive(false);
    }

    public void ClosePopup()
    {
        rewardPopup.SetActive(false);
    }

    private void UpdateUI()
    {
        CheckClaimAvailability();
        StartCoroutine(UpdateTimer());
    }
}
