using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections;
using System;

using UnityEngine;
using HarmonyLib;

namespace RiseOfWar.WeaponMeshModificator
{
    public struct ModificationType
    {
        public string name;
        public Param.Type type;
        public Action<Transform, Param> callback;

        public ModificationType(string name, Param.Type type, Action<Transform, Param> callback)
        {
            this.name = name;
            this.type = type;
            this.callback = callback;
        }
    }

    [Serializable]
    [XmlRoot(ElementName = "weaponMeshModification")]
    public class WeaponMeshModification
    {
        public static readonly ModificationType[] WeaponMeshModificationTypes = new ModificationType[]
        {
            new ModificationType("isActive", Param.Type.Bool, (Transform target, Param param) =>
            {
                target.gameObject.SetActive(param.GetBool());
            }),
            new ModificationType("position", Param.Type.Vector3, (Transform target, Param param) =>
            {
                target.localPosition = param.GetVector3();
            }),
            new ModificationType("rotation", Param.Type.Vector3, (Transform target, Param param) =>
            {
                target.localRotation = Quaternion.Euler(param.GetVector3());
            }),
            new ModificationType("scale", Param.Type.Vector3, (Transform target, Param param) =>
            {
                target.localScale = param.GetVector3();
            }),
        };

        [XmlElement(ElementName = "weapon")]
        public string weapon;

        [XmlElement(ElementName = "element")]
        public List<ModificationElement> Element;
    }

    [Serializable]
    [XmlRoot(ElementName = "element")]
    public class ModificationElement : XMLParamHolderBase
    {
        [XmlAttribute(AttributeName = "target")]
        public string target;

        [XmlElement(ElementName = "element")]
        public List<ModificationElement> Element;
    }
}