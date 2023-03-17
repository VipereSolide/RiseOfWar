using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RiseOfWar
{
    public static class WeaponRegistry
    {
        public enum Weapons
        {
            Gewehr43,
            M1Garand,
            M1Carbine,
            M1A1Carbine,
            SVT40,
            Karabiner98Kurz,
            M1903,
            MosinNagantM9130,
            MP34,
            MP40,
            M3GreaseGun,
            ThompsonM1A1,
            PPD40,
            PPSH41,
            PPS43,
            MachineCarbineMK1,
            MG13,
            MG34,
            MG42,
            FG42AusfE,
            M1918A2,
            Johnson1941,
            M1919MG,
            DegtyarevDP28,
            DegtyarevDT29,
            MaximTokarevMT,
            STG44,
            M1M2Carbine,
            AVS36,
            Flammenwefer41,
            M2Flamethrower,
            ROKS3Flamethrower,
            Panzerbuchse39,
            PTRD41,
            PTRS41,
            Model1934,
            PistolC96,
            LugerP08,
            WaltherP38,
            M1903PocketHammerless,
            Pistol1911,
            M1917,
            KorovinTK,
            TT33,
            NagantM1895,
        }
        
        private static string[] _weaponNames = new string[]
        {
            "GREASER",
            ".357 CONDER",
            "HMG",
            "PATRIOT",
            "PATRIOT GL",
            "PATRIOT TAC",
            "QUICKSILVER",
            "RK-44",
            "SIGNAL DMR",
            "SL-DEFENDER",
            "M1 GARRET",
            "RECON LRR",
            "76 EAGLE",
            "AUTOMATICO AKIMBO",
            "BUNDLE O' BUSS",
            "S-IND7",
            "C4",
            "FRAG",
            "SMOKE GRENADE",
            "SPEARHEAD",
            "THUMPER",
            "SCALPEL",
            "DAGGER",
            "HYDRA",
            "SLAM-R",
            "AIR-HORN",
            "AMMO BAG",
            "MEDIC BAG",
            "SABRE",
            "SQUAD LEADER KIT",
            "WRENCH",
        };
        public static string[] weaponNames
        {
            get { return _weaponNames; }
        }
        public static bool IsCustomWeapon(Weapon _weapon)
        {
            if (_weapon == null || _weapon.name == "null")
            {
                return false;
            }

            return !weaponNames.ToList().Contains(_weapon.name);
        }
    }
}