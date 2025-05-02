using HarmonyLib;
using System;
using ServiceConfig;
using Util;
using UnityEngine;
using System.Reflection;

[HarmonyPatch]
public class NetworkHelperServerPatch
{
    [HarmonyPatch(typeof(NetworkHelperServer), "AccessWeb")]
    class AccessWebPatch
    {
        static bool Prefix(NetworkHelperServer __instance, ref string remoteIp, ref string origIp, ref int puerto, ref string path, ref string propActiveUser, ref string fromComputerNetID, ref int windowPID, out bool isRouter, out Computer targetComputer, out string contenidoWeb, ref string __result)
        {
            isRouter = false;
            targetComputer = null;
            contenidoWeb = "";
            try
            {
                Computer remoteComputer = ServerMap.Singleton.GetRemoteComputer(fromComputerNetID, true, null);
                if (remoteComputer == null)
                {
                    __result = "Router error: unknown error";
                    return false;
                }
                bool flag = IP.IsValidIP(remoteIp, false) && IP.IsLanIp(remoteIp);
                if (remoteComputer.GetTipoRed() == ServerMap.TipoRed.CTF && !flag)
                {
                    __result = "Error: This network has restricted access to the outside.";
                    return false;
                }
                MethodInfo methodInfo = AccessTools.Method(typeof(HelperServer), "BrowserCanSaveInfo", new Type[]
                {
                    typeof(Computer),
                    typeof(Router),
                    typeof(string),
                    typeof(string),
                    typeof(bool)
                });
                string text = methodInfo.Invoke(__instance, new object[] { remoteComputer, null, path, propActiveUser, true }) as string;
                if (!string.IsNullOrEmpty(text))
                {
                    __result = text;
                    return false;
                }
                text = __instance.ConnectComputer(remoteIp, ref origIp, remoteComputer, puerto, windowPID, out targetComputer, false, null);
                if (!string.IsNullOrEmpty(text))
                {
                    if (targetComputer != null)
                    {
                        Database.Singleton.UpdateWebAccess(targetComputer, true);
                    }
                    __result = text;
                    return false;
                }
                string text2 = Networking.CheckServiceOnline(targetComputer, ServicioID.http, origIp, puerto, remoteIp, true);
                if (!string.IsNullOrEmpty(text2))
                {
                    __result = "The connection could not be established: \n" + text2;
                    return false;
                }
                if (targetComputer.IsRouter())
                {
                    isRouter = true;
                    __result = "";
                    return false;
                }
                FileSystem.Archivo archivo = targetComputer.GetFileSystem().GetArchivo("/Public/htdocs/website.html", true, 0);
                if (archivo == null)
                {
                    __result = "url_not_found";
                    return false;
                }
                if (archivo.IsBinario())
                {
                    __result = "can't read website.html. Binary file.";
                    return false;
                }
                text = Networking.CheckServiceOnline(targetComputer, ServicioID.http, origIp, puerto, remoteIp, true);
                if (!string.IsNullOrEmpty(text))
                {
                    Database.Singleton.UpdateWebAccess(targetComputer, false);
                    __result = text;
                    return false;
                }
                FieldInfo fieldInfo = AccessTools.Field(typeof(NetworkHelperServer), "player");
                PlayerServer player = fieldInfo.GetValue(__instance) as PlayerServer;
                Database.Singleton.IncreaseVisit(player.GetComputerID(), targetComputer);
                contenidoWeb = archivo.GetContenido();
                player.playerHelper.userConnections.AddConnection(targetComputer.GetID(), "guest");
                player.fileHelper.WriteLog(LogSystem.StatusLog.open, origIp, puerto, targetComputer, -1);
                targetComputer.NpcActions();
                ServerMap.TipoRed tipoRed = targetComputer.GetTipoRed();
                if (tipoRed == ServerMap.TipoRed.HackShop || tipoRed == ServerMap.TipoRed.Comisaria || tipoRed == ServerMap.TipoRed.RentServer)
                {
                    PlayerComputer computer = player.GetComputer();
                    targetComputer.FillDirectMissions(computer.GetUsers(true)[1].GetCurrentLevel() == 0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            __result = "";
            return false;
        }
    }
}