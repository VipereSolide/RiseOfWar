using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections;
using System.IO;
using System;

using UnityEngine;

namespace RiseOfWar
{
    using WeaponMeshModificator;

    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        private AudioClip[] _musicThemesAudioClips;

        private AudioClip _captureJingleSovietUnion;
        private AudioClip _captureJingleUnitedStates;
        private AudioClip _captureJingleGermany;
        private AudioClip _captureJinglePointLost;
        private AudioClip _captureJinglePointNeutralized;

        private AudioClip[] _whistleAudioClips;
        private AudioClip[] _hurtAudioClips;

        private GameObject _projectilePrefab;
        private GameObject _loadingScreen;
        private AudioClip _temporaryAudioClip;

        private readonly List<WeaponXMLProperties> _registeredWeaponProperties = new List<WeaponXMLProperties>();
        private readonly List<RegisteredWeaponModifications> _registeredWeaponModifications = new List<RegisteredWeaponModifications>();

        public AudioClip[] WhistleAudioClips { get { return _whistleAudioClips; } }
        public AudioClip[] HurtAudioClips { get { return _hurtAudioClips; } }
        public AudioClip CaptureJingleSovietUnion { get { return _captureJingleSovietUnion; } }
        public AudioClip CaptureJingleGermany { get { return _captureJingleGermany; } }
        public AudioClip CaptureJingleUnitedStates { get { return _captureJingleUnitedStates; } }
        public AudioClip CaptureJinglePointLost { get { return _captureJinglePointLost; } }
        public AudioClip CaptureJinglePointNeutralized { get { return _captureJinglePointNeutralized; } }

        public Material[] ProjectileTracerMaterials { get; private set; }

        public AssetBundle GlobalKillfeedAssetBundle { get; private set; }
        public GameObject GlobalKillfeedPrefab { get; private set; }
        public GameObject GlobalKillfeedItemPrefab { get; private set; }

        public AssetBundle KillfeedAssetBundle { get; private set; }
        public AssetBundle LoadingScreenAssetBundle { get; private set; }
        public AssetBundle PlayerUIAssetBundle { get; private set; }
        public AssetBundle ProjectileAssetBundle { get; private set; }
        public AssetBundle WeaponEditorManagerAssetBundle { get; private set; }
        public Texture2D HitmarkerTexture { get; private set; }
        public GameObject WeaponEditorManager { get; private set; }

        private void Awake()
        {
            Plugin.Log("ResourceManager: Awaking...");

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadBundleGlobalKillfeed();
            LoadBundleKillfeed();
            LoadBundlePlayerUI();
            GetProjectilePrefab();

            LoadCaptureJingles();
            LoadHitmarkerTexture();
            LoadWhistleSounds();
            LoadHurtSounds();

            LoadWeaponMeshModifications();
            LoadWeaponModifications();
            LoadWeaponPatches();

            Plugin.Log("ResourceManager: Awakened.");
        }

        private void Start()
        {
            Plugin.Log("ResourceManager: Starting...");

            Invoke(nameof(RegisterWeaponProperties), 1);

            Plugin.Log("ResourceManager: Started.");
        }

        private void LoadWeaponMeshModifications()
        {
            Plugin.Log($"ResourceManager: Loading weapon mesh modifications...");

            string _meshModificationsPath = Application.dataPath + GameConfiguration.defaultMeshModificationsPath;
            string[] _meshModifications = Directory.GetFiles(_meshModificationsPath);

            Plugin.Log($"ResourceManager: Found {_meshModifications.Length} mesh modification files:");

            foreach (string _meshModificationFile in _meshModifications)
            {
                string _content = File.ReadAllText(_meshModificationFile);

                XmlSerializer _serializer = new XmlSerializer(typeof(WeaponMeshModification));
                StringReader _reader = new StringReader(_content);
                WeaponMeshModification _meshModification = (WeaponMeshModification)_serializer.Deserialize(_reader);

                Plugin.Log($"ResourceManager: * Mesh modification for weapon \"{_meshModification.weapon}\".");

                WeaponMeshModificationRegistry.weaponMeshModifications.Add(_meshModification);
            }
        }

        private void LoadWeaponModifications()
        {
            Plugin.Log($"ResourceManager: Loading weapon modifications...");

            string _weaponModificationsPath = Application.dataPath + GameConfiguration.defaultWeaponModificationsPath;

            string[] _weaponTypesFolders = new string[]
            {
                _weaponModificationsPath + "us/",
                _weaponModificationsPath + "ge/",
                _weaponModificationsPath + "ru/",
                _weaponModificationsPath + "neu/",
            };

            foreach (string _weaponTypeFolder in _weaponTypesFolders)
            {
                if (Directory.Exists(_weaponTypeFolder) == false)
                {
                    Plugin.LogWarning($"ResourceManager: Could not find folder \"{_weaponTypeFolder}\". Creating it instead...");
                    Directory.CreateDirectory(_weaponTypeFolder);
                }

                Plugin.Log($"ResourceManager: Analyzing folder \"{_weaponTypeFolder}\".");

                string[] _weaponModificationsFolders = Directory.GetDirectories(_weaponTypeFolder);

                Plugin.Log($"ResourceManager: Found {_weaponModificationsFolders.Length} weapon modifications folders.");

                foreach (string _weaponModificationFolder in _weaponModificationsFolders)
                {
                    Plugin.Log($"ResourceManager: Analyzing weapon modification folder \"{_weaponModificationFolder}\".");
                    string[] _modificationFiles = Directory.GetFiles(_weaponModificationFolder);

                    foreach (string _modificationFile in _modificationFiles)
                    {
                        if (Path.GetExtension(_modificationFile).Contains("xml") == false)
                        {
                            continue;
                        }

                        Plugin.Log($"ResourceManager: * Weapon modification XML file \"{_modificationFile}\".");
                        string _fileLocalPath = _modificationFile.Replace(".xml", "");
                        string _modificationProfilePicturePath = _fileLocalPath + ".png";

                        if (File.Exists(_modificationProfilePicturePath) == false)
                        {
                            Plugin.LogWarning($"ResourceManager: Could not load weapon modification file \"{_fileLocalPath}\"! Please place a \".png\" profile picture for this weapon modification of the same name as the \".xml\" file.");
                            continue;
                        }

                        string _modificationFileContent = File.ReadAllText(_modificationFile);
                        XmlSerializer _serializer = new XmlSerializer(typeof(Modification));
                        StringReader _reader = new StringReader(_modificationFileContent);
                        Modification _modification = (Modification)_serializer.Deserialize(_reader);

                        byte[] _pictureBytes = File.ReadAllBytes(_modificationProfilePicturePath);
                        Texture2D _profilePicture = new Texture2D(2, 2);
                        _profilePicture.LoadImage(_pictureBytes);
                        _profilePicture.Apply();

                        RegisteredWeaponModifications _registeredItem = new RegisteredWeaponModifications(_modification, _profilePicture);
                        _registeredWeaponModifications.Add(_registeredItem);
                    }

                    Plugin.EndLogGroup();
                }

                Plugin.EndLogGroup();
            }
        }

        private WeaponXMLProperties ReadWeaponPropertiesFile(string weaponPropertiesFileContent)
        {
            // Reads the XML of the file and stores it into the properties class.
            XmlSerializer _serializer = new XmlSerializer(typeof(WeaponXMLProperties));
            StringReader _reader = new StringReader(weaponPropertiesFileContent);
            WeaponXMLProperties _properties = (WeaponXMLProperties)_serializer.Deserialize(_reader);

            Plugin.Log($"ResourceManager: Read XML properties for weapon \"{_properties.name}\".");
            return _properties;
        }

        private void UnpackWeaponPropertiesFile(string path)
        {
            Plugin.Log($"ResourceManager: Unpacking weapon properties file from path \"{path}\"...");

            string _dataFilePath = path + "/data/weapon.xml";
            string _dataFileContent = File.ReadAllText(_dataFilePath);
            WeaponXMLProperties _weaponProperties = ReadWeaponPropertiesFile(_dataFileContent);

            WeaponRegistry.RegisterWeapon(_weaponProperties.name);

            string _soundsDirectory = path + "/sounds";
            string _fireSoundsDirectory = _soundsDirectory + "/fire";
            string _aimInSoundsDirectory = _soundsDirectory + "/aim in";

            _weaponProperties.soundRegisters = new List<WeaponXMLProperties.SoundRegister>()
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

            int _wantedFireSoundsSamples = _weaponProperties.GetInt(WeaponXMLProperties.BULLETS);
            int _wantedAimSoundsSamples = 2;

            _weaponProperties = GetWeaponSounds(_weaponProperties, _fireSoundsDirectory, 0, _wantedFireSoundsSamples);
            _weaponProperties = GetWeaponSounds(_weaponProperties, _aimInSoundsDirectory, 1, _wantedAimSoundsSamples);

            _registeredWeaponProperties.Add(_weaponProperties);
            Plugin.Log($"ResourceManager: Unpacked weapon properties file for weapon properties \"{_weaponProperties.name}\".");
        }

        private WeaponXMLProperties GetWeaponSounds(WeaponXMLProperties baseProperties, string soundsFolderPath, int soundRegisterIndex, int wantedSoundSamples)
        {
            Plugin.Log($"ResourceManager: Getting weapon sounds for weapon \"{baseProperties.name}\"...");

            List<AudioClip> _weaponSounds = new List<AudioClip>();
            string[] _audioFiles = Directory.GetFiles(soundsFolderPath);

            int _currentAudioFileIndex = 0;

            for (int _i = 0; _i < wantedSoundSamples; _i++)
            {
                AudioClip _weaponSound = LoadAudioClip(_audioFiles[_currentAudioFileIndex]);
                _weaponSounds.Add(_weaponSound);

                _currentAudioFileIndex++;
                if (_currentAudioFileIndex >= _audioFiles.Length)
                {
                    _currentAudioFileIndex = 0;
                }
            }

            baseProperties.soundRegisters[soundRegisterIndex].clips = _weaponSounds;

            Plugin.Log($"ResourceManager: Got weapon sounds for weapon \"{baseProperties.name}\".");
            return baseProperties;
        }

        public AudioClip LoadAudioClip(string path)
        {
            StartCoroutine(LoadAudioClipInternal(path));
            return _temporaryAudioClip;
        }

        private IEnumerator LoadAudioClipInternal(string path)
        {
            Plugin.Log($"ResourceManager: Loading sound \"{Path.GetFileNameWithoutExtension(path)}\"...");
            WWW _audioWWW = new WWW(path);

            AudioClip _clip = _audioWWW.GetAudioClip(false, true);

            while (!_audioWWW.isDone) { }

            if (_clip == null || _clip.length == 0)
            {
                Plugin.LogError($"ResourceManager: Could not load sound \"{Path.GetFileNameWithoutExtension(path)}\".");
                yield break;
            }

            _temporaryAudioClip = _clip;
            Plugin.Log($"ResourceManager: Successfully loaded sound \"{Path.GetFileNameWithoutExtension(path)}\".");
        }

        private void LoadHitmarkerTexture()
        {
            Plugin.Log("ResourceManager: Loading hitmarker texture...");

            HitmarkerTexture = new Texture2D(2, 2);

            string _hitmarkerTexturePath = Application.dataPath + GameConfiguration.defaultImagesPath + "UI/hitmarker.png";
            byte[] _hitmarkerTextureBytes = File.ReadAllBytes(_hitmarkerTexturePath);

            HitmarkerTexture.LoadImage(_hitmarkerTextureBytes);
            HitmarkerTexture.Apply();

            if (HitmarkerTexture != null)
            {
                Plugin.Log("ResourceManager: Loaded hitmarker texture.");
            }
            else
            {
                Plugin.LogWarning("ResourceManager: Could not load hitmarker texture.");
            }
        }

        private string[] RecursivelySearchForWeaponProperties(string path)
        {
            List<string> _weaponProperties = new List<string>();
            _weaponProperties.AddRange(Directory.GetFiles(path));

            string[] _folders = Directory.GetDirectories(path);

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

            foreach (string _musicThemeFile in _musicThemePaths)
            {
                AudioClip _theme = LoadAudioClip(_musicThemeFile);
                _themes.Add(_theme);
            }

            _musicThemesAudioClips = _themes.ToArray();
            MainMenu.instance.music.clips = _musicThemesAudioClips;
        }

        private void GetProjectilePrefab()
        {
            ProjectileAssetBundle = AssetBundle.LoadFromFile(Application.dataPath + GameConfiguration.defaultAssetBundlesPath + "tracer");
            _projectilePrefab = (GameObject)ProjectileAssetBundle.LoadAsset("assets/tracer/tracer.prefab");

            List<Material> _materials = new List<Material>();
            _materials.Add((Material)ProjectileAssetBundle.LoadAsset("assets/tracer/yellow tracer.mat"));
            _materials.Add((Material)ProjectileAssetBundle.LoadAsset("assets/tracer/red tracer.mat"));
            _materials.Add((Material)ProjectileAssetBundle.LoadAsset("assets/tracer/green tracer.mat"));
            _materials.Add((Material)ProjectileAssetBundle.LoadAsset("assets/tracer/blue tracer.mat"));

            ProjectileTracerMaterials = _materials.ToArray();
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

        public Weapon.Configuration GetWeaponConfigurationFromProperties(Weapon weapon, WeaponXMLProperties weaponProperties)
        {
            Plugin.Log($"ResourceManager: Getting weapon configuration from properties of \"{weapon.name}\"...");

            if (weapon == null)
            {
                Plugin.LogError("ResourceManager: Cannot get weapon configuration for null weapon.");
                return null;
            }

            if (weaponProperties == null)
            {
                Plugin.LogWarning("ResourceManager: Cannot get weapon configuration for weapon from null weapon properties.");
                return weapon.configuration;
            }

            Weapon.Configuration _baseConfiguration = weapon.configuration;
            WeaponAdditionalData _additionalData = weapon.GetAdditionalData();
            Projectile _projectile = _baseConfiguration.projectile();

            _baseConfiguration.kickback = 0;
            _baseConfiguration.spread = 0;
            _baseConfiguration.randomKick = 0;
            _baseConfiguration.snapMagnitude = 0;
            _baseConfiguration.followupMaxSpreadAim = 0;
            _baseConfiguration.followupMaxSpreadHip = 0;

            if (_projectile.armorDamage != Vehicle.ArmorRating.AntiTank)
            {
                Plugin.Log("ResourceManager: Weapon should receive projectile armor data patch.");

                _projectile.autoAssignArmorDamage = false;
                _projectile.armorDamage = Vehicle.ArmorRating.HeavyArms;
                _baseConfiguration.diffGroundVehicles = Weapon.Difficulty.Easy;

                _projectile.configuration.passThroughPenetrateLayer = true;
                _projectile.configuration.piercing = true;
            }

            if (weaponProperties.projectile.HasParam(WeaponXMLProperties.Projectile.IS_LOUD))
            {
                _baseConfiguration.loud = weaponProperties.projectile.GetBool(WeaponXMLProperties.Projectile.IS_LOUD);
            }

            if (weaponProperties.projectile.HasParam(WeaponXMLProperties.Projectile.DISABLE_REFLECTION_SOUND))
            {
                weapon.GetAdditionalData().disableReflectionSound = weaponProperties.projectile.GetBool(WeaponXMLProperties.Projectile.DISABLE_REFLECTION_SOUND);
            }

            if (weaponProperties.projectile.HasParam(WeaponXMLProperties.Projectile.REFLECTION_SOUND))
            {
                Plugin.Log("ResourceManager: Weapon has custom reflection sound property.");

                string _reflectionSoundToString = weaponProperties.projectile.GetString(WeaponXMLProperties.Projectile.REFLECTION_SOUND);
                Weapon.ReflectionSound _reflectionSound = (Weapon.ReflectionSound)Enum.Parse(typeof(Weapon.ReflectionSound), _reflectionSoundToString); ;

                Plugin.Log($"ResourceManager: Custom reflection sound property = \"{_reflectionSound}\" (from string \"{_reflectionSoundToString}\").");
                weapon.reflectionSound = _reflectionSound;
            }

            if (weaponProperties.damage.HasParam(WeaponXMLProperties.Damage.VEHICLE_DAMAGE))
            {
                _additionalData.damageToVehicles = weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.VEHICLE_DAMAGE);
            }
            else
            {
                _additionalData.damageToVehicles = _projectile.configuration.damageDropOff[0].value;
            }

            int _bulletsCount = weaponProperties.GetInt(WeaponXMLProperties.BULLETS);
            int _reserveMagazinesCount = weaponProperties.GetInt(WeaponXMLProperties.MAGAZINES);

            _baseConfiguration.ammo = _bulletsCount;
            _baseConfiguration.maxAmmoPerReload = _bulletsCount;
            _baseConfiguration.spareAmmo = _reserveMagazinesCount * _bulletsCount;

            _baseConfiguration.aimSensitivityMultiplier = 1;
            _baseConfiguration.forceSniperAimSensitivity = false;

            float _roundsPerMinute = weaponProperties.GetFloat(WeaponXMLProperties.ROUNDS_PER_MINUTE);
            bool _isAdvancedReload = weaponProperties.HasParam(WeaponXMLProperties.IS_ADVANCED_RELOAD);
            bool _useMaxAmmoPerReload = weaponProperties.HasParam(WeaponXMLProperties.USE_MAX_AMMO_PER_RELOAD);
            bool _dropAmmoWhenReloading = weaponProperties.HasParam(WeaponXMLProperties.DROP_AMMO_WHEN_RELOADING);
            float _shortDamage = weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.SHORT_DAMAGE);
            float _shortDistance = weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.SHORT_DISTANCE);
            float _longDamage = weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.LONG_DAMAGE);
            float _longDistance = weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.LONG_DISTANCE);
            float _velocity = weaponProperties.projectile.GetFloat(WeaponXMLProperties.Projectile.VELOCITY);
            float _damageMultiplier = weaponProperties.projectile.GetFloat(WeaponXMLProperties.Projectile.DAMAGE_MULTIPLIER);

            if (_additionalData.modifications == null)
            {
                _additionalData.InitWeaponModifications();

                if (_additionalData.modifications == null)
                {
                    Plugin.LogError($"ResourceManager: Cannot get weapon configuration for weapon with null modifications ({weapon.transform.name})!");
                    return weapon.configuration;
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

            _baseConfiguration.cooldown = 60f / _roundsPerMinute;

            // _projectile.configuration.inheritVelocity = true;
            _projectile.configuration.gravityMultiplier = 1;
            _projectile.configuration.speed = _velocity;

            _baseConfiguration.aimFov = GameConfiguration.defaultAimingFieldOfView;
            weapon.GetProperty<AudioSource>("audio").clip = null;

            _projectile.configuration.damage = _damageMultiplier;
            _projectile.configuration.dropoffEnd = _longDistance;

            List<Keyframe> _damageDropOffKeys = new List<Keyframe>()
            {
                new Keyframe(0, _shortDamage),
                new Keyframe(_longDistance / _shortDistance,  _shortDamage),
                new Keyframe(1,  _longDamage),
            };

            _projectile.configuration.damageDropOff = new AnimationCurve(_damageDropOffKeys.ToArray());

            if (_isAdvancedReload)
                _baseConfiguration.advancedReload = weaponProperties.GetBool(WeaponXMLProperties.IS_ADVANCED_RELOAD);

            if (_useMaxAmmoPerReload)
                _baseConfiguration.useMaxAmmoPerReload = weaponProperties.GetBool(WeaponXMLProperties.USE_MAX_AMMO_PER_RELOAD);

            if (_dropAmmoWhenReloading)
                _baseConfiguration.dropAmmoWhenReloading = weaponProperties.GetBool(WeaponXMLProperties.DROP_AMMO_WHEN_RELOADING);

            _baseConfiguration.projectilePrefab = _projectile.gameObject;

            return _baseConfiguration;
        }

        public Weapon.Configuration GetWeaponConfigurationForBots(Weapon weapon, WeaponXMLProperties weaponProperties)
        {
            if (weapon == null || weapon.configuration == null)
            {
                return weapon.configuration;
            }

            Weapon.Configuration _base = weapon.configuration;
            Projectile _projectile = _base.projectile();

            float _shortDamage = weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.SHORT_DAMAGE);
            float _shortDistance = weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.SHORT_DISTANCE);
            float _longDamage = weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.LONG_DAMAGE);
            float _longDistance = weaponProperties.damage.GetFloat(WeaponXMLProperties.Damage.LONG_DISTANCE);

            _projectile.configuration.damage = weaponProperties.projectile.GetFloat(WeaponXMLProperties.Projectile.DAMAGE_MULTIPLIER);
            _projectile.configuration.dropoffEnd = _longDistance;

            List<Keyframe> _damageDropOffKeys = new List<Keyframe>()
            {
                new Keyframe(0, _shortDamage),
                new Keyframe(_longDistance / _shortDistance,  _shortDamage),
                new Keyframe(1,  _longDamage),
            };

            _projectile.configuration.damageDropOff = new AnimationCurve(_damageDropOffKeys.ToArray());
            _base.projectilePrefab = _projectile.gameObject;

            return _base;
        }

        public List<RegisteredWeaponModifications> GetPossibleWeaponModifications(string weaponName)
        {
            List<RegisteredWeaponModifications> _output = new List<RegisteredWeaponModifications>();

            Plugin.EndLogGroup();

            Plugin.Log($"ResourceManager: Searching for possible weapon modifications for weapon \"{weaponName}\"...");
            Plugin.Log($"ResourceManager: Found {_registeredWeaponModifications.Count} possible weapon modifications:");

            foreach (RegisteredWeaponModifications _modification in _registeredWeaponModifications)
            {
                Plugin.Log($"ResourceManager: * Modification \"{_modification.modification.name}\" applies to = \"{_modification.modification.weaponName}\".");

                if (_modification.modification.weaponName == weaponName)
                {
                    _output.Add(_modification);
                }
            }

            Plugin.EndLogGroup();

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

        public void LoadCaptureJingles()
        {
            Plugin.Log("ResourceManager: Loading capture jingles...");

            string _basePath = Application.dataPath + GameConfiguration.defaultCaptureJinglesPath;

            _captureJingleGermany = LoadAudioClip(_basePath + "capture_jingle_ge.wav");
            _captureJingleUnitedStates = LoadAudioClip(_basePath + "capture_jingle_us.wav");
            _captureJingleSovietUnion = LoadAudioClip(_basePath + "capture_jingle_ru.wav");
            _captureJinglePointLost = LoadAudioClip(_basePath + "capture_jingle_point_lost.wav");
            _captureJinglePointNeutralized = LoadAudioClip(_basePath + "capture_jingle_neutralized.wav");
            gameObject.AddComponent<MusicJingleManager>();

            Plugin.Log("ResourceManager: Loaded capture jingles.");
        }

        public void LoadLoadingScreenAssetBundle()
        {
            Plugin.Log("ResourceManager: Loading loading screen asset bundle...");

            string _loadingScreenPath = Application.dataPath + GameConfiguration.defaultAssetBundlesPath + "loading_screen";
            LoadingScreenAssetBundle = AssetBundle.LoadFromFile(_loadingScreenPath);

            string _loadingScreenPrefabPath = "assets/loading mods menu/loading mods menu.prefab";
            GameObject _loadingScreenPrefab = (GameObject)LoadingScreenAssetBundle.LoadAsset(_loadingScreenPrefabPath);

            _loadingScreen = Instantiate(_loadingScreenPrefab, Instance.transform);
            _loadingScreen.transform.name = "Loading Screen";

            DisableLoadingScreen();

            Plugin.Log("ResourceManager: Loaded loading screen asset bundle.");
        }

        public void LoadBundleGlobalKillfeed()
        {
            Plugin.Log("ResourceManager: Loading global killfeed asset bundle...");

            string _globalKillfeedPath = Application.dataPath + GameConfiguration.defaultAssetBundlesPath + "global_killfeed";
            GlobalKillfeedAssetBundle = AssetBundle.LoadFromFile(_globalKillfeedPath);

            if (GlobalKillfeedAssetBundle == null)
            {
                Plugin.LogError("ResourceManager: Could not load global killfeed asset bundle!");
                return;
            }

            try
            {
                GlobalKillfeedItemPrefab = (GameObject)GlobalKillfeedAssetBundle.LoadAsset("assets/global killfeed/prefabs/global killfeed item.prefab");
                GlobalKillfeedPrefab = (GameObject)GlobalKillfeedAssetBundle.LoadAsset("assets/global killfeed/prefabs/global killfeed.prefab");
            }
            catch (Exception _exception)
            {
                Plugin.LogError("ResourceManager: Could not load global killfeed asset bundle! " + _exception);
                return;
            }

            Plugin.Log("ResourceManager: Loaded global killfeed asset bundle.");
        }

        public void LoadBundleKillfeed()
        {
            Plugin.Log("ResourceManager: Loading killfeed asset bundle...");

            string _killfeedPath = Application.dataPath + GameConfiguration.defaultAssetBundlesPath + "killfeed_ui";
            KillfeedAssetBundle = AssetBundle.LoadFromFile(_killfeedPath);

            Plugin.Log("ResourceManager: Loaded killfeed asset bundle.");
        }

        public void LoadBundlePlayerUI()
        {
            Plugin.Log("ResourceManager: Loading player asset bundle...");

            string _playerUIPath = Application.dataPath + GameConfiguration.defaultAssetBundlesPath + "player_ui";
            PlayerUIAssetBundle = AssetBundle.LoadFromFile(_playerUIPath);

            Plugin.Log("ResourceManager: Loaded player asset bundle.");
        }

        private void RegisterWeaponProperties()
        {
            Plugin.Log("ResourceManager: Registering weapon properties...");

            string _basePath = Application.dataPath + GameConfiguration.defaultWeaponDataPath;

            List<string[]> _weaponPropertiesDirectories = new List<string[]>()
            {
                Directory.GetDirectories(_basePath + "ge/"),
                Directory.GetDirectories(_basePath + "us/"),
                Directory.GetDirectories(_basePath + "ru/"),
                Directory.GetDirectories(_basePath + "neu/"),
            };

            foreach(string[] _weaponPropertiesDirectory in _weaponPropertiesDirectories)
            {
                foreach(string _weaponDirectory in _weaponPropertiesDirectory)
                {
                    UnpackWeaponPropertiesFile(_weaponDirectory);
                }
            }
        }

        private void LoadWeaponPatches()
        {
            string _basePath = Application.dataPath + GameConfiguration.defaultPatchesPath + "Weapons/";
            string[] _weaponPatchFiles = Directory.GetFiles(_basePath);

            foreach (string _weaponPatchFile in _weaponPatchFiles)
            {
                string _weaponPatchFileContent = File.ReadAllText(_weaponPatchFile);
                
                XmlSerializer _serializer = new XmlSerializer(typeof(Patch));
                StringReader _reader = new StringReader(_weaponPatchFileContent);
                Patch _weaponPatch = (Patch)_serializer.Deserialize(_reader);
             
                InterpretWeaponPatch(_weaponPatch);
            }
        }

        private void InterpretWeaponPatch(Patch weaponPatch)
        {
            if (weaponPatch.type == "name")
            {
                string _defaultName = weaponPatch.GetString(Patch.PATCH_NAME_DEFAULT_NAME);
                string _patchedName = weaponPatch.GetString(Patch.PATCH_NAME_PATCHED_NAME);

                WeaponRegistry.RegisterNewRealName(_defaultName, _patchedName);
            }
        }

        public static WeaponXMLProperties GetWeaponProperties(Weapon _weapon)
        {
            if (_weapon == null)
            {
                Plugin.LogError("ResourceManager: Cannot get properties of null weapon!");
                return null;
            }

            foreach (WeaponXMLProperties _properties in ResourceManager.Instance._registeredWeaponProperties)
            {
                if (_properties.name.Equals(_weapon.name))
                {
                    return _properties;
                }
            }

            return null;
        }

        private void LoadWhistleSounds()
        {
            string _basePath = Application.dataPath + GameConfiguration.defaultWhistlePath;
            string[] _whistleSoundFiles = Directory.GetFiles(_basePath);
            List<AudioClip> _whistleSounds = new List<AudioClip>();

            foreach (string _whistleSoundFile in _whistleSoundFiles)
            {
                if (Path.GetExtension(_whistleSoundFile).Contains("wav") == false)
                {
                    continue;
                }

                _whistleSounds.Add(LoadAudioClip(_whistleSoundFile));
            }

            _whistleAudioClips = _whistleSounds.ToArray();
        }

        private void LoadHurtSounds()
        {
            string _basePath = Application.dataPath + GameConfiguration.defaultHurtPath;
            string[] _hurtSoundFiles = Directory.GetFiles(_basePath);
            List<AudioClip> _hurtSounds = new List<AudioClip>();

            foreach (string _hurtSoundFile in _hurtSoundFiles)
            {
                if (Path.GetExtension(_hurtSoundFile).Contains("wav") == false)
                {
                    continue;
                }

                _hurtSounds.Add(LoadAudioClip(_hurtSoundFile));
            }

            _hurtAudioClips = _hurtSounds.ToArray();
        }
    }
}