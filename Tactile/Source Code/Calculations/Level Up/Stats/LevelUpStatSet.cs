using System.Collections.Generic;
using System.Linq;

namespace Tactile.Calculations.LevelUp.Stats
{
    class LevelUpStatSet
    {
        private List<LevelUpStats> Levels = new List<LevelUpStats>();
        private bool Leftover_Stat_Points_Exist;
        private int[] Leftover_Stat_Points;

        public bool leftover_stats_points_exist { get { return Leftover_Stat_Points_Exist; } }
        public int[] leftover_stat_points
        {
            get { return Leftover_Stat_Points; }
            set
            {
                Leftover_Stat_Points_Exist = true;
                Leftover_Stat_Points = value;
            }
        }
        public int LevelCount { get { return Levels.Count; } }

        public void AddLevel(LevelUpStats level)
        {
            Levels.Add(level);
        }

        public LevelUpStats GetLevel(int index)
        {
            return Levels[index];
        }
        
        public int StatGains(Stat_Labels stat)
        {
            return StatGains((int)stat);
        }
        private int StatGains(int i)
        {
            return Levels.Sum(x => x.StatGains[i]);
        }
    }
}
