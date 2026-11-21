using UnityEngine;

namespace SlotGame.Bonus
{
    public class BonusFeature : MonoBehaviour
    {
        public int freeSpinsAwarded = 10;
        public int multiplier = 2;

        public bool TryTrigger(int scatterCount, out int spins, out int mult)
        {
            spins = 0; mult = 1;
            if (scatterCount < 3) return false;
            spins = freeSpinsAwarded + (scatterCount - 3) * 5;
            mult = multiplier;
            return true;
        }
    }
}