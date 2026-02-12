using BepInEx;
using HarmonyLib;
using System.Collections;
using UnityEngine;

[BepInPlugin("com.yourname.kc_crack", "KC Universal Plugin", "1.0.0")]
public class KCCrackPlugin : BaseUnityPlugin
{
    void Awake()
    {
        // 1. 初始化 Harmony 补丁引擎
        var harmony = new Harmony("com.ppx.kc_crack");
        harmony.PatchAll();

        Logger.LogInfo(">>> 持久化补丁已加载：DRM 屏蔽 + 全解锁激活 <<<");
    }
}

// 屏蔽 DRM 并强制激活入口 ---
[HarmonyPatch(typeof(TitleScreen.TitleSceneController), "Start")]
public class DRM_Bypass_Patch
{
    static void Postfix(TitleScreen.TitleSceneController __instance)
    {
        // 在游戏原本的 Start 执行后，强行启动我们的“清理协程”
        __instance.StartCoroutine(KillBlocker(__instance));
    }

    static IEnumerator KillBlocker(TitleScreen.TitleSceneController controller)
    {
        bool activated = false;
        while (true)
        {
            if (controller == null) yield break;

            // 寻找拦截器
            GameObject blocker = GameObject.Find("PleaseConnectToItch");
            if (blocker != null)
            {
                UnityEngine.Object.Destroy(blocker);

                if (!activated)
                {
                    controller.ActivateGame();
                    controller.pressStartText?.SetActive(true);
                    controller.EnableInput();
                    activated = true;
                }
            }
            yield return new WaitForSeconds(1.0f); // 每秒巡逻一次
        }
    }
}

// 全 CG & 回想解锁 ---
// 拦截 UVNGalleryImage 的解锁判断
[HarmonyPatch(typeof(UVNGalleryImage), "CheckUnlockConditions")]
public class GalleryUnlock_Patch
{
    static bool Prefix(ref bool __result)
    {
        __result = true; // 强行设为已解锁
        return false;    // 跳过原程序的逻辑
    }
}

// 拦截 KCSceneReplayEntry 的解锁判断
[HarmonyPatch(typeof(KCSceneReplayEntry), "CheckUnlockCondition")]
public class ReplayUnlock_Patch
{
    static bool Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }
}