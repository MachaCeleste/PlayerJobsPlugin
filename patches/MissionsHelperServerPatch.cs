using HarmonyLib;
using NetworkMessages;
using Newtonsoft.Json;
using PlayerJobsPlugin;
using System;
using System.Reflection;

[HarmonyPatch]
public class MissionsHelperServerPatch
{
    [HarmonyPatch(typeof(MissionsHelperServer), "GetMissionsServerRpc")]
    class GetMissionsServerRpcPatch
    {
        static bool Prefix(MissionsHelperServer __instance, ref string webRemoteNetID, ref int windowPID, ref bool showAllMissions)
        {
            Computer remoteComputer = ServerMap.Singleton.GetRemoteComputer(webRemoteNetID, true, null);
            if (remoteComputer == null)
            {
                MethodInfo methodInfo = AccessTools.Method(typeof(HelperServer), "CloseConnection", new Type[]
                {
                    typeof(int),
                    typeof(string),
                    typeof(bool)
                });
                methodInfo.Invoke(__instance, new object[] { windowPID, "Host is down", false });
                return false;
            }
            if (remoteComputer.GetTipoRed() != ServerMap.TipoRed.HackShop && remoteComputer.GetTipoRed() != ServerMap.TipoRed.Comisaria && remoteComputer.GetTipoRed() != ServerMap.TipoRed.RentServer)
            {
                return false;
            }
            FileSystem.Archivo archivo = remoteComputer.GetFileSystem().GetArchivo("/server/missions.db", true, 0);
            string text = "";
            if (archivo != null)
            {
                if (!archivo.IsBinario() || archivo.IsTypeFile(FileSystem.Fichero.TypeFile.Script))
                {
                    return false;
                }
                text = archivo.GetContenido();
            }
            byte[] value = new byte[0];
            if (!string.IsNullOrEmpty(text))
            {
                value = GCompressor.Zip(text);
            }
            string value2 = JsonConvert.SerializeObject(__instance.GetMissionsCooldown());
            MessageClient messageClient = new MessageClient(IdClient.SendMissionsInfoClientRpc);
            messageClient.AddByte(value);
            messageClient.AddInt(windowPID);
            messageClient.AddBool(showAllMissions);
            messageClient.AddString(value2);
            FieldInfo fieldInfo = AccessTools.Field(typeof(MissionsHelperServer), "player");
            PlayerServer player = fieldInfo.GetValue(__instance) as PlayerServer;
            player.SendData(messageClient);
            return false;
        }
    }
}