using System.IO;
using System;

using UnityEngine;

namespace RiseOfWar
{
    public class PlayerBadge
    {
        protected string _name;
        protected string _description;
        protected string _badgeIconPathBronze;
        protected string _badgeIconPathSilver;
        protected string _badgeIconPathGold;
        protected Texture2D[] _badgeIcons;
        protected Action<object> _onCalled;
        protected int _level;

        public Texture2D[] badgeIcons
        {
            get { return _badgeIcons; }
        }

        public string description
        {
            get { return _description; }
        }

        public string name
        {
            get { return _name; }
        }

        public int level
        {
            get { return _level; }
        }

        protected virtual void GetBadgeIcons()
        {
            _badgeIcons = new Texture2D[]
            {
                GetTexture(_badgeIconPathBronze),
                GetTexture(_badgeIconPathSilver),
                GetTexture(_badgeIconPathGold),
            };
        }

        protected Texture2D GetTexture(string path)
        {
            Texture2D _output = new Texture2D(2, 2);
            byte[] _badgeIconBytes = File.ReadAllBytes(path);

            _output.LoadImage(_badgeIconBytes);
            _output.Apply();

            return _output;
        }

        protected virtual void Init()
        {
            GetBadgeIcons();
        }

        public PlayerBadge(string name, string description, string badgeIconPathBronze, string badgeIconPathSilver, string badgeIconPathGold, Action<object> onCalled)
        {
            _name = name;
            _description = description;
            _badgeIconPathBronze = badgeIconPathBronze;
            _badgeIconPathSilver = badgeIconPathSilver;
            _badgeIconPathGold = badgeIconPathGold;
            _onCalled = onCalled;

            Init();
        }

        public virtual void Execute(object input)
        {
            _onCalled.Invoke(input);
        }
    }
}