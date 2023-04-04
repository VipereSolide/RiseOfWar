using HarmonyLib;
using TMPro;
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
        private static void OnDamageDealtPatch(DamageInfo info, HitInfo hit)
        {
            if (info.isPlayerSource)
            {
                EventManager.onPlayerDealtDamage.Invoke(new OnPlayerDealtDamageEvent(info, hit));
            }
        }

        [HarmonyPatch(typeof(IngameUi), "Awake")]
        [HarmonyPostfix]
        private static void AwakePatch(IngameUi __instance)
        {
            Plugin.Log("AwakePatch: Awaking...");

            LoadPlayerUI();
            InitializeHitmarker();
            CreateKillfeedUI();
            CreateGlobalKillfeedUI();
            SetupEvents();

            Plugin.Log("AwakePatch: Awakened.");
        }

        [HarmonyPatch(typeof(IngameUi), "Update")]
        [HarmonyPrefix]
        private static void UpdatePatch(IngameUi __instance)
        {
            __instance.forceHide = true;

            UpdateHitmarkerLifetime();
        }

        private static void OnPlayerDealtDamageListener(OnPlayerDealtDamageEvent _event)
        {
            ShowHitmarker(false);
        }

        private static void OnActorDieListener(OnActorDieEvent _event)
        {
            if (_event.victim == ActorManager.instance.player || _event.damage.sourceActor != ActorManager.instance.player || _event.victim.dead)
            {
                return;
            }

            ShowHitmarker(true);
        }

        private static void SetupEvents()
        {
            EventManager.onPlayerDealtDamage += OnPlayerDealtDamageListener;
            EventManager.onActorDie += OnActorDieListener;
        }

        private static void LoadPlayerUI()
        {
            if (ResourceManager.Instance.PlayerUIAssetBundle == null)
            {
                ResourceManager.Instance.LoadBundlePlayerUI();
            }

            Plugin.Log("AwakePatch: Loading up Player UI...");

            string _playerUIPath = "assets/player ui/prefabs/player ui.prefab";
            GameObject _playerUIPrefab = (GameObject)ResourceManager.Instance.PlayerUIAssetBundle.LoadAsset(_playerUIPath);
            GameObject _playerUI = GameObject.Instantiate(_playerUIPrefab);
            _playerUI.gameObject.AddComponent<PlayerUI>().Awake();

            Plugin.Log("AwakePatch: Successfully loaded up Player UI.");
        }

        private static void UpdateHitmarkerLifetime()
        {
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

        private static void InitializeHitmarker()
        {
            Plugin.Log("IngameUiPatcher: Initializing hitmarker...");

            try
            {
                RectTransform _hitmarkerTransform = new GameObject().AddComponent<RectTransform>();
                _hitmarkerTransform.SetParent(PlayerUI.instance.transform, false);

                _hitmarkerTransform.anchoredPosition = Vector2.zero;
                _hitmarkerTransform.anchorMax = new Vector2(0.5f, 0.5f);
                _hitmarkerTransform.anchorMin = new Vector2(0.5f, 0.5f);

                _hitmarkerTransform.sizeDelta = new Vector2(32, 32);

                _hitmarkerImage = _hitmarkerTransform.gameObject.AddComponent<RawImage>();
                _hitmarkerImage.texture = ResourceManager.Instance.HitmarkerTexture;
                
                DisableHitmarker();
            }
            catch (Exception _exception)
            {
                Plugin.LogError("IngameUiPatcher: Couldn't initialize hitmarker! " + _exception);
                return;
            }

            Plugin.Log("IngameUiPatcher: Successfully initialized hitmarker.");
        }

        private static void ShowHitmarker(bool kill)
        {
            _hitmarkerImage.transform.SetAsLastSibling();
            _hitmarkerImage.color = (kill) ?
                GameConfiguration.hitmarkerKillColor :
                GameConfiguration.hitmarkerNormalHitColor;

            _hitmarkerCurrentLifetime = GameConfiguration.hitmarkerLifetime;
        }

        private static void DisableHitmarker()
        {
            // Usage of color32 here because it is more efficient than Color performance
            // wise. Color32 uses bytes (and is the "base" class) while Color uses floats
            // (larger) then converted to bytes.
            _hitmarkerImage.color = new Color32(0, 0, 0, 0);
        }

        private static void CreateKillfeedUI()
        {
            Plugin.Log("IngameUiPatcher: Loading Killfeed UI...");

            GameObject _killfeedCanvasObject = (GameObject)ResourceManager.Instance.KillfeedAssetBundle.LoadAsset("assets/killfeed/prefabs/killfeed.prefab");

            if (_killfeedCanvasObject == null)
            {
                Plugin.LogError("IngameUiPatcher: Couldn't load killfeed UI! Killfeed canvas object was null.");
                return;
            }

            string _killfeedItemPath = "assets/killfeed/prefabs/Item.prefab";

            GameObject _killfeedCanvas = GameObject.Instantiate(_killfeedCanvasObject);
            GameObject _killfeedContainer = _killfeedCanvas.transform.Find("Canvas/Killfeed").gameObject;
            GameObject _itemPrefab = (GameObject)ResourceManager.Instance.KillfeedAssetBundle.LoadAsset(_killfeedItemPath);

            KillfeedManager _killfeedManager = _killfeedContainer.AddComponent<KillfeedManager>();
            _killfeedManager.Setup(_killfeedContainer.transform, _itemPrefab);

            Plugin.Log("IngameUiPatcher: Loaded killfeed UI.");
        }

        private static void CreateGlobalKillfeedUI()
        {
            Plugin.Log("IngameUiPatcher: Loading global killfeed UI...");

            ResourceManager _resourceManager = ResourceManager.Instance;

            if (_resourceManager.GlobalKillfeedItemPrefab == null || _resourceManager.GlobalKillfeedPrefab == null)
            {
                Plugin.LogError("IngameUiPatcher: Could not load global killfeed UI! Item or global killfeed prefab could not be loaded by the ResourceManager.");
                return;
            }

            GameObject _globalKillfeed = GameObject.Instantiate(_resourceManager.GlobalKillfeedPrefab);
            _globalKillfeed.transform.name = "Global Killfeed UI";

            TMP_Text _globalKillfeedItemPrefab = _resourceManager.GlobalKillfeedItemPrefab.GetComponent<TMP_Text>();
            GlobalKillfeed _globalKillfeedComponent = _globalKillfeed.AddComponent<GlobalKillfeed>();
            _globalKillfeedComponent.Setup(_globalKillfeedItemPrefab);

            Plugin.Log("IngameUiPatcher: Loaded global killfeed UI.");
        }
    }
}