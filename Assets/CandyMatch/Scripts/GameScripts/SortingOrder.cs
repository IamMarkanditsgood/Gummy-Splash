using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public static class SortingOrder
    {
        #region sorting orders
        public static int Base
        {
            get
            {
                return 2;
            }
        }
        public static int SubUnder
        {
            get
            {
                return Base + 5;
            }
        }

        public static int SubUnderToFront
        {
            get
            {
                return Base + 13;
            }
        }

        public static int Blocked
        {
            get
            {
                return Main;
            }
        }

        public static int BlockedToFront
        {
            get
            {
                return MainToFront;
            }
        }

        public static int Hidden
        {
            get
            {
                return Base + 7;
            }
        }

        public static int HiddenToFront
        {
            get
            {
                return Base + 7;
            }
        }

        public static int Under
        {
            get
            {
                return Base + 8;
            }
        }

        public static int UnderToFront
        {
            get
            {
                return Base + 13;
            }
        }

        public static int Over
        {
            get
            {
                return Base + 12;
            }
        }

        public static int OverToFront
        {
            get
            {
                return Base + 14;
            }
        }

        public static int Main
        {
            get
            {
                return Base + 9;
            }
        }

        public static int MainToFront
        {
            get
            {
                return Base + 13;
            }
        }

        public static int Treasure
        {
            get
            {
                return Base + 9;
            }
        }

        public static int TreasureToFront
        {
            get
            {
                return Base + 13;
            }
        }


        public static int MainIddle
        {
            get
            {
                return MainToFront+1;
            }
        }

        public static int StaticMatchBomb
        {
            get
            {
                return Main - 1;
            }
        }

        public static int StaticMatchBombIddleAnim
        {
            get
            {
                return StaticMatchBomb + 2;
            }
        }

        public static int StaticMatchBombToFront
        {
            get
            {
                return Base + 14;
            }
        }

        public static int DynamicMatchBomb
        {
            get
            {
                return Main + 1;
            }
        }

        public static int DynamicMatchBombIddleAnim
        {
            get
            {
                return DynamicMatchBomb + 2;
            }
        }

        public static int DynamicMatchBombToFront
        {
            get
            {
                return DynamicMatchBomb + 1;
            }
        }

        public static int DynamicClickBomb
        {
            get
            {
                return Main + 1;
            }
        }

        public static int DynamicClickBombIddleAnim
        {
            get
            {
                return DynamicClickBomb + 2;
            }
        }

        public static int DynamicClickBombToFront
        {
            get
            {
                return DynamicClickBomb + 1;
            }
        }

        public static int MainExplode
        {
            get
            {
                return Base + 22;
            }
        }

        public static int Booster
        {
            get { return Base + 16; }
        }

        public static int BoosterToFront
        {
            get { return GuiOverlay; }
        }

        public static int BombCreator
        {
            get { return Main - 1; }
        }

        public static int GuiOverlay { get { return 120; } }
        #endregion sorting orders
    }
}