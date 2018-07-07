using System.Reflection;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.UI;
using Harmony;
using SEModLoader;
using UnityEngine;

namespace OxygenTankLowPatch
{
    public class OxygenTankLowPatch : MonoBehaviour, IMod
    {
        public static object Instance = null;
        private static object _instanceLock = new object();

        public static string ModName = "SuitTankIndicatorMod";

        public static void Init()
        {
            lock (_instanceLock)
            {
                if (Instance == null)
                {
                    Instance = new object();
                    var harmony = HarmonyInstance.Create("com.zylanx.oxygentanklowpatch");
                    harmony.PatchAll(Assembly.GetExecutingAssembly());
                }
            }
        }

        public static bool IsSuitTankCaution()
        {
            if (StatusUpdates.Parent == null || StatusUpdates.Parent.AsHuman.SpaceSuit == null)
            {
                return false;
            }

            Human human = StatusUpdates.Parent.AsHuman;
            Suit suit = human.SpaceSuit;

            if (suit.AirTankSlot.Occupant != null && suit.AirTank.InternalAtmosphere != null)
            {
                float timeLeft = atmosOxygenTimeLeft(human, suit.AirTank.InternalAtmosphere);
                if (timeLeft > (5f * 60f))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsSuitTankCritical()
        {
            if (StatusUpdates.Parent == null || StatusUpdates.Parent.AsHuman.SpaceSuit == null)
            {
                return false;
            }

            Human human = StatusUpdates.Parent.AsHuman;
            Suit suit = human.SpaceSuit;

            if (suit.AirTankSlot.Occupant != null && suit.AirTank.InternalAtmosphere != null)
            {
                float timeLeft = atmosOxygenTimeLeft(human, suit.AirTank.InternalAtmosphere);
                if (timeLeft > 60f)
                {
                    return false;
                }
            }

            return true;
        }

        public static float atmosOxygenTimeLeft(Entity entity, Atmosphere atmos)
        {
            float ticksPerSecond = 1f / (float)AtmosphericsManager.Instance.TickSpeedMs;
            float oxygenPerTick = entity.MolePerBreath * entity.BreathingEfficiency;
            float oxygenPerSecond = oxygenPerTick * ticksPerSecond;

            float timeLeft = atmos.GasMixture.Oxygen.Quantity / oxygenPerSecond;

            return timeLeft;
        }
    }

    [HarmonyPatch(typeof(StatusUpdates))]
    [HarmonyPatch("IsOxygenCaution", PropertyMethod.Getter)]
    public class Patch_StatusUpdates_IsOxygenCaution
    {
        public static bool Postfix(bool __result)
        {
            return __result || OxygenTankLowPatch.IsSuitTankCaution();
        }
    }

    [HarmonyPatch(typeof(StatusUpdates))]
    [HarmonyPatch("IsOxygenCritical", PropertyMethod.Getter)]
    public class Patch_StatusUpdates_IsOxygenCritical
    {
        public static bool Postfix(bool __result)
        {
            return __result || OxygenTankLowPatch.IsSuitTankCritical();
        }
    }
}
