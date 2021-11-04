using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Runtime;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.KonamiKeybind
{
    public class KonamiCooldownManager : IAutoUpdate
    {
        private static KonamiCooldownManager s_instance;

        private static IDictionary<ushort, int> s_cooldowns = new Dictionary<ushort, int>();

        /// <summary>
        /// Checks if a duck has cooldown, if it doesn't, then cooldown is added.
        /// </summary>
        /// <param name="duck">Duck to check.</param>
        /// <returns>True if <paramref name="duck"/> has cooldown, false otherwise.</returns>
        public static bool CheckOrAddCooldown(Duck duck)
        {
            ushort index = duck.globalIndex;
            bool flag = s_cooldowns.ContainsKey(index);

            if  (!flag)
            {
                int date = (int)MonoMain.GetLocalTime().AddSeconds(1.15f).Ticks;

                s_cooldowns.Add(index, date);
            }

            return flag;
        }

        public static KonamiCooldownManager Instance => s_instance;

        public static void Initialize()
        {
            s_instance = new KonamiCooldownManager();
        }

        private KonamiCooldownManager()
        {
            AutoUpdatables.Add(this);
        }

        public void Update()
        {
            int date = (int)MonoMain.GetLocalTime().Ticks;
            List<ushort> removals = new List<ushort>();

            foreach (KeyValuePair<ushort, int> pair in s_cooldowns)
            {
                if (date >= pair.Value) removals.Add(pair.Key);
            }

            foreach (ushort address in removals)
            {
                s_cooldowns.Remove(address).ToString();
            }
        }
    }
}
