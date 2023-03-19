using System.Collections.Generic;
using System;

using UnityEngine.UI;
using UnityEngine;
using HarmonyLib;

namespace RiseOfWar
{
    public class WeaponSelectionUiPatcher
    {
        private static bool ShouldBeRemoved(WeaponManager.WeaponEntry entry)
        {
            bool _doesPlayerHaveAllWeapons = (GameManager.PlayerTeam() != -1 && !GameManager.GameParameters().playerHasAllWeapons && !GameManager.instance.gameInfo.team[GameManager.PlayerTeam()].IsWeaponEntryAvailable(entry));
            bool _isCustomWeapon = (GameManager.PlayerTeam() != -1 && WeaponRegistry.IsCustomWeapon(entry) == false);
            return _doesPlayerHaveAllWeapons || _isCustomWeapon;
        }

        [HarmonyPatch(typeof(WeaponSelectionUi), "SetupTagGroups")]
        [HarmonyPrefix]
        private static bool SetupTagGroupsPatch(WeaponSelectionUi __instance)
        {
            List<string> list;
            Dictionary<string, List<WeaponManager.WeaponEntry>> _taggedWeapons = WeaponManager.GetWeaponTagDictionary(__instance.slot, __instance.defaultTags, out list, false);

            __instance.SetProperty("selectionHighlighter", new Dictionary<int, RawImage>(list.Count));
            __instance.SetProperty("tagText", new Dictionary<int, Text>(list.Count));
            __instance.GetProperty<RectTransform>("tagButtonPanel").SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 20f + 340f * (float)list.Count);
            __instance.SetProperty("tagGroupContainer", new List<GameObject>());

            int num = Mathf.Max(1, Mathf.FloorToInt((float)Screen.width / 410f));
            float num2 = (float)(Screen.width / num);
            __instance.SetProperty("tagIndexOfEntry", new Dictionary<WeaponManager.WeaponEntry, int>());
            bool flag = false;

            foreach (string key in list)
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
                _taggedWeapons[list[0]].Add(null);
            }

            list.RemoveAll((string tag) => _taggedWeapons[tag].Count == 0);
            for (int i = 0; i < list.Count; i++)
            {
                Button component = UnityEngine.Object.Instantiate<GameObject>(__instance.tagGroupButtonPrefab).GetComponent<Button>();
                RectTransform rectTransform = (RectTransform)component.transform;
                rectTransform.SetParent(__instance.tagButtonPanel, false);
                rectTransform.anchoredPosition = new Vector2(20f + 340f * (float)i, 0f);
                int index = i;

                component.onClick.AddListener(delegate ()
                {
                    __instance.OpenTag(index);
                });

                component.GetComponentInChildren<Text>().text = list[i];
                __instance.GetProperty<Dictionary<int, RawImage>>("selectionHighlighter").Add(i, component.GetComponentInChildren<RawImage>());
                __instance.GetProperty<Dictionary<int, Text>>("tagText").Add(i, component.GetComponentInChildren<Text>());
                __instance.GetProperty<Dictionary<int, Text>>("tagText")[i].color = Color.gray;

                RectTransform rectTransform2 = (RectTransform)UnityEngine.Object.Instantiate<GameObject>(__instance.tagGroupContainerPrefab).transform;
                rectTransform2.SetParent(__instance.weaponEntryPanel, false);
                __instance.GetProperty<List<GameObject>>("tagGroupContainer").Add(rectTransform2.gameObject);

                RectTransform rectTransform3 = (RectTransform)rectTransform2.GetChild(0).GetChild(0);

                if (_taggedWeapons.ContainsKey(list[i]))
                {
                    int num3 = 0;
                    int num4 = 0;
                    float num5 = 0f;
                    ModInformation modInformation = ModInformation.OfficialContent;

                    using (List<WeaponManager.WeaponEntry>.Enumerator enumerator2 = _taggedWeapons[list[i]].GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            WeaponManager.WeaponEntry weaponEntry = enumerator2.Current;

                            if (weaponEntry != null && weaponEntry.sourceMod != modInformation)
                            {
                                modInformation = weaponEntry.sourceMod;
                                if (num3 > 0)
                                {
                                    num3 = 0;
                                    num4++;
                                }
                                RectTransform rectTransform4 = UnityEngine.Object.Instantiate<GameObject>(__instance.weaponEntryGroupPrefab).transform as RectTransform;
                                rectTransform4.SetParent(rectTransform3, false);
                                rectTransform4.anchoredPosition = new Vector2(((float)num3 + 0.5f) * num2, -20f - (float)num4 * 190f - num5);
                                rectTransform4.GetComponentInChildren<Text>().text = modInformation.title;
                                num5 += 40f;
                            }

                            Button component2 = UnityEngine.Object.Instantiate<GameObject>((weaponEntry == null || weaponEntry.slot == WeaponManager.WeaponSlot.Primary || weaponEntry.slot == WeaponManager.WeaponSlot.LargeGear) ? __instance.weaponEntryPrefab : __instance.weaponEntrySmallPrefab).GetComponent<Button>();
                            component2.onClick.AddListener(delegate ()
                            {
                                LoadoutUi.instance.WeaponSelected(weaponEntry);
                            });

                            RectTransform rectTransform5 = (RectTransform)component2.transform;
                            rectTransform5.SetParent(rectTransform3, false);

                            if (weaponEntry != null)
                            {
                                rectTransform5.GetComponentInChildren<Image>().sprite = weaponEntry.uiSprite;
                                rectTransform5.GetComponentInChildren<Text>().text = WeaponRegistry.GetRealName(weaponEntry);
                            }
                            else
                            {
                                rectTransform5.GetComponentInChildren<Image>().sprite = LoadoutUi.instance.nothingSprite;
                                rectTransform5.GetComponentInChildren<Text>().text = "";
                            }

                            rectTransform5.anchoredPosition = new Vector2(((float)num3 + 0.5f) * num2, -20f - (float)num4 * 190f - num5);
                            if (weaponEntry != null && !__instance.GetProperty<Dictionary<WeaponManager.WeaponEntry, int>>("tagIndexOfEntry").ContainsKey(weaponEntry))
                            {
                                __instance.GetProperty<Dictionary<WeaponManager.WeaponEntry, int>>("tagIndexOfEntry").Add(weaponEntry, i);
                            }

                            num3++;
                            if (num3 >= num)
                            {
                                num3 = 0;
                                num4++;
                            }
                        }
                    }

                    if (num3 > 0)
                    {
                        num4++;
                    }

                    rectTransform3.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 60f + (float)num4 * 190f + num5);
                }

                rectTransform2.gameObject.SetActive(false);
            }

            return false;
        }
    }
}