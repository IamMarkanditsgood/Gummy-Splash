using UnityEngine;

namespace Mkey
{
    public class BoosterTimePlus : BoosterFunc
    {
        #region override
        public override bool ActivateApply()
        {
            MBoard.WinContr.AddSeconds(30);
          //  MSound.PlayClip(0.2f, b.prefab.privateClip);
            return true;
        }
        #endregion override
    }
}

