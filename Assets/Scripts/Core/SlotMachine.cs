using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlotGame.Symbols;
using SlotGame.Audio;
using SlotGame.Utilities;
using SlotGame.Core;

namespace SlotGame.Core
{
    public class SlotMachine : MonoBehaviour
    {
        [SerializeField] private Reel[] reels;
        [SerializeField] private SymbolView symbolPrefab;
        [SerializeField] private Transform reelsParent;
        [SerializeField] private List<SymbolData> symbolPool;

        [SerializeField] private Button spinButton;
        [SerializeField] private Button betUpButton;
        [SerializeField] private Button betDownButton;
        [SerializeField] private TMP_Text balanceText;
        [SerializeField] private TMP_Text betText;
        [SerializeField] private TMP_Text winText;
        [SerializeField] private TMP_Text freeSpinsText;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject jackpotPanel;

        [SerializeField] private int startingBalance = 1000;
        [SerializeField] private int betStep = 10;
        [SerializeField] private int minBet = 10;
        [SerializeField] private int maxBet = 100;
        [SerializeField] private float reelStopStagger = 0.25f;

        private int balance;
        private int bet;
        private int freeSpins;
        private bool spinning;
        private int freeSpinMultiplier = 1;

        private void Start()
        {
            balance = startingBalance;
            bet = minBet;

            foreach (var reel in reels)
            {
                reel.Initialize(symbolPrefab, reelsParent);
                reel.SetSymbolPool(symbolPool);
            }

            spinButton.onClick.AddListener(OnSpinClicked);
            betUpButton.onClick.AddListener(() => ChangeBet(betStep));
            betDownButton.onClick.AddListener(() => ChangeBet(-betStep));

            UpdateUI();
        }

        private void ChangeBet(int delta)
        {
            if (spinning) return;
            bet = Mathf.Clamp(bet + delta, minBet, maxBet);
            UpdateUI();
        }

        private void OnSpinClicked()
        {
            if (spinning) return;
            if (freeSpins <= 0 && balance < bet)
            {
                winText.text = "Insufficient balance";
                return;
            }
            StartCoroutine(SpinAll());
        }

        private IEnumerator SpinAll()
        {
            spinning = true;
            spinButton.interactable = false;

            bool usingFreeSpin = freeSpins > 0;
            if (usingFreeSpin)
            {
                freeSpins--;
                if (freeSpins <= 0) freeSpinMultiplier = 1;
            }
            else
            {
                balance -= bet;
            }

            var targets = GenerateOutcome();

            foreach (var reel in reels) reel.StartSpin();
            if (AudioManager.Instance) AudioManager.Instance.PlaySpin();

            for (int i = 0; i < reels.Length; i++)
            {
                yield return new WaitForSeconds(reelStopStagger);
                reels[i].Stop(targets[i]);
                if (AudioManager.Instance) AudioManager.Instance.PlayStop();
            }

            while (AnySpinning()) yield return null;

            var result = PayTable.Evaluate(targets, bet, freeSpinMultiplier);

            if (result.isWin)
            {
                balance += result.payout;
                winText.text = result.isJackpot ? $"JACKPOT! +{result.payout}" : $"WIN +{result.payout}";
                if (result.isJackpot)
                {
                    if (jackpotPanel) jackpotPanel.SetActive(true);
                    if (AudioManager.Instance) AudioManager.Instance.PlayBigWin();
                }
                else if (AudioManager.Instance)
                {
                    AudioManager.Instance.PlayWin();
                }
            }
            else
            {
                winText.text = "Try again";
            }

            if (result.bonusTriggered)
            {
                freeSpins += 10;
                freeSpinMultiplier = 2;
                if (AudioManager.Instance) AudioManager.Instance.PlayBonus();
                if (freeSpinsText) freeSpinsText.text = $"Free Spins: {freeSpins}";
            }

            spinning = false;
            spinButton.interactable = true;
            UpdateUI();
        }

        private bool AnySpinning()
        {
            foreach (var r in reels) if (r.IsSpinning) return true;
            return false;
        }

        private SymbolData[][] GenerateOutcome()
        {
            int rows = 3;
            var grid = new SymbolData[reels.Length][];
            for (int r = 0; r < reels.Length; r++)
            {
                grid[r] = new SymbolData[rows];
                for (int row = 0; row < rows; row++)
                    grid[r][row] = PickWeighted();
            }
            return grid;
        }

        private SymbolData PickWeighted()
        {
            int[] w = new int[symbolPool.Count];
            for (int i = 0; i < symbolPool.Count; i++) w[i] = symbolPool[i].weight;
            return symbolPool[RNGService.WeightedIndex(w)];
        }

        private void UpdateUI()
        {
            if (balanceText) balanceText.text = $"Balance: {balance}";
            if (betText) betText.text = $"Bet: {bet}";
            if (freeSpinsText) freeSpinsText.text = $"Free Spins: {freeSpins}";
        }
    }
}