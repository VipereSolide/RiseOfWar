using System.Collections.Generic;
using System.Text;
using System;

using UnityEngine;
using TMPro;

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

        public static readonly string CAPTURED_POINT_MESSAGE = $"Captured <#{GameConfiguration.WHITE_COLOR}>67 XP</color>";
        public static readonly int CAPTURED_POINT_XP_AMOUNT = 67;

        public static readonly string NEUTRALIZED_POINT_MESSAGE = $"Neutralized <#{GameConfiguration.WHITE_COLOR}>45 XP</color>";
        public static readonly int NEUTRALIZED_POINT_XP_AMOUNT = 45;

        public static readonly string RAID_POINT_MESSAGE = $"Raid <#{GameConfiguration.WHITE_COLOR}>40 XP</color>";
        public static readonly int RAID_POINT_XP_AMOUNT = 40;

        public static readonly string DEFEND_POINT_MESSAGE = $"Defend <#{GameConfiguration.WHITE_COLOR}>65 XP</color>";
        public static readonly int DEFEND_POINT_XP_AMOUNT = 65;

        public static KillfeedManager Instance { get; set; }

        private GameObject _killfeedItemPrefab;
        private Transform _feedItemsContainer;

        private readonly List<CanvasGroup> _killfeedItemCanvasGroups = new List<CanvasGroup>();
        private readonly List<CanvasGroup> _toDestroy = new List<CanvasGroup>();
        private readonly List<KillfeedItem> _killfeedItems = new List<KillfeedItem>();

        private readonly Dictionary<Actor, float> _hitActorsRegister = new Dictionary<Actor, float>();
        private readonly List<Actor> _woundedActorsRegister = new List<Actor>();
        private readonly List<Actor> _deadActorsRegister = new List<Actor>();

        private readonly Dictionary<Vehicle, float> _hitVehiclesRegister = new Dictionary<Vehicle, float>();
        private readonly List<Vehicle> _deadVehiclesRegister = new List<Vehicle>();

        private float _lastCapturePointInteraction = 0;

        public KillfeedItem[] KillfeedItems { get { return _killfeedItems.ToArray(); } }

        public void AddKillfeedItem(string message, int score)
        {
            KillfeedItem _item = new KillfeedItem(message, score);
            AddKillfeedItem(_item);
        }
        
        public void CreateCustomKillfeedItem(string itemContent)
        {
            try
            {
                GameObject _item = Instantiate(_killfeedItemPrefab, _feedItemsContainer);
                _item.transform.SetParent(_feedItemsContainer, false);
                _item.SetActive(true);

                _killfeedItemCanvasGroups.Add(_item.GetComponent<CanvasGroup>());
                _item.transform.Find("Content").GetComponent<TMP_Text>().text = itemContent;
            }
            catch (Exception _exception)
            {
                Plugin.LogError("KillfeedManager: Could not create custom killfeed item! " + _exception);
                return;
            }

            Invoke(nameof(DestroyFeed), GameConfiguration.killfeedItemLifetime);
        }

        public void AddKillfeedItem(KillfeedItem item)
        {
            _killfeedItems.Add(item);
        }

        public void Setup(Transform container, GameObject item)
        {
            if (container == null)
            {
                Plugin.LogWarning("KillfeedManager: The provided container cannot be null! Assigning this object instead...");
                container = transform;
            }

            if (item == null)
            {
                Plugin.LogError("KillfeedManager: The provided item cannot be null!");
                return;
            }

            _killfeedItemPrefab = item;
            _feedItemsContainer = container;

            Plugin.Log("KillfeedManager: Successfully set up Killfeed Manager.");

            Start();
        }

        private void Start()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            RegisterEvents();

            Plugin.Log("KillfeedManager: Killfeed successfully started!");
        }

        private void Update()
        {
            UpdateLivingItems();
            UpdateDestroyedItems();
        }

        private void UpdateDestroyedItems()
        {
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

        private void RegisterEvents()
        {
            EventManager.onPlayerDealtDamage += OnPlayerDealtDamage;
            EventManager.onCapturePointInteraction += OnCapturePointInteraction;

            EventManager.onActorDie += OnActorDie;
            EventManager.onActorSpawn += OnActorSpawn;
            EventManager.onVehicleSpawn += OnVehicleSpawn;
        }

        private void OnCapturePointInteraction(OnCapturePointInteractionEvent _event)
        {
            bool _containsPlayer = _event.actorsOnPoint.ToList().Contains(ReferenceManager.player);

            if (Time.time < _lastCapturePointInteraction + GameConfiguration.capturePointCaptureDelay)
            {
                Plugin.LogWarning($"KillfeedManager: Cannot capture point faster than every {GameConfiguration.capturePointCaptureDelay} seconds.");
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

            _lastCapturePointInteraction = Time.time;
        }

        private void OnActorSpawn(OnActorSpawnEvent _event)
        {
            // When an actor considered as dead is respawning, we can check him off
            // the dead actors list, so we can handle killfeed items for it again.
            if (_deadActorsRegister.Contains(_event.actor))
            {
                _deadActorsRegister.Remove(_event.actor);
            }
        }

        private void OnVehicleSpawn(OnVehicleSpawnEvent _event)
        {
            // When a vehicle considered as dead is respawning, we can check him off
            // the dead vehicles list, so we can handle killfeed items for it again.
            if (_deadVehiclesRegister.Contains(_event.vehicle))
            {
                _deadVehiclesRegister.Remove(_event.vehicle);
            }
        }

        private void OnActorDie(OnActorDieEvent _event)
        {
            // If we already said this actor was dead, and thus treated it's killfeed item(s),
            // then we really don't need to make yet another killfeed item(s) for it.
            if (_deadActorsRegister.Contains(_event.victim))
            {
                return;
            }

            Actor _player = ReferenceManager.player;
            Vector3 _playerPosition = _player.CenterPosition();
            Vector3 _victimPosition = _event.victim.CenterPosition();
            bool _headshot = _event.damage.isCriticalHit;

            if (_event.damage.sourceActor == _player)
            {
                // This is to avoid having multiple kill messages from the same actor dying
                // over and over for some reason that I don't comprehend.
                _deadActorsRegister.Add(_event.victim);

                if (_event.victim == _player)
                {
                    AddKillfeedItem($"Suicide <#{GameConfiguration.RED_COLOR}>-1 XP</color>", -1);
                    GlobalKillfeed.instance.AddSuicideItem(_player);
                    return;
                }

                if (_event.victim.team == _player.team)
                {
                    AddKillfeedItem($"Team kill <#{GameConfiguration.RED_COLOR}>-10 XP</color>", -10);

                    GlobalKillfeed.instance.AddKillItem(_event.victim, _event.damage.sourceActor, _event.damage.sourceWeapon, _headshot);
                    return;
                }

                SpawnPoint _nearestToPlayer = ActorManager.ClosestSpawnPoint(_playerPosition);
                SpawnPoint _nearestToVictim = ActorManager.ClosestSpawnPoint(_victimPosition);

                // bool _isPlayerInPointProtect = Vector3.Distance(_playerPosition, _nearestToPlayer.transform.position) < _nearestToPlayer.protectRange + 10;
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

                GlobalKillfeed.instance.AddKillItem(_event.victim, _event.damage.sourceActor, _event.damage.sourceWeapon, _headshot);
                return;
            }

            if (_event.damage.sourceActor != null)
            {
                GlobalKillfeed.instance.AddKillItem(_event.victim, _event.damage.sourceActor, _event.damage.sourceWeapon, _headshot);
            }
            else
            {
                _deadActorsRegister.Add(_event.victim);
                AddKillfeedItem($"Suicide <#{GameConfiguration.RED_COLOR}>-1 XP</color>", -1);
                GlobalKillfeed.instance.AddSuicideItem(_player);
            }
        }

        private void DealDamageToVehicle(OnPlayerDealtDamageEvent _event, Vehicle vehicle)
        {
            if (_deadVehiclesRegister.Contains(vehicle))
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

            if (_hitVehiclesRegister.ContainsKey(_event.hit.vehicle) && Time.time < _hitVehiclesRegister[_event.hit.vehicle] + 0.01f)
            {
                return;
            }

            bool _isKillingVehicle = (vehicle.health - _event.damage.healthDamage) <= 0;

            float _healthPercentage = (vehicle.health / vehicle.maxHealth) * 100;
            float _damagedHealthPercentage = (vehicle.health - _event.damage.healthDamage) / vehicle.maxHealth * 100;

            Plugin.Log($"KillfeedManager: Health percentage {_healthPercentage}. Damaged health percentage {_damagedHealthPercentage}.");

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
                string _destroyed = vehicle.GetAdditionalData().DestroyPart(out _isDestroyed);

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

            if (_hitVehiclesRegister.ContainsKey(_event.hit.vehicle))
            {
                _hitVehiclesRegister[_event.hit.vehicle] = Time.time;
            }
            else
            {
                _hitVehiclesRegister.Add(_event.hit.vehicle, Time.time);
            }

            Plugin.Log($"KillfeedManager: Is killing vehicle = {_isKillingVehicle}");

            if (_isKillingVehicle)
            {
                AddKillfeedItem($"Destroyed <#{GameConfiguration.WHITE_COLOR}>Vehicle</color> <#{GameConfiguration.WHITE_COLOR}>20 XP</color>", 20);
                _deadVehiclesRegister.Add(vehicle);

                return;
            }
        }

        private void DealDamageToActor(OnPlayerDealtDamageEvent _event)
        {
            // We don't want to hit twice the same actor in the same iteration.
            if (_woundedActorsRegister.Contains(_event.hit.actor))
            {
                return;
            }

            if (_hitActorsRegister.ContainsKey(_event.hit.actor) && Time.time < _hitActorsRegister[_event.hit.actor] + 1.25f)
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
            
            if (_hitActorsRegister.ContainsKey(_event.hit.actor))
            {
                _hitActorsRegister[_event.hit.actor] = Time.time;
            }
            else
            {
                _hitActorsRegister.Add(_event.hit.actor, Time.time);
            }
         
            _woundedActorsRegister.Add(_event.hit.actor);
        }

        private void OnPlayerDealtDamage(OnPlayerDealtDamageEvent _event)
        {
            // If we're dealing with a vehicle...
            if (_event.hit.actor == null)
            {
                Vehicle _vehicle = _event.hit.vehicle;

                if (_vehicle == null)
                {
                    Plugin.LogWarning($"KillfeedManager: Cannot deal damage to neither actor nor vehicles at the same time.");
                    return;
                }

                DealDamageToVehicle(_event, _vehicle);
                return;
            }

            // If we're dealing with an actor...
            DealDamageToActor(_event);

            return;
        }

        private void UpdateLivingItems()
        {
            if (_killfeedItems.Count <= 0)
            {
                // If we have treated every items, we can clear the wounded actors
                // list so we allow the player to get XP for hitting an actor again.
                _woundedActorsRegister.Clear();
                return;
            }

            KillfeedItem _item = _killfeedItems[0];

            string _message = _item.message;
            int _score = _item.score;

            ReferenceManager.player.AddScore(_score);
            CreateCustomKillfeedItem(_message);

            _killfeedItems.RemoveAt(0);
        }

        private void DestroyFeed()
        {
            _toDestroy.Add(_killfeedItemCanvasGroups[0]);
            _killfeedItemCanvasGroups.RemoveAt(0);
        }
    }
}