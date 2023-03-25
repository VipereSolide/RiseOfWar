using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RiseOfWar
{
    using Events;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class KillfeedManager : MonoBehaviour
    {
        public enum KillfeedItemType
        {
            Actor,
            Vehicle,
            Game
        }
        public enum ActorActions
        {
            Killed,
            Wounded
        }
        public enum GameActions
        {
            Neutralized,
            Captured,
            SquadOrder,
            Protect,
            Raid,
        }
        public enum VehicleActions
        {
            Damaged,
            Repaired,
            Destroyed,
            DestroyedTransmission,
            DestroyedEngine,
            DestroyedCockpit,
        }

        [Serializable]
        public class KillfeedQueueItem
        {
            public KillfeedItemType type;
            public ActorActions actorAction;
            public VehicleActions vehicleAction;
            public GameActions gameAction;

            public Actor victimActor;
            public Vehicle victimVehicle;
            public SpawnPoint victimSpawnpoint;
            public DamageInfo damageInfo;
            public HitInfo hitInfo;
            public Actor sourceActor;

            public KillfeedQueueItem(ActorActions _actorAction, Actor _victim, DamageInfo _damageInfo, HitInfo _hitInfo, Actor _sourceActor)
            {
                type = KillfeedItemType.Actor;
                actorAction = _actorAction;
                victimActor = _victim;
                damageInfo = _damageInfo;
                hitInfo = _hitInfo;
                sourceActor = _sourceActor;
            }
            public KillfeedQueueItem(VehicleActions _vehicleAction, Vehicle _victimVehicle, DamageInfo _damageInfo, Actor _sourceActor)
            {
                type = KillfeedItemType.Vehicle;
                vehicleAction = _vehicleAction;
                victimVehicle = _victimVehicle;
                damageInfo = _damageInfo;
                sourceActor = _sourceActor;
            }
            public KillfeedQueueItem(GameActions _gameAction, SpawnPoint _victimSpawnpoint)
            {
                type = KillfeedItemType.Game;
                gameAction = _gameAction;
                victimSpawnpoint = _victimSpawnpoint;
            }

            public KillfeedQueueItem()
            {

            }

            public override bool Equals(object _obj)
            {
                if (_obj is KillfeedQueueItem)
                {
                    KillfeedQueueItem _compared = (KillfeedQueueItem)_obj;

                    if (_compared.type != this.type)
                    {
                        return false;
                    }

                    switch (_compared.type)
                    {
                        case KillfeedItemType.Game:
                            return _compared.gameAction == gameAction && _compared.victimSpawnpoint == victimSpawnpoint && _compared.sourceActor == sourceActor;

                        case KillfeedItemType.Vehicle:
                            return _compared.victimVehicle == victimVehicle && _compared.vehicleAction == vehicleAction && _compared.sourceActor == sourceActor;

                        case KillfeedItemType.Actor:
                            return _compared.actorAction == actorAction && _compared.victimActor == victimActor && _compared.sourceActor == sourceActor;
                    }
                }

                return base.Equals(_obj);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

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

        public static readonly string WHITE_COLOR = "C2BFB3";
        public static readonly string GREEN_COLOR = "95BD63";
        public static readonly string RED_COLOR = "832423";

        public static KillfeedManager Instance { get; set; }

        [SerializeField]
        private GameObject _killfeedItemPrefab;

        [SerializeField]
        private Transform _feedItemsContainer;

        private List<KillfeedQueueItem> _queuedItems = new List<KillfeedQueueItem>();
        private List<CanvasGroup> _feedItems = new List<CanvasGroup>();
        private List<CanvasGroup> _toDestroy = new List<CanvasGroup>();
        private List<KillfeedQueueItem> _lastItems = new List<KillfeedQueueItem>();
        private float _vehicleInfluence;
        private float _increment;

        private List<KillfeedItem> _killfeedItems = new List<KillfeedItem>();
        public KillfeedItem[] killfeedItems { get { return _killfeedItems.ToArray(); } }

        public static readonly string CAPTURED_POINT_MESSAGE = $"Captured <#{WHITE_COLOR}>67 XP</color>";
        public static readonly int CAPTURED_POINT_XP_AMOUNT = 67;

        public static readonly string NEUTRALIZED_POINT_MESSAGE = $"Neutralized <#{WHITE_COLOR}>45 XP</color>";
        public static readonly int NEUTRALIZED_POINT_XP_AMOUNT = 45;

        public static readonly string RAID_POINT_MESSAGE = $"Raid <#{WHITE_COLOR}>40 XP</color>";
        public static readonly int RAID_POINT_XP_AMOUNT = 40;

        public static readonly string DEFEND_POINT_MESSAGE = $"Defend <#{WHITE_COLOR}>65 XP</color>";
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

            HandleListeners();

            Plugin.Log("KillfeedManager: Killfeed successfully initialized!");
        }

        private void Update()
        {
            TreatKillfeedItems();
            HandleIncrement();
            HandleVehicleInfluence();
            HandleLivingItems();
        }

        private void HandleListeners()
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

            if (_event.damage.sourceActor == _player)
            {
                // This is to avoid having multiple kill messages from the same actor dying
                // over and over for some reason that I don't comprehend.
                _deadActors.Add(_event.victim);

                if (_event.victim == _player)
                {
                    AddKillfeedItem($"Suicide <#{RED_COLOR}>-1 XP</color>", -1);
                    return;
                }

                if (_event.victim.team == _player.team)
                {
                    AddKillfeedItem($"Team kill <#{RED_COLOR}>-10 XP</color>", -10);
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
                AddKillfeedItem($"Killed <#{RED_COLOR}>{_victimName}</color> <#{WHITE_COLOR}>3 XP</color>", 3);
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

                    if (_isDestroyed) {
                        AddKillfeedItem($"Destroyed <#{WHITE_COLOR}>{_destroyed}</color> <#{WHITE_COLOR}>1 XP</color>", 1);
                    }
                    else
                    {
                        AddKillfeedItem($"Penetrated <#{WHITE_COLOR}>Armor</color> <#{WHITE_COLOR}>1 XP</color>", 1);
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
                    AddKillfeedItem($"Destroyed <#{WHITE_COLOR}>Vehicle</color> <#{WHITE_COLOR}>20 XP</color>", 20);
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
            AddKillfeedItem($"Wounded <#{WHITE_COLOR}>{_victimName}</color> <#{WHITE_COLOR}>2 XP</color>", 2);
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

            /*
            if (_event.hit.actor == null)
            {
                Vehicle _vehicle = _event.hit.vehicle;
                if (_event.hit.vehicle != null)
                {
                    if (_event.damage.healthDamage < 0)
                    {
                        CreateVehicleFeed(VehicleActions.Repaired, _vehicle, _event.damage);
                        return;
                    }

                    Vehicle.ArmorRating _armorRating = _event.damage.sourceWeapon.configuration.projectile().armorDamage;
                    bool _isDamaged = _vehicle.IsDamagedByRating(_armorRating);

                    if (!_isDamaged)
                    {
                        return;
                    }

                    float _armorDamage = _vehicle.GetDamageMultiplier(_armorRating) * _event.damage.healthDamage;

                    if (_vehicle.health - _armorDamage <= 0)
                    {
                        CreateVehicleFeed(VehicleActions.Destroyed, _vehicle, _event.damage);
                    }
                    else
                    {
                        // CreateVehicleFeed(VehicleActions.Damaged, _vehicle, _event.damage);

                        float _healthNow = _vehicle.health / _vehicle.maxHealth;
                        float _healthBefore = (_vehicle.health + _event.damage.healthDamage / _vehicle.GetDamageMultiplier(_armorRating)) / _vehicle.maxHealth;

                        if (_healthNow < 0.7 && _healthBefore >= 0.7)
                        {
                            CreateVehicleFeed(VehicleActions.DestroyedTransmission, _vehicle, _event.damage);
                        }

                        if (_healthNow < 0.4 && _healthBefore >= 0.4)
                        {
                            CreateVehicleFeed(VehicleActions.DestroyedEngine, _vehicle, _event.damage);
                        }

                        if (_healthNow < 0.1 && _healthBefore >= 0.1)
                        {
                            CreateVehicleFeed(VehicleActions.DestroyedCockpit, _vehicle, _event.damage);
                        }
                    }
                }
            }
            else
            {
                if (_event.hit.actor == _event.damage.sourceActor)
                {
                    return;
                }

                CreateActorFeed(ActorActions.Wounded, _event.damage, _event.hit, _event.hit.actor);
            }
        
           */
        }

        private void HandleQueuedItems()
        {
            if (_queuedItems.Count > 0)
            {
                TreatKillfeedItems();
                _lastItems.Add(_queuedItems[0]);
                _queuedItems.RemoveAt(0);
            }
        }

        private void HandleIncrement()
        {
            _increment += Time.deltaTime;

            if (_increment > GameConfiguration.killfeedClearInterval)
            {
                _lastItems.Clear();
                _increment = 0;
            }
        }

        private void HandleVehicleInfluence()
        {
            if (_vehicleInfluence > 0f)
            {
                _vehicleInfluence -= Time.deltaTime;
            }

            _vehicleInfluence = Mathf.Clamp(_vehicleInfluence, 0f, 100f);
        }

        private void HandleLivingItems()
        {
            try
            {
                foreach (CanvasGroup canvasGroup in _toDestroy)
                {
                    canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, Time.deltaTime * 2f);
                    if (canvasGroup.alpha <= 0.05f)
                    {
                        _toDestroy.Remove(canvasGroup);
                        Destroy(canvasGroup.gameObject);
                    }
                }
            }
            catch { }
        }

        private void TreatKillfeedItems()
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

        /*
        private void TreatKillfeedItems()
        {
            foreach (KillfeedQueueItem _item in _lastItems)
            {
                if (_item.Equals(_queuedItems[0]))
                {
                    return;
                }
            }

            Actor _player = FpsActorController.instance.actor;

            int _xp = 0;
            string _feedMessage = string.Empty;

            if (_queuedItems[0].type == KillfeedItemType.Actor)
            {
                if (_queuedItems[0].actorAction == ActorActions.Wounded)
                {
                    if (_queuedItems[0].victimActor.team != _queuedItems[0].sourceActor.team)
                    {
                        _xp = 2;
                        _feedMessage = $"Wounded <#{WHITE_COLOR}>{_queuedItems[0].victimActor.name}</color> <#{WHITE_COLOR}>{_xp} XP</color>";
                    }
                }
                else
                {
                    if (_queuedItems[0].victimActor.team == _queuedItems[0].sourceActor.team)
                    {
                        if (_queuedItems[0].victimActor == _queuedItems[0].sourceActor)
                        {
                            _xp = -1;
                            _feedMessage = $"Accident <#{RED_COLOR}>{_xp} XP</color>";
                        }
                        else
                        {
                            _xp = -10;
                            _feedMessage = $"Team kill <#{RED_COLOR}>{_xp} XP</color>";
                        }
                    }
                    else
                    {
                        _xp = 3;
                        _feedMessage = $"Killed <#{RED_COLOR}>{_queuedItems[0].victimActor.name}</color> <#{WHITE_COLOR}>{_xp} XP</color>";

                        SpawnPoint _point = ActorManager.ClosestSpawnPoint(_player.CenterPosition());
                        bool _continue = true;

                        if (_point.owner == _player.team)
                        {
                            if (Vector3.Distance(_player.CenterPosition(), _point.transform.position) < _point.protectRange + 10 && _point.IsContested(_player.team))
                            {
                                CreateGameFeed(GameActions.Protect, _point);
                                _continue = false;
                            }
                        }
                        else
                        {
                            if (Vector3.Distance(_player.CenterPosition(), _point.transform.position) < _point.GetCaptureRange() + 10 && _point.IsContested(_player.team == 0 ? 1 : 0))
                            {
                                CreateGameFeed(GameActions.Raid, _point);
                                _continue = false;
                            }
                        }

                        if (_continue)
                        {
                            SpawnPoint _point2 = ActorManager.ClosestSpawnPoint(_queuedItems[0].victimActor.CenterPosition());

                            if (Vector3.Distance(_queuedItems[0].victimActor.CenterPosition(), _point2.transform.position) < _point2.GetCaptureRange() && _point2.owner != _queuedItems[0].victimActor.team)
                            {
                                if (Vector3.Distance(_player.CenterPosition(), _point.transform.position) < _point.protectRange + 10 && _point.IsContested(_player.team))
                                {
                                    CreateGameFeed(GameActions.Protect, _point);
                                    _continue = false;
                                }
                            }
                        }
                    }
                }
            }
            else if (_queuedItems[0].type == KillfeedItemType.Vehicle)
            {
                if (_queuedItems[0].vehicleAction == VehicleActions.Damaged)
                {
                    _xp = 2;
                    _feedMessage = $"Damaged <#{WHITE_COLOR}>Vehicle</color> <#{WHITE_COLOR}>{_xp} XP</color>";
                }
                else if (_queuedItems[0].vehicleAction == VehicleActions.DestroyedCockpit)
                {
                    _xp = 2;
                    _feedMessage = $"Destroyed <#{WHITE_COLOR}>Cockpit</color> <#{WHITE_COLOR}>{_xp} XP</color>";
                }
                else if (_queuedItems[0].vehicleAction == VehicleActions.DestroyedTransmission)
                {
                    _xp = 2;
                    _feedMessage = $"Destroyed <#{WHITE_COLOR}>Transmission</color> <#{WHITE_COLOR}>{_xp} XP</color>";
                }
                else if (_queuedItems[0].vehicleAction == VehicleActions.DestroyedEngine)
                {
                    _xp = 2;
                    _feedMessage = $"Destroyed <#{WHITE_COLOR}>Repaired</color> <#{WHITE_COLOR}>{_xp} XP</color>";
                }
                else if (_queuedItems[0].vehicleAction == VehicleActions.Repaired)
                {
                    _xp = 1;
                    _feedMessage = $"Repaired <#{WHITE_COLOR}>Vehicle</color> <#{WHITE_COLOR}>{_xp} XP</color>";
                }
                else if (_queuedItems[0].vehicleAction == VehicleActions.Destroyed)
                {
                    _xp = 20;
                    _feedMessage = $"Destroyed <#{WHITE_COLOR}>Vehicle</color> <#{WHITE_COLOR}>{_xp} XP</color>";
                }
            }
            else if (_queuedItems[0].type == KillfeedItemType.Game)
            {
                if (_queuedItems[0].gameAction == GameActions.Captured)
                {
                    _xp = 67;
                    _feedMessage = $"Captured <#{WHITE_COLOR}>{_xp} XP</color>";
                }
                else if (_queuedItems[0].gameAction == GameActions.Neutralized)
                {
                    _xp = 45;
                    _feedMessage = $"Neutralized <#{WHITE_COLOR}>{_xp} XP</color>";
                }
                else if (_queuedItems[0].gameAction == GameActions.SquadOrder)
                {
                    _xp = 5;
                    _feedMessage = $"Squad <#{GREEN_COLOR}>Order</color> <#{WHITE_COLOR}>{_xp} XP</color>";
                }
                else if (_queuedItems[0].gameAction == GameActions.Protect)
                {
                    _xp = 60;
                    _feedMessage = $"Defend <#{WHITE_COLOR}>{_xp} XP</color>";
                }
                else if (_queuedItems[0].gameAction == GameActions.Raid)
                {
                    _xp = 40;
                    _feedMessage = $"Raid <#{WHITE_COLOR}>{_xp} XP</color>";
                }
            }

            if (string.IsNullOrEmpty(_feedMessage))
            {
                return;
            }

            FpsActorController.instance.AddXP(_xp);
            CreateCustomFeed(_feedMessage);
        }
        */
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

        public void CreateActorFeed(ActorActions _action, DamageInfo _damageInfo, HitInfo _hitInfo, Actor _victimActor)
        {
            Actor _sourceActor = _damageInfo.sourceActor;
            _queuedItems.Add(new KillfeedQueueItem(_action, _victimActor, _damageInfo, _hitInfo, _sourceActor));
        }

        public void CreateGameFeed(GameActions _action, SpawnPoint _spawnpoint)
        {
            _lastItems.Clear();
            _queuedItems.Add(new KillfeedQueueItem(_action, _spawnpoint));
        }

        public void CreateVehicleFeed(VehicleActions _action, Vehicle _victimVehicle, DamageInfo _damageInfo)
        {
            _lastItems.Clear();
            _queuedItems.Add(new KillfeedQueueItem(_action, _victimVehicle, _damageInfo, _damageInfo.sourceActor));
        }
    }
}