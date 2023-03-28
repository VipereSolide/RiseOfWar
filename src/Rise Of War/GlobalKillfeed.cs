using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiseOfWar
{
    public class GlobalKillfeed : MonoBehaviour
    {
        public static GlobalKillfeed instance { get; private set; }

        protected Transform _itemContainer;
        protected TMP_Text _itemPrefab;

        protected List<TMP_Text> _items = new List<TMP_Text>();
        protected List<TMP_Text> _toDestroy = new List<TMP_Text>();

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (_toDestroy.Count == 0)
            {
                return;
            }

            StartCoroutine(FadeText(_toDestroy[0], 1.5f, true));
            _toDestroy.RemoveAt(0);
        }

        public void Setup(TMP_Text itemPrefab)
        {
            _itemContainer = transform.Find("Canvas/Killfeed");
            _itemPrefab = itemPrefab;
        }

        private IEnumerator FadeText(TMP_Text text, float fadeTime, bool destroyOnEnd)
        {
            if (text == null)
            {
                yield break;
            }

            Color _original = text.color;
            Color _color = text.color;
            float _cooldown = fadeTime;

            while (_cooldown > 0)
            {
                _cooldown -= Time.deltaTime;
                _color.a = _cooldown / fadeTime;
                text.color = _color;

                yield return new WaitForEndOfFrame();
            }

            if (destroyOnEnd)
            {
                Destroy(text.gameObject);
                yield break;
            }

            text.gameObject.SetActive(false);
            text.color = _original;
        }

        private bool IsInKillfeed(string text)
        {
            foreach (TMP_Text _text in _items)
            {
                if (_text.text == text)
                {
                    return true;
                }
            }

            return false;
        }

        public void AddItem(string itemName)
        {
            if (IsInKillfeed(itemName))
            {
                return;
            }

            TMP_Text _item = Instantiate(_itemPrefab, _itemContainer);
            _item.text = itemName;

            _items.Add(_item);
            Invoke("AddKillfeedItemToDestroy", 5);
        }

        private void AddKillfeedItemToDestroy()
        {
            _toDestroy.Add(_items[0]);
            _items.RemoveAt(0);
        }

        public void AddSuicideItem(Actor player)
        {
            string _playerColor = GameConfiguration.BLUE_COLOR;
            Actor _localPlayer = FpsActorController.instance.actor;

            if (player.team != _localPlayer.team)
            {
                _playerColor = GameConfiguration.RED_COLOR;
            }

            if (player == _localPlayer)
            {
                _playerColor = GameConfiguration.GREEN_COLOR;
            }

            AddItem($"<#{_playerColor}>{player.name}</color> <#{GameConfiguration.WHITE_COLOR}>[Suicide]</color> <#{_playerColor}>{player.name}</color>");
        }

        public void AddKillItem(Actor killed, Actor killer, Weapon source, bool isHeadshot, bool isVictimInSquad = false, bool isKillerInSquad = false)
        {
            string _killedTeam = (killed.team == 0) ? GameConfiguration.BLUE_COLOR : GameConfiguration.RED_COLOR;
            string _killerTeam = (killer.team == 0) ? GameConfiguration.BLUE_COLOR : GameConfiguration.RED_COLOR;
            string _weapon = (isHeadshot) ? "Headshot" : WeaponRegistry.GetRealName(source.name);

            if (isVictimInSquad) _killedTeam = GameConfiguration.GREEN_COLOR;
            if (isKillerInSquad) _killerTeam = GameConfiguration.GREEN_COLOR;

            AddItem($"<#{_killerTeam}>{killer.name}</color> <#{GameConfiguration.WHITE_COLOR}>[{_weapon}]</color> <#{_killedTeam}>{killed.name}</color>");
        }

        public void AddKillItem(Actor killed, Actor killer, Weapon source, bool isHeadshot)
        {
            string _killedTeam = (killed.team == 0) ? GameConfiguration.BLUE_COLOR : GameConfiguration.RED_COLOR;
            string _killerTeam = (killer.team == 0) ? GameConfiguration.BLUE_COLOR : GameConfiguration.RED_COLOR;
            string _weapon = (isHeadshot) ? "Headshot" : WeaponRegistry.GetRealName(source.name);

            Actor _player = FpsActorController.instance.actor;

            if (killed == _player || _player.controller.GetSquad().members.Contains(killed.controller))
            {
                _killedTeam = GameConfiguration.GREEN_COLOR;
            }

            if (killer == _player || _player.controller.GetSquad().members.Contains(killer.controller))
            {
                _killerTeam = GameConfiguration.GREEN_COLOR;
            }

            AddItem($"<#{_killerTeam}>{killer.name}</color> <#{GameConfiguration.WHITE_COLOR}>[{_weapon}]</color> <#{_killedTeam}>{killed.name}</color>");
        }
    }
}