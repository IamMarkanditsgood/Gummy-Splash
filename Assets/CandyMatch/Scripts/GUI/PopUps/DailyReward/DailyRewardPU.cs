using UnityEngine;

namespace Mkey
{
	public class DailyRewardPU : PopUpsController
	{
        [SerializeField]
        private DailyRewardLine[] dailyRewardLines;

        #region temp vars
        private DailyRewardController DRC => DailyRewardController.Instance; 
        private int rewDay = -1;
        private DailyRewardLine currLine;
        #endregion temp vars

        public override void RefreshWindow()
        {
            rewDay = DRC.RewardDay;
            if (rewDay < 0) return;
            int rewLength = dailyRewardLines.Length;
            int rewDayCl = DRC.RepeatingReward ? rewDay % rewLength : Mathf.Clamp(rewDay, 0, rewLength - 1);
            currLine = dailyRewardLines[rewDayCl];

            for (int day = 0; day < rewLength; day++)
            {
                dailyRewardLines[day].SetData( day, rewDayCl);
            }
            base.RefreshWindow();
        }

        public void Apply_Click()
        {
            if (rewDay >= 0)
            {
                DRC.ApplyReward();
                if (currLine) currLine.Apply();
            }
            CloseWindow();
        }
    }
}
