using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace RiseOfWar
{
    using Events;
    using System;

    public class IngameUiPatcher
    {
        static RawImage _hitmarkerImage;
        static float _hitmarkerCurrentLifetime;

        [HarmonyPatch(typeof(IngameUi), "OnDamageDealt")]
        [HarmonyPostfix]
        static void OnDamageDealtPatch(DamageInfo info, HitInfo hit)
        {
            if (info.isPlayerSource)
            {
                EventManager.onPlayerDealtDamage.Invoke(new OnPlayerDealtDamageEvent(info, hit));
            }
        }

        [HarmonyPatch(typeof(IngameUi), "Awake")]
        [HarmonyPostfix]
        static void AwakePatch(IngameUi __instance)
        {
            Plugin.Log("AwakePatch: Awaking...");

            if (ResourceManager.Instance.playerUIAssetBundle == null)
            {
                ResourceManager.Instance.LoadPlayerUI();
            }

            Plugin.Log("AwakePatch: Loading up player ui...");

            GameObject _playerUIPrefab = (GameObject)ResourceManager.Instance.playerUIAssetBundle.LoadAsset("assets/player ui/prefabs/player ui.prefab");
            GameObject _playerUI = GameObject.Instantiate(_playerUIPrefab);
            _playerUI.gameObject.AddComponent<PlayerUI>().Awake();
            
            Plugin.Log("AwakePatch: Successfully loaded up player ui.");

            CreateHitmarker(__instance);
            CreateKillfeedUI();

            EventManager.onPlayerDealtDamage += OnPlayerDealtDamageListener;
            EventManager.onActorDie += OnActorDieListener;
        }

        [HarmonyPatch(typeof(IngameUi), "Update")]
        [HarmonyPrefix]
        static void UpdatePatch(IngameUi __instance)
        {
            __instance.forceHide = true;

            if (_hitmarkerCurrentLifetime > 0)
            {
                _hitmarkerCurrentLifetime -= Time.deltaTime;
            }

            if (_hitmarkerCurrentLifetime < 0)
            {
                _hitmarkerCurrentLifetime = 0;
            }

            if (_hitmarkerCurrentLifetime == 0 && _hitmarkerImage.color != new Color32(0, 0, 0, 0))
            {
                DisableHitmarker();
            }
        }

        static void CreateHitmarker(IngameUi _instance)
        {
            Plugin.Log("IngameUiPatcher: Creating hitmarker...");

            try
            {
                RectTransform _hitmarker = new GameObject().AddComponent<RectTransform>();
                _hitmarker.SetParent(PlayerUI.instance.transform, false);
                _hitmarker.anchoredPosition = Vector2.zero;
                _hitmarker.anchorMax = new Vector2(0.5f, 0.5f);
                _hitmarker.anchorMin = new Vector2(0.5f, 0.5f);
                _hitmarker.sizeDelta = new Vector2(32, 32);

                _hitmarkerImage = _hitmarker.gameObject.AddComponent<RawImage>();
                _hitmarkerImage.texture = ResourceManager.Instance.hitmarkerTexture;
                DisableHitmarker();
            }
            catch (Exception _exception)
            {
                Plugin.LogError("IngameUiPatcher: Couldn't create hitmarker! " + _exception);
                return;
            }

            Plugin.Log("IngameUiPatcher: Successfully created hitmarker!");
        }

        static void OnPlayerDealtDamageListener(OnPlayerDealtDamageEvent _event)
        {
            ShowHitmarker(IngameUi.instance, false);
        }

        static void OnActorDieListener(OnActorDieEvent _event)
        {
            if (_event.victim == ActorManager.instance.player || _event.damage.sourceActor != ActorManager.instance.player || _event.victim.dead)
            {
                return;
            }

            ShowHitmarker(IngameUi.instance, true);
        }

        static void ShowHitmarker(IngameUi _instance, bool _isKill)
        {
            _hitmarkerImage.transform.SetAsLastSibling();
            _hitmarkerImage.color = (_isKill) ? Color.red : Color.white;

            _hitmarkerCurrentLifetime = GameConfiguration.hitmarkerLifetime;
        }

        static void DisableHitmarker()
        {
            // Usage of color32 here because it is more efficient than Color performance
            // wise. Color32 uses bytes (and is the "base" class) while Color uses floats
            // (larger) then converted to bytes.
            _hitmarkerImage.color = new Color32(0, 0, 0, 0);
        }

        static void CreateKillfeedUI()
        {
            Plugin.Log("IngameUiPatcher: Loading killfeed ui...");
            GameObject _killfeedCanvasObject = (GameObject)ResourceManager.Instance.killfeedAssetBundle.LoadAsset("assets/killfeed/prefabs/killfeed.prefab");

            if (_killfeedCanvasObject == null)
            {
                Plugin.LogError("IngameUiPatcher: Couldn't load killfeed ui!");
                return;
            }

            GameObject _killfeedCanvas = GameObject.Instantiate(_killfeedCanvasObject);
            Transform _container = _killfeedCanvas.transform.Find("Canvas/Killfeed");
            GameObject _item = (GameObject)ResourceManager.Instance.killfeedAssetBundle.LoadAsset("assets/killfeed/prefabs/Item.prefab");
            _container.gameObject.AddComponent<KillfeedManager>().Setup(_container, _item);

            Plugin.Log("IngameUiPatcher: Successfully loaded killfeed ui!");
        }
    }
}