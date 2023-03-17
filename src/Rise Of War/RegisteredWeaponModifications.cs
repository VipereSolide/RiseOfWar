using UnityEngine;

namespace RiseOfWar
{
    [System.Serializable]
    public class RegisteredWeaponModifications
    {
        public Modification modification;
        public Texture2D profilePicture;

        public RegisteredWeaponModifications(Modification modification, Texture2D profilePicture)
        {
            this.modification = modification;
            this.profilePicture = profilePicture;
        }
    }
}