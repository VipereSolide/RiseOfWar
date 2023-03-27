using System.Collections.Generic;
using System.Collections;
using System;

using UnityEngine.UI;
using UnityEngine;
using HarmonyLib;
using TMPro;

namespace RiseOfWar
{
    public class GlobalKillfeed : MonoBehaviour
    {
        public static GlobalKillfeed instance { get; private set; }

        protected Transform _itemContainer;
        protected TMP_Text _itemPrefab;

        protected List<TMP_Text> _items = new List<TMP_Text>();
        protected float _itemCooldown;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            UpdateItemCooldown();
        }

        public void Setup(TMP_Text itemPrefab)
        {
            _itemContainer = transform.Find("Canvas/Killfeed");
            this._itemPrefab = itemPrefab;
        }

        private void UpdateItemCooldown()
        {
            _itemCooldown += Time.deltaTime;

            if (_itemCooldown > GameConfiguration.globalKillfeedItemLifetime)
            {
                DestroyLatestKillfeedItem();
                _itemCooldown = 0;
            }
        }

        private void DestroyLatestKillfeedItem()
        {
            StartCoroutine(FadeText(_items[_items.Count - 1], 1.5f, true));
        }

        private IEnumerator FadeText(TMP_Text text, float fadeTime, bool destroyOnEnd)
        {
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

        public void AddItem(string itemName)
        {
            TMP_Text _item = Instantiate(_itemPrefab, _itemContainer);
            _item.text = itemName;

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_itemContainer.parent);

            _items.Add(_item);
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

        public void AddKillItem(Actor killed, Actor killer, Weapon source, bool isHeadshot)
        {
            string _killedTeam = (killed.team == 0) ? GameConfiguration.BLUE_COLOR : GameConfiguration.RED_COLOR;
            string _killerTeam = (killer.team == 0) ? GameConfiguration.BLUE_COLOR : GameConfiguration.RED_COLOR;
            string _weapon = (isHeadshot) ? "Headshot" : WeaponRegistry.GetRealName(source.name);

            AddItem($"<#{_killerTeam}>{killer.name}</color> <#{GameConfiguration.WHITE_COLOR}>[{_weapon}]</color> <#{_killedTeam}>{killed.name}</color>");
        }
    }
}