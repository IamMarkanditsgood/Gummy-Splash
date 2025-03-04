using System;
using System.Collections;
using System.Collections.Generic;
using Mkey;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyBonus : MonoBehaviour
{
    [Serializable]
    public class DailyReward
    {
        public int day;
        public int coins;
        public bool isCollected;
    }
    [SerializeField] private GameObject _bg;
    [SerializeField] private GameObject _view;
    [SerializeField] private GameObject _viewFinish;
    [SerializeField] private DailyReward[] dailyRewards; // Масив винагород
    [SerializeField] private Button claimButton; // Кнопка отримання бонусу
    [SerializeField] private Button[] closeButton;
    [SerializeField] private TMP_Text[] rewardText; // Відображення нагороди
    [SerializeField] private GameObject[] receivedText; // Панелі з днями
    [SerializeField] private GameObject[] borders;
    [SerializeField] private GameObject[] _Prices;

    private int currentDay;
    private const string LastClaimKey = "LastClaimDate";
    private const string CurrentDayKey = "CurrentBonusDay";

    private void Start()
    {
        for(int i = 0; i< closeButton.Length; i++)
        {
            int index = i;
            closeButton[index].onClick.AddListener(ClosePopup);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < closeButton.Length; i++)
        {
            int index = i;
            closeButton[index].onClick.RemoveListener(ClosePopup);
        }
    }

    public void ShowPopup()
    {
        _bg.SetActive(true);
        foreach (var border in borders)
        {
            border.SetActive(false);
        }
        foreach (var text in receivedText)
        {
            text.SetActive(false);
        }
        foreach (var text in rewardText)
        {
            text.gameObject.SetActive(false);
        }
        foreach (var text in _Prices)
        {
            text.gameObject.SetActive(true);
        }
        _view.SetActive(true);
        LoadProgress();
        UpdateUI();
    }

    public void ClosePopup()
    {
        _bg.SetActive(false);
        _view.SetActive(false);
        foreach(var border in borders)
        {
            border.SetActive(false);
        }
        foreach (var text in receivedText)
        {
            text.SetActive(false);
        }
        foreach (var price in _Prices)
        {
            price.SetActive(true);
        }
    }

    private void LoadProgress()
    {
        currentDay = PlayerPrefs.GetInt(CurrentDayKey, 0);
        string lastClaimDate = PlayerPrefs.GetString(LastClaimKey, "");

        if (!string.IsNullOrEmpty(lastClaimDate))
        {
            DateTime lastClaim = DateTime.Parse(lastClaimDate);
            if (DateTime.Now.Date > lastClaim.Date)
            {
                claimButton.interactable = true;
            }
            else
            {
                claimButton.interactable = false;
            }
        }        
    }

    private void UpdateUI()
    {
        foreach (var price in _Prices)
        {
            price.SetActive(true);
        }
        foreach (var border in borders)
        {
            border.SetActive(false);
        }
        foreach (var text in receivedText)
        {
            text.SetActive(false);
        }
        foreach (var text in rewardText)
        {
            text.gameObject.SetActive(false);
        }

        for (int i = 0; i < rewardText.Length; i++)
        {
            rewardText[i].gameObject.SetActive(true);
            rewardText[i].text = dailyRewards[i].coins.ToString();
        }

        for (int i = 0; i < currentDay; i++)
        {
            rewardText[i].gameObject.SetActive(false);
            _Prices[i].gameObject.SetActive(false);
            receivedText[i].SetActive(true);
        }

        borders[currentDay].SetActive(true);
        if (currentDay > dailyRewards.Length)
        {
            claimButton.interactable = false;
            _view.SetActive(false);
            _viewFinish.SetActive(true);
        }
    }

    public void ClaimReward()
    {
        if (currentDay >= dailyRewards.Length) return;

        int rewardAmount = dailyRewards[currentDay].coins;
        GiveCoins(rewardAmount);

        rewardText[currentDay].gameObject.SetActive(false);
        _Prices[currentDay].gameObject.SetActive(false);

        currentDay++;
        PlayerPrefs.SetInt(CurrentDayKey, currentDay);
        PlayerPrefs.SetString(LastClaimKey, DateTime.Now.ToString());
        PlayerPrefs.Save();

        claimButton.interactable = false;

        UpdateUI();
    }

    private void GiveCoins(int amount)
    {
        UnityEngine.Debug.Log(amount);
        CoinsHolder.Add(amount);
    }
}