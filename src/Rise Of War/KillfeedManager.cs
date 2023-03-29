using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RiseOfWar
{
    using Events;
    using System.Collections;
    using System.Linq;

    public class KillfeedManager : MonoBehaviour
    {
        [Serializable]
        public struct KillfeedItem
        {
            public string message;
            public int score;

            public KillfeedItem(string message, int score)
            {
                this.message = message;
                this.score = score;
            }
        }

        public static KillfeedManager Instance { get; set; }

        [SerializeField]
        private GameObject _killfeedItemPrefab;

        [SerializeField]
        private Transform _feedItemsContainer;

        private List<CanvasGroup> _feedItems = new List<CanvasGroup>();
        private List<CanvasGroup> _toDestroy = new List<CanvasGroup>();

        private List<KillfeedItem> _killfeedItems = new List<KillfeedItem>();
        public KillfeedItem[] killfeedItems { get { return _killfeedItems.ToArray(); } }

        public static readonly string CAPTURED_POINT_MESSAGE = $"Captured <#{GameConfiguration.WHITE_COLOR}>67 XP</color>";
        public static readonly int CAPTURED_POINT_XP_AMOUNT = 67;

        public static readonly string NEUTRALIZED_POINT_MESSAGE = $"Neutralized <#{GameConfiguration.WHITE_COLOR}>45 XP</color>";
        public static readonly int NEUTRALIZED_POINT_XP_AMOUNT = 45;

        public static readonly string RAID_POINT_MESSAGE = $"Raid <#{GameConfiguration.WHITE_COLOR}>40 XP</color>";
        public static readonly int RAID_POINT_XP_AMOUNT = 40;

        public static readonly string DEFEND_POINT_MESSAGE = $"Defend <#{GameConfiguration.WHITE_COLOR}>65 XP</color>";
        public static readonly int DEFEND_POINT_XP_AMOUNT = 65;

        private List<Actor> _deadActors = new List<Actor>();
        private List<Vehicle> _deadVehicles = new List<Vehicle>();
        private List<Actor> _woundedActors = new List<Actor>();
        private Dictionary<Actor, float> _lastHitActors = new Dictionary<Actor, float>();
        private Dictionary<Vehicle, float> _lastHitVehicles = new Dictionary<Vehicle, float>();
        private float _lastPointCaptureInteraction = 0;

        public void AddKillfeedItem(string message, int score)
        {
            KillfeedItem _item = new KillfeedItem(message, score);
            AddKillfeedItem(_item);
        }

        public void AddKillfeedItem(KillfeedItem item)
        {
            _killfeedItems.Add(item);
        }

        public void Setup(Transform _container, GameObject _item)
        {
            if (_container == null)
            {
                Plugin.LogError("KillfeedManager: The provided container cannot be null! Assigning this object instead.");
                _container = transform;
            }

            if (_item == null)
            {
                Plugin.LogError("KillfeedManager: The provided item cannot be null!");
                return;
            }

            Plugin.Log("KillfeedManager: Successfully setup killfeed manager.");
            _killfeedItemPrefab = _item;
            _feedItemsContainer = _container;

            Start();
        }

        private void Start()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            AddListeners();

            Plugin.Log("KillfeedManager: Killfeed successfully initialized!");
        }

        private void Update()
        {
            HandleLivingKillfeedItems();

            if (_toDestroy.Count == 0)
            {
                return;
            }

            StartCoroutine(FadeItem(_toDestroy[0], 1.5f, true));
            _toDestroy.RemoveAt(0);
        }

        private IEnumerator FadeItem(CanvasGroup canvasGroup, float fadeTime, bool destroyOnEnd)
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            float _cooldown = fadeTime;

            while (_cooldown > 0)
            {
                _cooldown -= Time.deltaTime;
                canvasGroup.alpha = _cooldown / fadeTime;

                yield return new WaitForEndOfFrame();
            }

            if (destroyOnEnd)
            {
                Destroy(canvasGroup.gameObject);
                yield break;
            }

            canvasGroup.gameObject.SetActive(false);
        }

        private void AddListeners()
        {
            EventManager.onPlayerDealtDamage += OnPlayerDealtDamage;
            EventManager.onCapturePointInteraction += OnCapturePointInteraction;

            EventManager.onActorDie += OnActorDie;
            EventManager.onActorSpawn += OnActorSpawn;
            EventManager.onVehicleSpawn += OnVehicleSpawn;
        }

        private void OnCapturePointInteraction(OnCapturePointInteractionEvent _event)
        {
            bool _containsPlayer = _event.actorsOnPoint.ToList().Contains(FpsActorController.instance.actor);
            Plugin.Log("KillfeedManager: Contains player = " + _containsPlayer);

            if (Time.time < _lastPointCaptureInteraction + 0.5f)
            {
                return;
            }

            if (_event.type == OnCapturePointInteractionEvent.InteractionType.Captured)
            {
                if (_event.currentOwner != GameManager.PlayerTeam() || _containsPlayer == false)
                {
                    return;
                }

                AddKillfeedItem(CAPTURED_POINT_MESSAGE, CAPTURED_POINT_XP_AMOUNT);
            }
            else if (_event.type == OnCapturePointInteractionEvent.InteractionType.Neutralized)
            {
                if (_containsPlayer == false)
                {
                    return;
                }

                AddKillfeedItem(NEUTRALIZED_POINT_MESSAGE, NEUTRALIZED_POINT_XP_AMOUNT);
            }

            _lastPointCaptureInteraction = Time.time;
        }

        private void OnActorSpawn(OnActorSpawnEvent _event)
        {
            // When an actor considered as dead is respawning, we can check him off
            // the dead actors list, so we can handle killfeed items for it again.
            if (_deadActors.Contains(_event.actor))
            {
                _deadActors.Remove(_event.actor);
            }
        }

        private void OnVehicleSpawn(OnVehicleSpawnEvent _event)
        {
            // When a vehicle considered as dead is respawning, we can check him off
            // the dead vehicles list, so we can handle killfeed items for it again.
            if (_deadVehicles.Contains(_event.vehicle))
            {
                _deadVehicles.Remove(_event.vehicle);
            }
        }

        private void OnActorDie(OnActorDieEvent _event)
        {
            // If we already said this actor was dead, and thus treated it's killfeed item(s),
            // then we really don't need to make yet another killfeed item(s) for it.
            if (_deadActors.Contains(_event.victim))
            {
                return;
            }

            Actor _player = ActorManager.instance.player;
            Vector3 _playerPosition = _player.CenterPosition();
            Vector3 _victimPosition = _event.victim.CenterPosition();
            bool _isHeadshot = _event.damage.isCriticalHit;

            if (_event.damage.sourceActor == _player)
            {
                // This is to avoid having multiple kill messages from the same actor dying
                // over and over for some reason that I don't comprehend.
                _deadActors.Add(_event.victim);

                if (_event.victim == _player)
                {
                    AddKillfeedItem($"Suicide <#{GameConfiguration.RED_COLOR}>-1 XP</color>", -1);
                    GlobalKillfeed.instance.AddSuicideItem(_player);
                    return;
                }

                if (_event.victim.team == _player.team)
                {
                    AddKillfeedItem($"Team kill <#{GameConfiguration.RED_COLOR}>-10 XP</color>", -10);

                    GlobalKillfeed.instance.AddKillItem(_event.victim, _event.damage.sourceActor, _event.damage.sourceWeapon, _isHeadshot);
                    return;
                }

                SpawnPoint _nearestToPlayer = ActorManager.ClosestSpawnPoint(_playerPosition);
                SpawnPoint _nearestToVictim = ActorManager.ClosestSpawnPoint(_victimPosition);

                bool _isPlayerInPointProtect = Vector3.Distance(_playerPosition, _nearestToPlayer.transform.position) < _nearestToPlayer.protectRange + 10;
                bool _isPlayerInPointAttack = Vector3.Distance(_playerPosition, _nearestToPlayer.transform.position) < _nearestToPlayer.GetCaptureRange() + 10;

                bool _isVictimInPointProtect = Vector3.Distance(_victimPosition, _nearestToVictim.transform.position) < _nearestToPlayer.protectRange + 10;
                bool _isVictimInPointAttack = Vector3.Distance(_victimPosition, _nearestToVictim.transform.position) < _nearestToPlayer.GetCaptureRange() + 10;

                bool _raidConditions = _isPlayerInPointAttack && _nearestToPlayer.owner != -1 && _isVictimInPointProtect && _nearestToPlayer.owner != _player.team;
                bool _defendConditions = _isVictimInPointAttack && _nearestToVictim.owner != -1 && _nearestToVictim.owner != _event.victim.team;

                if (_raidConditions)
                {
                    AddKillfeedItem(RAID_POINT_MESSAGE, RAID_POINT_XP_AMOUNT);
                }

                if (_defendConditions)
                {
                    AddKillfeedItem(DEFEND_POINT_MESSAGE, DEFEND_POINT_XP_AMOUNT);
                }

                string _victimName = _event.victim.name;
                AddKillfeedItem($"Killed <#{GameConfiguration.RED_COLOR}>{_victimName}</color> <#{GameConfiguration.WHITE_COLOR}>3 XP</color>", 3);

                GlobalKillfeed.instance.AddKillItem(_event.victim, _event.damage.sourceActor, _event.damage.sourceWeapon, _isHeadshot);
                return;
            }

            if (_event.damage.sourceActor != null)
            {
                GlobalKillfeed.instance.AddKillItem(_event.victim, _event.damage.sourceActor, _event.damage.sourceWeapon, _isHeadshot);
            }
            else
            {
                _deadActors.Add(_event.victim);
                AddKillfeedItem($"Suicide <#{GameConfiguration.RED_COLOR}>-1 XP</color>", -1);
                GlobalKillfeed.instance.AddSuicideItem(_player);
            }
        }

        private void OnPlayerDealtDamage(OnPlayerDealtDamageEvent _event)
        {
            // If we're dealing with a vehicle...
            if (_event.hit.actor == null)
            {
                Vehicle _vehicle = _event.hit.vehicle;

                if (_vehicle == null)
                {
                    Plugin.LogError($"KillfeedManager: Actor and vehicle cannot be null at the same time!");
                    return;
                }

                if (_deadVehicles.Contains(_vehicle))
                {
                    return;
                }

                Weapon _sourceWeapon = _event.damage.sourceWeapon;

                if (_sourceWeapon == null)
                {
                    return;
                }

                // If we deal negative damage, it means we're healing this vehicle.
                if (_event.damage.healthDamage < 0)
                {
                    // TODO: Add support for repairing vehicles in kill feed.
                    return;
                }

                if (_lastHitVehicles.ContainsKey(_event.hit.vehicle) && Time.time < _lastHitVehicles[_event.hit.vehicle] + 0.01f)
                {
                    return;
                }

                bool _isKillingVehicle = (_vehicle.health - _event.damage.healthDamage) <= 0;

                float _healthPercentage = (_vehicle.health / _vehicle.maxHealth) * 100;
                float _damagedHealthPercentage = (_vehicle.health - _event.damage.healthDamage) / _vehicle.maxHealth * 100;

                Debug.Log("Health percentage: " + _healthPercentage + "; Damaged health percentage: " + _damagedHealthPercentage);

                bool[] _shouldShowDamage = new bool[]
                {
                    _damagedHealthPercentage <= 85 && _healthPercentage > 85,
                    _damagedHealthPercentage <= 70 && _healthPercentage > 70,
                    _damagedHealthPercentage <= 55 && _healthPercentage > 55,
                    _damagedHealthPercentage <= 40 && _healthPercentage > 40,
                    _damagedHealthPercentage <= 25 && _healthPercentage > 25,
                    _damagedHealthPercentage <= 10 && _healthPercentage > 10
                };

                for (int _i = 0; _i < _shouldShowDamage.Length; _i++)
                {
                    bool _part = _shouldShowDamage[_i];

                    if (!_part)
                    {
                        continue;
                    }

                    bool _isDestroyed = false;
                    string _destroyed = _vehicle.GetAdditionalData().DestroyPart(out _isDestroyed);

                    if (_isDestroyed)
                    {
                        AddKillfeedItem($"Destroyed <#{GameConfiguration.WHITE_COLOR}>{_destroyed}</color> <#{GameConfiguration.WHITE_COLOR}>1 XP</color>", 1);
                    }
                    else
                    {
                        AddKillfeedItem($"Penetrated <#{GameConfiguration.WHITE_COLOR}>Armor</color> <#{GameConfiguration.WHITE_COLOR}>1 XP</color>", 1);
                    }

                    _shouldShowDamage[_i] = false;
                    break;
                }

                if (_lastHitVehicles.ContainsKey(_event.hit.vehicle))
                {
                    _lastHitVehicles[_event.hit.vehicle] = Time.time;
                }
                else
                {
                    _lastHitVehicles.Add(_event.hit.vehicle, Time.time);
                }

                Debug.Log("Is Killing Vehicle: " + _isKillingVehicle);

                if (_isKillingVehicle)
                {
                    AddKillfeedItem($"Destroyed <#{GameConfiguration.WHITE_COLOR}>Vehicle</color> <#{GameConfiguration.WHITE_COLOR}>20 XP</color>", 20);
                    _deadVehicles.Add(_vehicle);

                    return;
                }

                return;
            }

            // If we're dealing with an actor...

            // We don't want to hit twice the same actor in the same iteration.
            if (_woundedActors.Contains(_event.hit.actor))
            {
                return;
            }

            if (_lastHitActors.ContainsKey(_event.hit.actor) && Time.time < _lastHitActors[_event.hit.actor] + 1.25f)
            {
                return;
            }

            // We don't want to get XP for injuring ourselves.
            if (_event.hit.actor == _event.damage.sourceActor)
            {
                return;
            }

            string _victimName = _event.hit.actor.name;
            AddKillfeedItem($"Wounded <#{GameConfiguration.WHITE_COLOR}>{_victimName}</color> <#{GameConfiguration.WHITE_COLOR}>2 XP</color>", 2);
            _woundedActors.Add(_event.hit.actor);

            if (_lastHitActors.ContainsKey(_event.hit.actor))
            {
                _lastHitActors[_event.hit.actor] = Time.time;
            }
            else
            {
                _lastHitActors.Add(_event.hit.actor, Time.time);
            }

            return;
        }

        private void HandleLivingKillfeedItems()
        {
            if (_killfeedItems.Count <= 0)
            {
                // If we have treated every items, we can clear the wounded actors
                // list so we allow the player to get XP for hitting an actor again.
                _woundedActors.Clear();
                return;
            }

            KillfeedItem _item = _killfeedItems[0];

            string _message = _item.message;
            int _score = _item.score;

            FpsActorController.instance.AddXP(_score);
            CreateCustomFeed(_message);

            _killfeedItems.RemoveAt(0);
        }

        private void DestroyFeed()
        {
            _toDestroy.Add(_feedItems[0]);
            _feedItems.RemoveAt(0);
        }

        public void CreateCustomFeed(string _feedContent)
        {
            try
            {
                GameObject _newItem = Instantiate(_killfeedItemPrefab, _feedItemsContainer);
                _newItem.transform.SetParent(_feedItemsContainer, false);
                _newItem.SetActive(true);
                _feedItems.Add(_newItem.GetComponent<CanvasGroup>());

                _feedContent = new System.Text.StringBuilder(_feedContent).ToString();
                _newItem.transform.Find("Content").GetComponent<TMP_Text>().text = _feedContent;
            }
            catch (Exception _exception)
            {
                Plugin.LogError("KillfeedManager: Couldn't create custom feed! " + _exception);
                return;
            }

            Invoke(nameof(DestroyFeed), 5f);
        }
    }
}