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

        private float ParseButUglyIEJFNZIEJEF(string _bodytext)
        {
            return float.Parse(_bodytext);
/*
            string _fin = "";

            foreach (char _char in _bodytext)
            {
                if (_char == '0')
                {
                    continue;
                }

                if (_char == '1')
                {
                    if (float.TryParse(_char.ToString(), out float _r))
                    {
                        _fin += _char;
                    }
                }
                if (_char == '2')
                {
                    if (float.TryParse(_char.ToString(), out float _r))
                    {
                        _fin += _char;
                    }
                }
                if (_char == '3')
                {
                    if (float.TryParse(_char.ToString(), out float _r))
                    {
                        _fin += _char;
                    }
                }
                if (_char == '4')
                {
                    if (float.TryParse(_char.ToString(), out float _r))
                    {
                        _fin += _char;
                    }
                }
                if (_char == '5')
                {
                    if (float.TryParse(_char.ToString(), out float _r))
                    {
                        _fin += _char;
                    }
                }
                if (_char == '6')
                {
                    if (float.TryParse(_char.ToString(), out float _r))
                    {
                        _fin += _char;
                    }
                }
                if (_char == '7')
                {
                    if (float.TryParse(_char.ToString(), out float _r))
                    {
                        _fin += _char;
                    }
                }
                if (_char == '8')
                {
                    if (float.TryParse(_char.ToString(), out float _r))
                    {
                        _fin += _char;
                    }
                }
                if (_char == '9')
                {
                    if (float.TryParse(_char.ToString(), out float _r))
                    {
                        _fin += _char;
                    }
                }
                if (_char == '.')
                {
                    if (float.TryParse(_char.ToString(), out float _r))
                    {
                        _fin += _char;
                    }
                }
            }

            return float.Parse(_fin);*/
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
        public virtual int GetInt(string _name)
        {
            Param _output = null;

            if (TryFindParam(_name, out _output))
            {
                return Mathf.CeilToInt(float.Parse(_output.text));
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
                    return ParseButUglyIEJFNZIEJEF(_output.text);
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
                return new Vector2(ParseButUglyIEJFNZIEJEF(_output.vx), ParseButUglyIEJFNZIEJEF(_output.vy));
            }

            return Vector2.zero;
        }
        public virtual Vector3 GetVector3(string _name)
        {
            Param _output = null;

            if (TryFindParam(_name, out _output))
            {
                return new Vector3(ParseButUglyIEJFNZIEJEF(_output.vx), ParseButUglyIEJFNZIEJEF(_output.vy), ParseButUglyIEJFNZIEJEF(_output.vz));
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
    }
}