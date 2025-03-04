using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
    public class ScoreGUIController : MonoBehaviour
    {
        [SerializeField]
        private GameObject ScoreGroup;
        [SerializeField]
        private GameObject ScoreComplete;
        [SerializeField]
        private Text scoreAmountText;
        [SerializeField]
        private bool showTargetScore;
        [SerializeField]
        private string targetScoreSeparator;
        [SerializeField]
        private bool showOnlyIfTarget = false;
        [SerializeField]
        private bool showTargetInEditMode = true;

        #region temp vars
        private TweenIntValue tween;
        private GameBoard MBoard { get { return GameBoard.Instance; } }
        private WinController WController { get { return MBoard ? MBoard.WinContr : null; } }
        private GameConstructor GConstructor { get { return (MBoard) ? MBoard.gConstructor : null; } }
        private ScoreHolder MScore => ScoreHolder.Instance;

        private bool isScoreLevel = false;
        private string targetScoreString = "";
        private bool showScore = false;
        #endregion temp vars

        #region regular
        private IEnumerator Start()
        {
            while (!MBoard)
            {
                yield return new WaitForEndOfFrame();
            }

            if (ScoreComplete) ScoreComplete.SetActive(false);

            if (GameBoard.GMode == GameMode.Play)
            {
                while (!WController || !WController.Loaded)
                {
                    yield return new WaitForEndOfFrame();
                }
                isScoreLevel = WController.ScoreTarget > 0;
                showScore = ((isScoreLevel && showOnlyIfTarget) || (!showOnlyIfTarget));
                if (ScoreGroup) ScoreGroup.SetActive(showScore);
                if (showScore)
                {
                    // set player event handlers
                    MScore.ChangeEvent.AddListener(ChangeScoreHandler);
                    targetScoreString = (isScoreLevel && showTargetScore) ? targetScoreSeparator + WController.ScoreTarget : "";
                    if (scoreAmountText) tween = new TweenIntValue(scoreAmountText.gameObject, ScoreHolder.Count, 1, 3, true, (s) => { if (this && scoreAmountText) scoreAmountText.text = GetScoreString(s); });
                }
            }
#if UNITY_EDITOR
            else // edit mode
            {
                while (!GConstructor)
                {
                    yield return new WaitForEndOfFrame();
                }
                MBoard.gConstructor.ChangeTargetScoreEvent += ChangeScoreTargetHandler;
            }
#endif
            Refresh();

        }

        private void OnDestroy()
        {
            if (showScore)
            {
                // remove player event handlers
                MScore.ChangeEvent.RemoveListener(ChangeScoreHandler);
            }
            if (scoreAmountText) SimpleTween.Cancel(scoreAmountText.gameObject, false);
        }
        #endregion regular

        private void Refresh()
        {
            if (GameBoard.GMode == GameMode.Play)
            {
                if (scoreAmountText) scoreAmountText.text = GetScoreString(ScoreHolder.Count);
                if (ScoreComplete && MBoard.WinContr != null) ScoreComplete.SetActive(WController.HasScoreTarget && (ScoreHolder.Count >= WController.ScoreTarget));
            }
#if UNITY_EDITOR
            else
            {
                if (scoreAmountText && GConstructor) scoreAmountText.text = GetScoreString(GConstructor.ScoreTarget);
            }
#endif
        }

        private string GetScoreString(int score)
        {
          return  (score > 0) ? score.ToString() + targetScoreString : "0" + targetScoreString;
        }

        #region eventhandlers
        private void ChangeScoreHandler(int score)
        {
            if (tween != null) tween.Tween(score, 100);
            else
            {
                if (scoreAmountText) scoreAmountText.text = GetScoreString(score); 
            }

            if (ScoreComplete && WController) ScoreComplete.SetActive(WController.HasScoreTarget && (score >= WController.ScoreTarget));
        }

        private void ChangeScoreTargetHandler(int score)
        {
            if (scoreAmountText) scoreAmountText.text = GetScoreString(score);
        }
        #endregion eventhandlers
    }
}