using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RiseOfWar
{
    using Events;

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
            SquadOrder
        }
        public enum VehicleActions
        {
            Damaged,
            Repaired,
            Destroyed
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
            public KillfeedQueueItem(GameActions _gameAction, SpawnPoint _victimSpawnpoint, Actor _sourceActor)
            {
                type = KillfeedItemType.Game;
                gameAction = _gameAction;
                victimSpawnpoint = _victimSpawnpoint;
                sourceActor = _sourceActor;
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

        public string WHITE_COLOR
        {
            get { return "C2BFB3"; }
        }
        public string GREEN_COLOR
        {
            get { return "95BD63"; }
        }
        public string RED_COLOR
        {
            get { return "832423"; }
        }


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

            EventManager.onPlayerDealtDamage += OnPlayerDealtDamage;
            EventManager.onActorDie += OnActorDie;

            Plugin.Log("KillfeedManager: Kill feed initialized.");
        }
        private void Update()
        {
            HandleQueuedItems();
            HandleIncrement();
            HandleVehicleInfluence();
            HandleLivingItems();
        }

        private void OnActorDie(OnActorDieEvent _event)
        {
            if (_event.damage.sourceActor == ActorManager.instance.player)
            {
                CreateActorFeed(ActorActions.Killed, _event.damage, default(HitInfo), _event.victim);
            }
        }
        private void OnPlayerDealtDamage(OnPlayerDealtDamageEvent _event)
        {
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
                        CreateVehicleFeed(VehicleActions.Damaged, _vehicle, _event.damage);
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
        private void TreatKillfeedItems()
        {
            foreach (KillfeedQueueItem _item in _lastItems)
            {
                if (_item.Equals(_queuedItems[0]))
                {
                    return;
                }
            }

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
            }

            if (string.IsNullOrEmpty(_feedMessage))
            {
                return;
            }

            CreateCustomFeed(_feedMessage);
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
        public void CreateActorFeed(ActorActions _action, DamageInfo _damageInfo, HitInfo _hitInfo, Actor _victimActor)
        {
            Actor _sourceActor = _damageInfo.sourceActor;
            _queuedItems.Add(new KillfeedQueueItem(_action, _victimActor, _damageInfo, _hitInfo, _sourceActor));
        }
        public void CreateGameFeed(GameActions _action, SpawnPoint _spawnpoint, Actor _sourceActor)
        {
            _lastItems.Clear();
            _queuedItems.Add(new KillfeedQueueItem(_action, _spawnpoint, _sourceActor));
        }
        public void CreateVehicleFeed(VehicleActions _action, Vehicle _victimVehicle, DamageInfo _damageInfo)
        {
            _lastItems.Clear();
            _queuedItems.Add(new KillfeedQueueItem(_action, _victimVehicle, _damageInfo, _damageInfo.sourceActor));
        }
    }
}