using UnityEngine;

namespace Mkey
{
    public class BoosterMovePlus5 : BoosterFunc
    {
        #region override
        public override bool ActivateApply()
        {
            MBoard.WinContr.AddMoves(5);
            return true;
        }
        #endregion override
    }
}

