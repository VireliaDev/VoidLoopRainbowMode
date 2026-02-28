using System.Collections;
using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[BepInPlugin("virelia.voidloop.rainbowmode", "Rainbow Mode (Virelia)", "1.0")]
public class RainbowPlugin : BaseUnityPlugin
{
    private bool created;

    private void Awake()
    {
        new Harmony("virelia.voidloop.rainbowmode").PatchAll();
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(Initialize());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        created = false;
    }

    private IEnumerator Initialize()
    {
        yield return null;
        yield return new WaitForSeconds(1f);
        CreateRainbowOverlay();
    }

    private void Update()
    {
        if (created) return;

        var binders = Object.FindObjectsByType<SettingBinder>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        // If the Rainbow row already exists in the current UI, don't add another
        foreach (var b in binders)
        {
            if (b.settingFieldName == "rainbowMode" && b.gameObject.activeInHierarchy)
            {
                created = true;
                return;
            }
        }

        foreach (var binder in binders)
        {
            if (binder.settingFieldName != "greenMode") continue;
            if (!binder.gameObject.activeInHierarchy) continue;

            var rowRoot = binder.transform.parent.parent.gameObject;
            CreateRainbowToggle(rowRoot);

            created = true;
            return;
        }
    }

    private void CreateRainbowToggle(GameObject greenRow)
    {
        var parent = greenRow.transform.parent;

        var rainbowRow = Instantiate(greenRow, parent);
        rainbowRow.name = "Rainbow";
        rainbowRow.transform.SetSiblingIndex(greenRow.transform.GetSiblingIndex() + 1);

        var text = rainbowRow.GetComponentInChildren<TMP_Text>(true);
        if (text != null) text.text = "Rainbow Mode";

        var binder = rainbowRow.GetComponentInChildren<SettingBinder>(true);
        if (binder != null) binder.settingFieldName = "rainbowMode";

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(parent as RectTransform);
    }

    private void CreateRainbowOverlay()
    {
        if (GameObject.Find("RainbowCanvas") != null) return;

        var gm = GameObject.Find("GameManager");
        if (gm == null) return;

        var canvasObj = new GameObject("RainbowCanvas");
        canvasObj.transform.SetParent(gm.transform, false);

        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        var overlayObj = new GameObject("RainbowOverlay");
        overlayObj.transform.SetParent(canvasObj.transform, false);
        overlayObj.transform.SetAsLastSibling();

        overlayObj.AddComponent<RainbowModeController>();
    }
}

[HarmonyPatch(typeof(SettingBinder), "Start")]
internal static class RainbowBinderPatch
{
    private static bool Prefix(SettingBinder __instance)
    {
        if (__instance.settingFieldName != "rainbowMode")
            return true;

        var toggle = __instance.GetComponent<Toggle>();
        if (toggle == null)
            return false;

        var enabled = PlayerPrefs.GetInt("rainbowMode", 0) == 1;
        toggle.isOn = enabled;

        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(val =>
        {
            PlayerPrefs.SetInt("rainbowMode", val ? 1 : 0);
            PlayerPrefs.Save();
            SettingsManager.OnSettingsChanged?.Invoke();
        });

        return false;
    }
}