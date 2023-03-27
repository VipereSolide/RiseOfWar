using System;
using System.Xml.Serialization;

namespace RiseOfWar
{
    [XmlRoot(ElementName = "param")]
    [Serializable]
    public class Param
    {
        public static readonly string OPERATION_ADD = "addition";
        public static readonly string OPERATION_SUBSTRACTION = "substraction";
        public static readonly string OPERATION_DIVISION = "division";
        public static readonly string OPERATION_MULTIPLICATION = "multiplication";
        public static readonly string OPERATION_ADDED_PERCENTAGE = "addedPercentage";
        public static readonly string OPERATION_SUBSTRACTED_PERCENTAGE = "substractedPercentage";
        public static readonly string OPERATION_OVERRIDE_PERCENTAGE = "overridePercentage";

        [XmlAttribute(AttributeName = "name")]
        public string name;

        [XmlAttribute(AttributeName = "type")]
        public string type;

        [XmlText]
        public string text;

        [XmlAttribute(AttributeName = "vx")]
        public string vx;

        [XmlAttribute(AttributeName = "vy")]
        public string vy;

        [XmlAttribute(AttributeName = "vz")]
        public string vz;

        [XmlAttribute(AttributeName = "operation")]
        public string operation;
    }
}