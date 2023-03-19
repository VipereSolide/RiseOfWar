using System.Xml.Serialization;
using UnityEngine;

namespace RiseOfWar
{
    [SerializeField]
    [XmlRoot(ElementName = "patch")]
    public class Patch : XMLParamHolderBase
    {
        public enum PatchTypes
        {
            name
        }

        public static readonly string PATCH_NAME_DEFAULT_NAME = "defaultName";
        public static readonly string PATCH_NAME_PATCHED_NAME = "patchedName";

        [XmlAttribute(AttributeName = "type")]
        public string type;
    }
}