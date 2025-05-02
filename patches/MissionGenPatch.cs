using HarmonyLib;
using MissionConfig;
using PlayerJobsPlugin;
using System;
using System.Collections.Generic;
using System.Reflection;

[HarmonyPatch]
public class MissionGenPatch
{
    [HarmonyPatch(typeof(MissionGen), "GetRandomDirectMissions")]
    class GetRandomDirectMissions
    {
        static bool Prefix(MissionGen __instance, ref int numMissions, ref ServerMap.TipoRed tipoRed, ref bool forceMinRep, ref List<DirectMission> __result)
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            List<DirectMission> list = new List<DirectMission>();
            FieldInfo hsFieldInfo = AccessTools.Field(typeof(MissionGen), "dmHackShop");
            List<DirectMissionPreview> dmHackShop = hsFieldInfo.GetValue(__instance) as List<DirectMissionPreview>;
            FieldInfo pFieldInfo = AccessTools.Field(typeof(MissionGen), "dmPolice");
            List<DirectMissionPreview> dmPolice = pFieldInfo.GetValue(__instance) as List<DirectMissionPreview>;
            List<DirectMissionPreview> dmPlayer = new List<DirectMissionPreview>();
            dmPlayer.AddRange(dmHackShop);
            dmPlayer.AddRange(dmPolice);
            List<DirectMissionPreview> list2 = dmPlayer;
            if (tipoRed == ServerMap.TipoRed.HackShop)
            {
                list2 = dmHackShop;
            }
            if (tipoRed == ServerMap.TipoRed.Comisaria)
            {
                list2 = dmPolice;
            }
            int minValue = (tipoRed == ServerMap.TipoRed.HackShop || tipoRed == ServerMap.TipoRed.RentServer ? 1 : 0);
            for (int i = 0; i < numMissions; i++)
            {
                list.Add(new DirectMission(list2[random.Next(minValue, list2.Count)], random, forceMinRep));
            }
            __result = list;
            return false;
        }
    }

    [HarmonyPatch(typeof(MissionGen), "GetMaxDirectMissions")]
    class GetMaxDirectMissionsPatch
    {
        static bool Prefix(MissionGen __instance, ref ServerMap.TipoRed tipoRed, ref int __result)
        {
            if (tipoRed == ServerMap.TipoRed.RentServer)
            {
                __result = Plugin.maxMissions.Value;
                return false;
            }
            if (tipoRed == ServerMap.TipoRed.Comisaria)
            {
                __result = 5;
                return false;
            }
            if (tipoRed == ServerMap.TipoRed.HackShop)
            {
                __result = 20;
                return false;
            }
            __result = 0;
            return false;
        }
    }
}