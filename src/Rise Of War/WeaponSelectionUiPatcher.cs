using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RiseOfWar
{
    public class WeaponSelectionUiPatcher
    {
        private static bool ShouldBeRemoved(WeaponManager.WeaponEntry entry)
        {
            bool _doesPlayerHaveAllWeapons = (GameManager.PlayerTeam() != -1 && !GameManager.GameParameters().playerHasAllWeapons && !GameManager.instance.gameInfo.team[GameManager.PlayerTeam()].IsWeaponEntryAvailable(entry));
            bool _isCustomWeapon = GameManager.PlayerTeam() != -1 && WeaponRegistry.IsCustomWeapon(entry);

            if (GameConfiguration.isDebugModeEnabled)
            {
                return false;
            }

            return _doesPlayerHaveAllWeapons || !_isCustomWeapon;
        }

        [HarmonyPatch(typeof(WeaponSelectionUi), "SetupTagGroups")]
        [HarmonyPrefix]
        private static bool SetupTagGroupsPatch(WeaponSelectionUi __instance)
        {
            List<string> _registeredWeapons;
            Dictionary<string, List<WeaponManager.WeaponEntry>> _taggedWeapons = WeaponManager.GetWeaponTagDictionary(__instance.slot, __instance.defaultTags, out _registeredWeapons, true);

            __instance.SetProperty("selectionHighlighter", new Dictionary<int, RawImage>(_registeredWeapons.Count));
            __instance.SetProperty("tagText", new Dictionary<int, Text>(_registeredWeapons.Count));
            __instance.GetProperty<RectTransform>("tagButtonPanel").SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 20f + 340f * _registeredWeapons.Count);
            __instance.SetProperty("tagGroupContainer", new List<GameObject>());

            int num = Mathf.Max(1, Mathf.FloorToInt(Screen.width / 410f));
            float num2 = Screen.width / num;
            __instance.SetProperty("tagIndexOfEntry", new Dictionary<WeaponManager.WeaponEntry, int>());
            bool flag = false;

            foreach (string key in _registeredWeapons)
            {
                _taggedWeapons[key].RemoveAll(ShouldBeRemoved);

                if (_taggedWeapons[key].Count > 0)
                {
                    _taggedWeapons[key].Insert(0, null);
                    flag = true;
                }
            }

            if (!flag)
            {
                _taggedWeapons[_registeredWeapons[0]].Add(null);
            }

            _registeredWeapons.RemoveAll((string tag) => _taggedWeapons[tag].Count == 0);
            for (int i = 0; i < _registeredWeapons.Count; i++)
            {
                Button _instantiatedWeapon = GameObject.Instantiate(__instance.tagGroupButtonPrefab).GetComponent<Button>();
                RectTransform _instantiatedWeaponRectTransform = (RectTransform)_instantiatedWeapon.transform;
                _instantiatedWeaponRectTransform.SetParent(__instance.tagButtonPanel, false);
                _instantiatedWeaponRectTransform.anchoredPosition = new Vector2(20f + 340f * i, 0f);

                int _index = i;
                _instantiatedWeapon.onClick.AddListener(() => __instance.OpenTag(_index));

                _instantiatedWeapon.GetComponentInChildren<Text>().text = WeaponRegistry.GetRealName(_registeredWeapons[i]);
                __instance.GetProperty<Dictionary<int, RawImage>>("selectionHighlighter").Add(i, _instantiatedWeapon.GetComponentInChildren<RawImage>());
                __instance.GetProperty<Dictionary<int, Text>>("tagText").Add(i, _instantiatedWeapon.GetComponentInChildren<Text>());
                __instance.GetProperty<Dictionary<int, Text>>("tagText")[i].color = Color.gray;

                RectTransform _instantiatedTagGroupContainer = (RectTransform)GameObject.Instantiate(__instance.tagGroupContainerPrefab).transform;
                _instantiatedTagGroupContainer.SetParent(__instance.weaponEntryPanel, false);
                __instance.GetProperty<List<GameObject>>("tagGroupContainer").Add(_instantiatedTagGroupContainer.gameObject);
                RectTransform _weaponEntryContainer = (RectTransform)_instantiatedTagGroupContainer.GetChild(0).GetChild(0);

                if (_taggedWeapons.ContainsKey(_registeredWeapons[i]))
                {
                    int _weaponEntryLine = 0;
                    int _weaponEntryCount = 0;
                    float _height = 0f;
                    ModInformation modInformation = ModInformation.OfficialContent;

                    using (List<WeaponManager.WeaponEntry>.Enumerator enumerator2 = _taggedWeapons[_registeredWeapons[i]].GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            WeaponManager.WeaponEntry _weaponEntry = enumerator2.Current;

                            if (_weaponEntry != null && _weaponEntry.sourceMod != modInformation)
                            {
                                modInformation = _weaponEntry.sourceMod;

                                if (_weaponEntryLine > 0)
                                {
                                    _weaponEntryLine = 0;
                                    _weaponEntryCount++;
                                }

                                RectTransform _instantiatedWeaponEntry = (RectTransform)GameObject.Instantiate(__instance.weaponEntryGroupPrefab).transform;
                                _instantiatedWeaponEntry.SetParent(_weaponEntryContainer, false);
                                _instantiatedWeaponEntry.anchoredPosition = new Vector2((_weaponEntryLine + 0.5f) * num2, -20f - _weaponEntryCount * 190f - _height);

                                Text _text = _instantiatedWeaponEntry.GetComponentInChildren<Text>();
                                _text.text = modInformation.title;

                                _height += 40f;
                            }

                            Button _weaponEntryButton = GameObject.Instantiate(__instance.weaponEntrySmallPrefab).GetComponent<Button>();
                            // Button _weaponEntryButton = GameObject.Instantiate((_weaponEntry == null || _weaponEntry.slot == WeaponManager.WeaponSlot.Primary || _weaponEntry.slot == WeaponManager.WeaponSlot.LargeGear) ? __instance.weaponEntryPrefab : __instance.weaponEntrySmallPrefab).GetComponent<Button>();
                            _weaponEntryButton.onClick.AddListener(delegate ()
                            {
                                LoadoutUi.instance.WeaponSelected(_weaponEntry);
                            });

                            RectTransform _entryRectTransform = (RectTransform)_weaponEntryButton.transform;
                            _entryRectTransform.SetParent(_weaponEntryContainer, false);

                            if (_weaponEntry != null)
                            {
                                _entryRectTransform.GetComponentInChildren<Image>().sprite = _weaponEntry.uiSprite;
                                Text _text = _entryRectTransform.GetComponentInChildren<Text>();

                                if (WeaponRegistry.IsCustomWeapon(_weaponEntry) == false)
                                {
                                    _text.color = Color.red;
                                    _text.text = _weaponEntry.name;
                                }
                                else
                                {
                                    _text.text = WeaponRegistry.GetRealName(_weaponEntry);
                                }
                            }
                            else
                            {
                                _entryRectTransform.GetComponentInChildren<Image>().sprite = LoadoutUi.instance.nothingSprite;
                                _entryRectTransform.GetComponentInChildren<Text>().text = "";
                            }

                            _entryRectTransform.anchoredPosition = new Vector2((_weaponEntryLine + 0.5f) * num2, -20f - _weaponEntryCount * 190f - _height);
                            if (_weaponEntry != null && !__instance.GetProperty<Dictionary<WeaponManager.WeaponEntry, int>>("tagIndexOfEntry").ContainsKey(_weaponEntry))
                            {
                                __instance.GetProperty<Dictionary<WeaponManager.WeaponEntry, int>>("tagIndexOfEntry").Add(_weaponEntry, i);
                            }

                            _weaponEntryLine++;
                            if (_weaponEntryLine >= num)
                            {
                                _weaponEntryLine = 0;
                                _weaponEntryCount++;
                            }
                        }
                    }

                    if (_weaponEntryLine > 0)
                    {
                        _weaponEntryCount++;
                    }

                    _weaponEntryContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 60f + _weaponEntryCount * 190f + _height);
                }

                _instantiatedTagGroupContainer.gameObject.SetActive(false);
            }

            return false;
        }
    }
}