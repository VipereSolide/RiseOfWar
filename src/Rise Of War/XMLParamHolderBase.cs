using UnityEngine;

using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace RiseOfWar
{
    public abstract class XMLParamHolderBase
    {
        [XmlElement(ElementName = "param")]
        public List<Param> Param;

        private float ParseToFloat(string content)
        {
            try
            {
                return float.Parse(content);
            }
            catch
            {
                return float.Parse(content.Replace(".", ","));
            }
        }

        public virtual bool HasParam(string name)
        {
            Param _param = null;
            return TryFindParam(name, out _param);
        }

        public virtual Param FindParam(string _name)
        {
            for (int _i = 0; _i < Param.Count; _i++)
            {
                if (Param[_i].name == _name)
                {
                    return Param[_i];
                }
            }

            Debug.LogError($"WeaponGroupBase: Couldn't find param named \"{_name}\"!");
            return null;
        }

        public virtual bool TryFindParam(string _name, out Param _output)
        {
            for (int _i = 0; _i < Param.Count; _i++)
            {
                if (Param[_i].name == _name)
                {
                    _output = Param[_i];
                    return true;
                }
            }

            _output = null;
            return false;
        }

        public virtual string GetString(string name)
        {
            Param _output = null;

            if (TryFindParam(name, out _output))
            {
                return _output.text;
            }

            return string.Empty;
        }

        public virtual int GetInt(string _name)
        {
            Param _output = null;

            if (TryFindParam(_name, out _output))
            {
                return Mathf.CeilToInt(ParseToFloat(_output.text));
            }

            return 0;
        }

        public virtual float GetFloat(string _name)
        {
            // Debug.Log($"name: {_name}");
            Param _output = null;

            if (TryFindParam(_name, out _output))
            {
                // Debug.Log($"output is null: {_output == null}");
                string _toparse = _output.text;
                // Debug.Log($"output.text: {_toparse.Replace(" ","&&").Replace("\n","&&").Replace("\r","&&").Replace("\\","")}");
                try
                {
                    return ParseToFloat(_output.text);
                }
                catch (Exception _ex)
                {
                    // Debug.LogError("TO PARSE=====" + _toparse);
                    Plugin.LogError($"Couldn't parse float: " + _ex);
                    return 0;
                }
            }

            return 0;
        }

        public virtual Vector2 GetVector2(string _name)
        {
            Param _output = null;

            if (TryFindParam(_name, out _output))
            {
                return new Vector2(ParseToFloat(_output.vx), ParseToFloat(_output.vy));
            }

            return Vector2.zero;
        }

        public virtual Vector3 GetVector3(string _name)
        {
            Param _output;

            if (TryFindParam(_name, out _output))
            {
                return new Vector3(ParseToFloat(_output.vx), ParseToFloat(_output.vy), ParseToFloat(_output.vz));
            }

            return Vector3.zero;
        }

        public virtual bool GetBool(string _name)
        {
            Param _output = null;

            if (TryFindParam(_name, out _output))
            {
                return bool.Parse(_output.text.ToLower());
            }

            return false;
        }

        public virtual void SetVector3(string name, Vector3 value)
        {
            Param _param;

            if (TryFindParam(name, out _param) == false)
            {
                return;
            }

            int _index = Param.IndexOf(_param);

            _param.vx = value.x.ToString();
            _param.vy = value.y.ToString();
            _param.vz = value.z.ToString();

            Param[_index] = _param;
        }
    }
}