using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace RiseOfWar
{
    using WeaponMeshModificator;

    public partial class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        private AudioClip[] _musicThemes;
        private AudioClip _tempAudioClip;
        private GameObject _loadingScreen;
        private AudioClip _captureJingleRU;
        private AudioClip _captureJingleUS;
        private AudioClip _captureJingleGE;
        private AudioClip _captureJingleLost;
        private AudioClip _captureJingleNeutralized;
        private AudioClip[] _whistleSounds;
        private AudioClip[] _hurtSounds;

        private GameObject _projectilePrefab;

        private List<WeaponXMLProperties> _registeredWeaponProperties = new List<WeaponXMLProperties>();
        private List<RegisteredWeaponModifications> _registeredWeaponModifications = new List<RegisteredWeaponModifications>();

        public AudioClip[] whistleSounds { get { return _whistleSounds; } }
        public AudioClip[] hurtSounds { get { return _hurtSounds; } }
        public AudioClip captureJingleRu { get { return _captureJingleRU; } }
        public AudioClip captureJingleGe { get { return _captureJingleGE; } }
        public AudioClip captureJingleUs { get { return _captureJingleUS; } }
        public AudioClip captureJingleCapturePointLost { get { return _captureJingleLost; } }
        public AudioClip captureJingleCapturePointNeutralized { get { return _captureJingleNeutralized; } }
        public Material[] projectileTracerMaterials { get; private set; }

        public AssetBundle globalKillfeedAssetBundle { get; private set; }
        public AssetBundle loadingScreenAssetBundle { get; private set; }
        public AssetBundle killfeedAssetBundle { get; private set; }
        public AssetBundle playerUIAssetBundle { get; private set; }
        public AssetBundle projectileAssetBundle { get; private set; }
        public AssetBundle weaponEditorManagerAssetBundle { get; private set; }
        public Texture2D hitmarkerTexture { get; private set; }
        public GameObject weaponEditorManager { get; private set; }
        public GameObject globalKillfeedPrefab { get; private set; }
        public GameObject globalKillfeedItemPrefab { get; private set; }

        private void Awake()
        {
            Plugin.Log("ResourceManager: Awaking resource manager...");

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadGlobalKillfeedAssetBundle();
            LoadCaptureJingleSounds();
            LoadKillfeedAssetBundle();
            LoadPlayerUIAssetBundle();
            LoadProjectilePrefab();

            GetHitmarkerTexture();
            GetWhistleSounds();
            GetHurtSounds();

            GetWeaponMeshModifications();
            AcquireWeaponModifications();
            LoadWeaponPatches();

            Plugin.Log("ResourceManager: Resoure manager awaken.");
        }

        private void Start()
        {
            Plugin.Log("ResourceManager: Starting resource manager...");

            StartCoroutine(RegisterAllWeaponProperties());

            Plugin.Log("ResourceManager: Resource manager started.");
        }

        private void GetWeaponMeshModifications()
        {
            string _path = Application.dataPath + GameConfiguration.defaultMeshModificationsPath;
            string[] _files = Directory.GetFiles(_path);

            foreach(string _file in _files)
            {
                string _content = File.ReadAllText(_file);

                XmlSerializer _serializer = new XmlSerializer(typeof(WeaponMeshModification));
                StringReader _reader = new StringReader(_content);
                WeaponMeshModification _meshModification = (WeaponMeshModification)_serializer.Deserialize(_reader);

                WeaponMeshModificationRegistry.weaponMeshModifications.Add(_meshModification);
            }
        }

        private void AcquireWeaponModifications()
        {
            string _path = Application.dataPath + GameConfiguration.defaultWeaponModificationsPath;

            string[] _foldersPath = new string[]
            {
                _path + "us/",
                _path + "ge/",
                _path + "ru/",
                _path + "neu/",
            };

            foreach (string _folder in _foldersPath)
            {
                if (Directory.Exists(_folder) == false)
                {
                    Directory.CreateDirectory(_folder);
                }

                Plugin.Log($"ResourceManager: Looking into folder \"{_folder}\"...");

                string[] _weaponFolders = Directory.GetDirectories(_folder);

                foreach (string _weaponFolder in _weaponFolders)
                {
                    Plugin.Log($"ResourceManager: Looking into weapon folder \"{_weaponFolder}\"...");
                    string[] _configurationFiles = Directory.GetFiles(_weaponFolder);

                    foreach (string _configurationFile in _configurationFiles)
                    {
                        if (Path.GetExtension(_configurationFile).Contains("xml") == false)
                        {
                            continue;
                        }

                        Plugin.Log($"ResourceManager: Looking for weapon modification XML file \"{_configurationFile}\"...");
                        string _localPath = _configurationFile.Replace(".xml", "");
                        string _profilePicturePath = _localPath + ".png";

                        if (File.Exists(_profilePicturePath) == false)
                        {
                            Plugin.LogWarning($"ResourceManager: Could not load weapon modification (\"{_configurationFile}\") because there was no profile picture PNG file found!");
                            continue;
                        }

                        string _content = File.ReadAllText(_configurationFile);
                        XmlSerializer _serializer = new XmlSerializer(typeof(Modification));
                        StringReader _reader = new StringReader(_content);
                        Modification _modification = (Modification)_serializer.Deserialize(_reader);
                        Plugin.Log($"ResourceManager: Serialized XML is null? {_modification == null}");

                        byte[] _pictureBytes = File.ReadAllBytes(_profilePicturePath);
                        Texture2D _profilePicture = new Texture2D(2, 2);
                        _profilePicture.LoadImage(_pictureBytes);
                        _profilePicture.Apply();

                        RegisteredWeaponModifications _registeredItem = new RegisteredWeaponModifications(_modification, _profilePicture);
                        _registeredWeaponModifications.Add(_registeredItem);
                    }
                }
            }
        }

        public void LoadWeaponEditorManager()
        {
            if (weaponEditorManagerAssetBundle == null)
            {
                weaponEditorManagerAssetBundle = AssetBundle.LoadFromFile(Application.dataPath + GameConfiguration.defaultAssetBundlesPath + "weapon_editor");
            }

            GameObject _weaponEditorManagerPrefab = weaponEditorManagerAssetBundle.LoadAsset("assets/weapon editor/prefabs/weapon editor.prefab") as GameObject;
            weaponEditorManager = Instantiate(_weaponEditorManagerPrefab);
            _weaponEditorManagerPrefab.AddComponent<WeaponEditorManager>().Awake();
        }

        private WeaponXMLProperties InterpretXMLFile(string _content)
        {
            // Reads the XML of the file and stores it into the properties class.
            XmlSerializer _serializer = new XmlSerializer(typeof(WeaponXMLProperties));
            StringReader _reader = new StringReader(_content);
            WeaponXMLProperties _properties = (WeaponXMLProperties)_serializer.Deserialize(_reader);

            Plugin.Log("ResourceManager: Interpretted XML properties for weapon " + _properties.name);
            return _properties;
        }

        private IEnumerator UnpackWeaponPropertiesFile(string _path)
        {
            Plugin.Log("ResourceManager: Unpacking file from path \"" + _path + "\".");

            string _dataFilePath = _path + "/data/weapon.xml";
            WeaponXMLProperties _properties = InterpretXMLFile(File.ReadAllText(_dataFilePath));
            WeaponRegistry.RegisterWeapon(_properties.name);

            yield return null;

            string _soundsDirectory = _path + "/sounds";
            string _fireSoundsDirectory = _soundsDirectory + "/fire";
            string _aimInSoundsDirectory = _soundsDirectory + "/aim in";

            _properties.soundRegisters = new List<WeaponXMLProperties.SoundRegister>()
            {
                new WeaponXMLProperties.SoundRegister()
                {
                    registerName = "fire"
                },
                new WeaponXMLProperties.SoundRegister()
                {
                    registerName = "aim in"
                }
            };

            _properties = HandleWeaponSounds(_properties, _fireSoundsDirectory, 0, _properties.GetInt(WeaponXMLProperties.BULLETS));
            _properties = HandleWeaponSounds(_properties, _aimInSoundsDirectory, 1, 3);
            _registeredWeaponProperties.Add(_properties);
        }

        private WeaponXMLProperties HandleWeaponSounds(WeaponXMLProperties _base, string _soundPath, int _index, int _soundsCount)
        {
            Plugin.Log($"ResourceManager: Handling weapon sounds for weapon \"{_base.name}\"");

            List<AudioClip> _clips = new List<AudioClip>();
            string[] _filesInAudioDirectory = Directory.GetFiles(_soundPath);

            int _count = _filesInAudioDirectory.Length;
            int _currentIndex = 0;

            for (int _i = 0; _i < _soundsCount; _i++)
            {
                _clips.Add(LoadAudioClip(_filesInAudioDirectory[_currentIndex]));
                _currentIndex++;

                if (_currentIndex >= _count)
                {
                    _currentIndex = 0;
                }
            }

            _base.soundRegisters[_index].clips = _clips;
            return _base;
        }

        private AudioClip LoadAudioClip(string _path)
        {
            StartCoroutine(LoadAudioClipInternal(_path));
            return _tempAudioClip;
        }

        private IEnumerator LoadAudioClipInternal(string _path)
        {
            Plugin.Log($"ResourceManager: Loading sound \"{Path.GetFileNameWithoutExtension(_path)}\"...");
            WWW _audioWWW = new WWW(_path);

            AudioClip _clip = _audioWWW.GetAudioClip(false, true);

            while (!_audioWWW.isDone) { }

            if (_clip == null || _clip.length == 0)
            {
                Plugin.LogError($"ResourceManager: Could not load sound \"{Path.GetFileNameWithoutExtension(_path)}\".");
                yield break;
            }

            _tempAudioClip = _clip;
            Plugin.Log($"ResourceManager: Successfully loaded sound \"{Path.GetFileNameWithoutExtension(_path)}\".");
        }

        private void GetHitmarkerTexture()
        {
            Plugin.Log("ResourceManager: Loading hitmarker texture into memory...");

            hitmarkerTexture = new Texture2D(2, 2);
            hitmarkerTexture.LoadImage(File.ReadAllBytes(Application.dataPath + GameConfiguration.defaultImagesPath + "UI/hitmarker.png"));
            hitmarkerTexture.Apply();

            if (hitmarkerTexture != null)
            {
                Plugin.Log("ResourceManager: Successfully loaded hitmarker texture into memory.");
            }
        }

        private string[] RecursivelySearchForWeaponProperties(string _path)
        {
            List<string> _weaponProperties = new List<string>();
            _weaponProperties.AddRange(Directory.GetFiles(_path));

            string[] _folders = Directory.GetDirectories(_path);

            foreach (string _folder in _folders)
            {
                _weaponProperties.AddRange(RecursivelySearchForWeaponProperties(_folder));
            }

            return _weaponProperties.ToArray();
        }

        public void GetAndApplyMusicThemes()
        {
            string _path = Application.dataPath + GameConfiguration.defaultMusicThemesPath;
            string[] _musicThemePaths = Directory.GetFiles(_path);
            List<AudioClip> _themes = new List<AudioClip>();

            foreach (string _music in _musicThemePaths)
            {
                AudioClip _clip = LoadAudioClip(_music);

                _themes.Add(_clip);
            }

            _musicThemes = _themes.ToArray();
            MainMenu.instance.music.clips = _musicThemes;
        }

        private void LoadProjectilePrefab()
        {
            projectileAssetBundle = AssetBundle.LoadFromFile(Application.dataPath + GameConfiguration.defaultAssetBundlesPath + "tracer");
            _projectilePrefab = (GameObject)projectileAssetBundle.LoadAsset("assets/tracer/tracer.prefab");

            List<Material> _materials = new List<Material>();
            _materials.Add((Material)projectileAssetBundle.LoadAsset("assets/tracer/yellow tracer.mat"));
            _materials.Add((Material)projectileAssetBundle.LoadAsset("assets/tracer/red tracer.mat"));
            _materials.Add((Material)projectileAssetBundle.LoadAsset("assets/tracer/green tracer.mat"));
            _materials.Add((Material)projectileAssetBundle.LoadAsset("assets/tracer/blue tracer.mat"));

            projectileTracerMaterials = _materials.ToArray();
        }

        public void WriteChangesToFile(WeaponXMLProperties properties)
        {
            using (StringWriter _stringWriter = new StringWriter())
            {
                XmlSerializer _serializer = new XmlSerializer(typeof(WeaponXMLProperties));
                _serializer.Serialize(_stringWriter, properties);
                File.WriteAllText(Application.dataPath + "/Resources/Data/weapon.xml", _stringWriter.ToString());
            }
        }

        public Weapon.Configuration GetConfigurationFromProperties(Weapon _weapon, WeaponXMLProperties _weaponProperties)
        {
            Plugin.Log($"ResourceManager: Getting weapon configuration from properties of \"{_weapon.name}\"...");

            if (_weapon == null)
            {
                Plugin.LogError("ResourceManager: Cannot get weapon configuration for null weapon.");
                return null;
            }

            if (_weaponProperties == null)
            {
                Plugin.LogWarning("ResourceManager: Cannot get weapon configuration for weapon from null properties.");
                return _weapon.configuration;
            }

            Weapon.Configuration _base = _weapon.configuration;
            WeaponAdditionalData _additionalData = _weapon.GetAdditionalData();
            Projectile _projectile = _base.projectile();

            _base.kickback = 0;
            _base.spread = 0;
            _base.randomKick = 0;
            _base.snapMagnitude = 0;
            _base.followupMaxSpreadAim = 0;
            _base.followupMaxSpreadHip = 0;

            if (_projectile.armorDamage != Vehicle.ArmorRating.AntiTank)
            {
                _projectile.autoAssignArmorDamage = false;
                _projectile.armorDamage = Vehicle.ArmorRating.HeavyArms;
            }

            if (_weaponProperties.damage.HasParam(WeaponXMLProperties.Damage.VEHICLE_DAMAGE))
            {
                _additionalData.damageToVehicles = _weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.VEHICLE_DAMAGE);
            }
            else
            {
                _additionalData.damageToVehicles = _projectile.configuration.damageDropOff[0].value;
            }

            _base.ammo = _weaponProperties.GetInt(WeaponXMLProperties.BULLETS);
            _base.maxAmmoPerReload = _weaponProperties.GetInt(WeaponXMLProperties.BULLETS);
            _base.spareAmmo = _weaponProperties.GetInt(WeaponXMLProperties.MAGAZINES) * _weaponProperties.GetInt(WeaponXMLProperties.BULLETS);
            _base.aimSensitivityMultiplier = 1;
            _base.forceSniperAimSensitivity = false;

            float _shortDamage = _weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.SHORT_DAMAGE);
            float _shortDistance = _weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.SHORT_DISTANCE);
            float _longDamage = _weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.LONG_DAMAGE);
            float _longDistance = _weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.LONG_DISTANCE);
            float _roundsPerMinute = _weaponProperties.GetFloat(WeaponXMLProperties.ROUNDS_PER_MINUTE);
            float _velocity = _weaponProperties.projectile.GetFloat(WeaponXMLProperties.Projectile.VELOCITY);

            if (_additionalData.modifications == null)
            {
                _additionalData.InitWeaponModifications();

                if (_additionalData.modifications == null)
                {
                    Plugin.LogError($"ResourceManager: Cannot get weapon configuration for weapon with null modifications ({_weapon.transform.name})!");
                    return _weapon.configuration;
                }
            }

            if (_additionalData.modifications != null)
            {
                _shortDamage += _additionalData.modifications.GetModifiedValue(_shortDamage, Modification.Modifications.SHORT_DAMAGE);
                _shortDistance += _additionalData.modifications.GetModifiedValue(_shortDistance, Modification.Modifications.SHORT_DISTANCE);
                _longDamage += _additionalData.modifications.GetModifiedValue(_longDamage, Modification.Modifications.LONG_DAMAGE);
                _velocity += _additionalData.modifications.GetModifiedValue(_velocity, Modification.Modifications.VELOCITY);
                _longDistance += _additionalData.modifications.GetModifiedValue(_longDistance, Modification.Modifications.LONG_DISTANCE);
                _roundsPerMinute += _additionalData.modifications.GetModifiedValue(_roundsPerMinute, Modification.Modifications.ROUNDS_PER_MINUTE);
            }

            _base.cooldown = 60f / _roundsPerMinute;

            // _projectile.configuration.inheritVelocity = true;
            _projectile.configuration.gravityMultiplier = 1;
            _projectile.configuration.speed = _velocity;

            _base.aimFov = GameConfiguration.defaultAimingFieldOfView;
            ((AudioSource)Traverse.Create(_weapon).Field("audio").GetValue()).clip = null;

            _projectile.configuration.damage = _weaponProperties.projectile.GetFloat(WeaponXMLProperties.Projectile.DAMAGE_MULTIPLIER);
            _projectile.configuration.dropoffEnd = _longDistance;

            List<Keyframe> _keys = new List<Keyframe>()
            {
                new Keyframe(0, _shortDamage),
                new Keyframe(_base.projectile().configuration.dropoffEnd / _shortDistance,  _shortDamage),
                new Keyframe(1,  _longDamage),
            };
            _projectile.configuration.damageDropOff = new AnimationCurve(_keys.ToArray());

            _base.followupMaxSpreadAim = 0;
            _base.followupMaxSpreadHip = 0;

            if (_weaponProperties.HasParam(WeaponXMLProperties.IS_ADVANCED_RELOAD))
            {
                _base.advancedReload = _weaponProperties.GetBool(WeaponXMLProperties.IS_ADVANCED_RELOAD);
            }

            if (_weaponProperties.HasParam(WeaponXMLProperties.USE_MAX_AMMO_PER_RELOAD))
            {
                _base.useMaxAmmoPerReload = _weaponProperties.GetBool(WeaponXMLProperties.USE_MAX_AMMO_PER_RELOAD);
            }
            if (_weaponProperties.HasParam(WeaponXMLProperties.DROP_AMMO_WHEN_RELOADING))
            {
                _base.dropAmmoWhenReloading = _weaponProperties.GetBool(WeaponXMLProperties.DROP_AMMO_WHEN_RELOADING);
            }

            _base.projectilePrefab = _projectile.gameObject;

            return _base;
        }

        public Weapon.Configuration GetGlobalConfigurationForBots(Weapon _weapon, WeaponXMLProperties _weaponProperties)
        {
            if (_weapon == null || _weapon.configuration == null)
            {
                return _weapon.configuration;
            }

            Weapon.Configuration _base = _weapon.configuration;
            //_base.projectilePrefab = _projectilePrefab;
            //_base.projectilePrefab = CopyProjectileConfiguration(_base.projectile(), _projectilePrefab.GetComponent<Projectile>()).gameObject;

            Projectile _projectile = _base.projectile();
            float _shortDamage = _weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.SHORT_DAMAGE);
            float _shortDistance = _weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.SHORT_DISTANCE);
            float _longDamage = _weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.LONG_DAMAGE);
            float _longDistance = _weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.LONG_DISTANCE);

            _projectile.configuration.damage = _weaponProperties.projectile.GetFloat(WeaponXMLProperties.Projectile.DAMAGE_MULTIPLIER);
            _projectile.configuration.dropoffEnd = _longDistance;

            List<Keyframe> _keys = new List<Keyframe>()
            {
                new Keyframe(0, _shortDamage),
                new Keyframe(_base.projectile().configuration.dropoffEnd / _shortDistance,  _shortDamage),
                new Keyframe(1,  _longDamage),
            };

            _projectile.configuration.damageDropOff = new AnimationCurve(_keys.ToArray());
            _base.projectilePrefab = _projectile.gameObject;

            return _base;
        }

        private void CopyProjectileConfiguration(Projectile from, Projectile to)
        {
            to.configuration.makesFlybySound = from.configuration.makesFlybySound;
            to.configuration.flybyPitch = from.configuration.flybyPitch;
            to.configuration.impactDecalSize = from.configuration.impactDecalSize;
            to.configuration.passThroughPenetrateLayer = from.configuration.passThroughPenetrateLayer;
            to.configuration.piercing = from.configuration.piercing;
        }

        public List<RegisteredWeaponModifications> GetPossibleWeaponModifications(string weaponName)
        {
            List<RegisteredWeaponModifications> _output = new List<RegisteredWeaponModifications>();

            Debug.Log("There is no registered modifications right :(? " + (_registeredWeaponModifications.Count <= 0 ? "yeeaaaah ;-;" : "naah there is, it's fine :)"));

            foreach (RegisteredWeaponModifications _modification in _registeredWeaponModifications)
            {
                Debug.Log("weapon name = " + weaponName);
                Debug.Log("modification applies to = " + _modification.modification.weaponName);

                if (_modification.modification.weaponName == weaponName)
                {
                    _output.Add(_modification);
                }
            }

            return _output;
        }

        public void EnableLoadingScreen()
        {
            _loadingScreen.SetActive(true);
        }

        public void DisableLoadingScreen()
        {
            _loadingScreen.SetActive(false);
        }

        public void LoadCaptureJingleSounds()
        {
            Plugin.Log("ResourceManager: Loading capture jingles...");

            _captureJingleGE = LoadAudioClip(Application.dataPath + GameConfiguration.defaultCaptureJinglesPath + "capture_jingle_ge.wav");
            _captureJingleUS = LoadAudioClip(Application.dataPath + GameConfiguration.defaultCaptureJinglesPath + "capture_jingle_us.wav");
            _captureJingleRU = LoadAudioClip(Application.dataPath + GameConfiguration.defaultCaptureJinglesPath + "capture_jingle_ru.wav");
            _captureJingleLost = LoadAudioClip(Application.dataPath + GameConfiguration.defaultCaptureJinglesPath + "capture_jingle_point_lost.wav");
            _captureJingleNeutralized = LoadAudioClip(Application.dataPath + GameConfiguration.defaultCaptureJinglesPath + "capture_jingle_neutralized.wav");

            transform.gameObject.AddComponent<MusicJingleManager>();
            Plugin.Log("ResourceManager: Loaded capture jingles.");
        }

        public void LoadLoadingScreenAssetBundle()
        {
            Plugin.Log("ResourceManager: Loading loading screen asset bundle...");

            loadingScreenAssetBundle = AssetBundle.LoadFromFile(Application.dataPath + GameConfiguration.defaultAssetBundlesPath + "loading_screen");
            _loadingScreen = Instantiate((GameObject)loadingScreenAssetBundle.LoadAsset("assets/loading mods menu/loading mods menu.prefab"), Instance.transform);
            _loadingScreen.transform.name = "Loading Screen";
            DisableLoadingScreen();

            Plugin.Log("ResourceManager: Loaded loading screen asset bundle.");
        }

        public void LoadGlobalKillfeedAssetBundle()
        {
            Plugin.Log("ResourceManager: Loading global killfeed asset bundle...");

            globalKillfeedAssetBundle = AssetBundle.LoadFromFile(Application.dataPath + GameConfiguration.defaultAssetBundlesPath + "global_killfeed");

            if (globalKillfeedAssetBundle == null)
            {
                Plugin.LogError("ResourceManager: Could not load global killfeed asset bundle!");
                return;
            }

            try
            {
                globalKillfeedItemPrefab = globalKillfeedAssetBundle.LoadAsset("assets/global killfeed/prefabs/global killfeed item.prefab") as GameObject;
                globalKillfeedPrefab = globalKillfeedAssetBundle.LoadAsset("assets/global killfeed/prefabs/global killfeed.prefab") as GameObject;
            }
            catch (Exception _exception)
            {
                Plugin.LogError("ResourceManager: Could not load global killfeed asset bundle! " + _exception);
                return;
            }

            Plugin.Log("ResourceManager: Loaded global killfeed asset bundle.");
        }

        public void LoadKillfeedAssetBundle()
        {
            Plugin.Log("ResourceManager: Loading killfeed asset bundle...");

            killfeedAssetBundle = AssetBundle.LoadFromFile(Application.dataPath + GameConfiguration.defaultAssetBundlesPath + "killfeed_ui");

            Plugin.Log("ResourceManager: Loaded killfeed asset bundle.");
        }

        public void LoadPlayerUIAssetBundle()
        {
            Plugin.Log("ResourceManager: Loading player asset bundle...");

            playerUIAssetBundle = AssetBundle.LoadFromFile(Application.dataPath + GameConfiguration.defaultAssetBundlesPath + "player_ui");

            Plugin.Log("ResourceManager: Loaded player asset bundle.");
        }

        private IEnumerator RegisterAllWeaponProperties()
        {
            yield return new WaitForSeconds(1);

            Plugin.Log("ResourceManager: Registering all weapon properties...");

            string _basePath = Application.dataPath + GameConfiguration.defaultWeaponDataPath;

            string[] _directoriesGE = Directory.GetDirectories(_basePath + "ge/");
            string[] _directoriesUS = Directory.GetDirectories(_basePath + "us/");
            string[] _directoriesRU = Directory.GetDirectories(_basePath + "ru/");
            string[] _directoriesNEU = Directory.GetDirectories(_basePath + "neu/");

            foreach (string _dir in _directoriesGE)
            {
                StartCoroutine(UnpackWeaponPropertiesFile(_dir));
            }

            foreach (string _dir in _directoriesUS)
            {
                StartCoroutine(UnpackWeaponPropertiesFile(_dir));
            }

            foreach (string _dir in _directoriesRU)
            {
                StartCoroutine(UnpackWeaponPropertiesFile(_dir));
            }

            foreach (string _dir in _directoriesNEU)
            {
                StartCoroutine(UnpackWeaponPropertiesFile(_dir));
            }
        }

        private void LoadWeaponPatches()
        {
            string _path = Application.dataPath + GameConfiguration.defaultPatchesPath + "Weapons/";
            string[] _files = Directory.GetFiles(_path);

            foreach (string _file in _files)
            {
                string _content = File.ReadAllText(_file);
                XmlSerializer _serializer = new XmlSerializer(typeof(Patch));
                StringReader _reader = new StringReader(_content);
                Patch _patch = (Patch)_serializer.Deserialize(_reader);

                if (_patch.type == "name")
                {
                    WeaponRegistry.RegisterNewRealName(_patch.GetString(Patch.PATCH_NAME_DEFAULT_NAME), _patch.GetString(Patch.PATCH_NAME_PATCHED_NAME));
                }
            }
        }

        public static WeaponXMLProperties GetWeaponProperties(Weapon _weapon)
        {
            if (_weapon == null)
            {
                Plugin.LogError("ResourceManager: Cannot get properties of null weapon!");
                return null;
            }

            foreach (WeaponXMLProperties _properties in Instance._registeredWeaponProperties)
            {
                if (_properties.name.Equals(_weapon.name))
                {
                    return _properties;
                }
            }

            // Plugin.LogError($"ResourceManager: Could not find weapon properties for weapon \"{_weapon.name}\"!");
            return null;
        }

        private void GetWhistleSounds()
        {
            string _path = Application.dataPath + GameConfiguration.defaultWhistlePath;
            string[] _files = Directory.GetFiles(_path);
            List<AudioClip> _clips = new List<AudioClip>();

            foreach (string _file in _files)
            {
                if (Path.GetExtension(_file).Contains("wav") == false)
                {
                    continue;
                }

                _clips.Add(LoadAudioClip(_file));
            }

            _whistleSounds = _clips.ToArray();
        }

        private void GetHurtSounds()
        {
            string _path = Application.dataPath + GameConfiguration.defaultHurtPath;
            string[] _files = Directory.GetFiles(_path);
            List<AudioClip> _clips = new List<AudioClip>();

            foreach (string _file in _files)
            {
                if (Path.GetExtension(_file).Contains("wav") == false)
                {
                    continue;
                }

                _clips.Add(LoadAudioClip(_file));
            }

            _hurtSounds = _clips.ToArray();
        }
    }
}