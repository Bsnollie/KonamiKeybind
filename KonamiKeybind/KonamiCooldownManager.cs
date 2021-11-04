using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.KonamiKeybind
{
    public class KonamiCooldownManager : IAutoUpdate
    {
        private static KonamiCooldownManager s_instance;

        private static IDictionary<Duck, ActionTimer> s_cooldowns = new Dictionary<Duck, ActionTimer>();

        /// <summary>
        /// Checks if a duck has cooldown.
        /// </summary>
        /// <param name="duck">Duck to check.</param>
        /// <returns>True if <paramref name="duck"/> has cooldown, false otherwise.</returns>
        public static bool CheckCooldown(Duck duck)
        {
            return s_cooldowns.Keys.Contains(duck);
        }

        /// <summary>
        /// Add cooldown to certain duck.
        /// </summary>
        /// <param name="duck"></param>
        public static void AddCooldown(Duck duck)
        {
            if (CheckCooldown(duck)) return;

            ActionTimer timer = new ActionTimer(0.01f, 0.65f);
            s_cooldowns.Add(duck, timer);
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
            IList<Duck> remove = new List<Duck>();

            // Find finished cooldowns
            foreach (KeyValuePair<Duck, ActionTimer> kPair in s_cooldowns)
            {
                if (kPair.Value)
                {
                    remove.Add(kPair.Key);
                    AutoUpdatables.core._updateables.Remove(new WeakReference(kPair.Value));
                }
            }

            // Removed finished cooldowns
            foreach (Duck duck in remove)
            {
                s_cooldowns.Remove(duck);
            }
        }
    }
}
