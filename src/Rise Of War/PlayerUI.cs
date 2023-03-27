
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiseOfWar
{
    public class PlayerUI : MonoBehaviour
    {
        public static PlayerUI instance { get; private set; }

        #region References

        // WeaponInfo

        private Transform _weaponInfoContainer;

        private TMP_Text _customDisplayNameText;
        private TMP_Text _currentAmmoText;
        private TMP_Text _currentAmmoInReserveText;
        private TMP_Text _currentFireModeText;
        private TMP_Text _currentFireRangeText;

        private Image _bulletAttachmentImage;
        private Image _sightsAttachmentImage;
        private Image _triggerAttachmentImage;
        private Image _springAttachmentImage;
        private Image _boltAttachmentImage;

        // Player

        private Transform _playerInfoContainer;
        private TMP_Text _playerUsernameText;
        private Slider _playerStaminaSlider;
        private Image _playerStaminaIconImage;
        private CanvasGroup _playerStaminaSliderCanvasGroup;
        private Image[] _healthImages;

        #endregion

        #region Public Methods

        public void SetPlayerUsername(string _username)
        {
            _playerUsernameText.text = _username;
        }

        public void SetHealthAmount(float _amount)
        {
            int _count = Mathf.RoundToInt(_amount / 10 * 2);

            for (int _i = 0; _i < _healthImages.Length; _i++)
            {
                _healthImages[_i].color = (_i < _count) ? new Color32(255, 255, 255, 255) : GameConfiguration.playerUIAttachmentInactiveColor;
            }
        }

        public void SetPlayerStaminaAmount(float _stamina)
        {
            _playerStaminaSlider.maxValue = GameConfiguration.playerMaxStamina;
            _playerStaminaSlider.minValue = 0;

            float _lastStamina = _playerStaminaSlider.value;
            _playerStaminaSlider.value = _stamina;

            float _alpha = (_stamina / GameConfiguration.playerMaxStamina) * 6;
            _playerStaminaSliderCanvasGroup.alpha = _alpha;
            _playerStaminaIconImage.color = new Color(_playerStaminaIconImage.color.r, _playerStaminaIconImage.color.g, _playerStaminaIconImage.color.b, _alpha);
        }

        public Image GetAttachmentImage(WeaponModificationType _weaponAttachment)
        {
            switch (_weaponAttachment)
            {
                case WeaponModificationType.Bullet: return _bulletAttachmentImage;
                case WeaponModificationType.Sights: return _sightsAttachmentImage;
                case WeaponModificationType.Trigger: return _triggerAttachmentImage;
                case WeaponModificationType.Spring: return _springAttachmentImage;
                case WeaponModificationType.Barrel: return _boltAttachmentImage;
                default: return _bulletAttachmentImage;
            }
        }

        public void SetWeaponModificationActive(WeaponModificationType _weaponAttachment, bool _active)
        {
            Image _attachment = GetAttachmentImage(_weaponAttachment);
            _attachment.color = (_active) ?
                GameConfiguration.playerUIAttachmentActiveColor :
                GameConfiguration.playerUIAttachmentInactiveColor;
        }

        public void SetAllWeaponModificationsActive(bool active)
        {
            SetWeaponModificationActive(WeaponModificationType.Bullet, active);
            SetWeaponModificationActive(WeaponModificationType.Sights, active);
            SetWeaponModificationActive(WeaponModificationType.Trigger, active);
            SetWeaponModificationActive(WeaponModificationType.Spring, active);
            SetWeaponModificationActive(WeaponModificationType.Barrel, active);
        }

        private string FillNumberWithZero(string _base, int _zeroAmount)
        {
            string _text = _base.ToString();

            if (_text == "0" || _text == "00" || _text == "000")
            {
                _text = $"<#{GameConfiguration.playerUICurrentAmmoZeroColor}>000</color>";
            }

            int _numbersAmount = 0;
            while (_text.Length < _zeroAmount)
            {
                _numbersAmount++;
                _text = _text.Insert(0, $"0");
            }
            _text = _text.Insert(_numbersAmount, "</color>");
            _text = _text.Insert(0, $"<#{GameConfiguration.playerUICurrentAmmoZeroColor}>");

            return _text;
        }

        public void SetCurrentBulletAmount(int _amount)
        {
            _currentAmmoText.text = FillNumberWithZero(_amount.ToString(), 3);
        }

        public void SetCurrentAmmoInReserveAmount(int _amount)
        {
            _currentAmmoInReserveText.text = FillNumberWithZero(_amount.ToString(), 3);
        }

        public void SetWeaponFireMode(WeaponFireMode _fireMode)
        {
            _currentFireModeText.text = _fireMode.ToString().ToUpper();
        }

        public void SetWeaponFireRange(int _rangeInMeters)
        {
            _currentFireRangeText.text = _rangeInMeters.ToString() + "m";
        }

        public void SetWeaponCustomDisplayName(string _customDisplayName)
        {
            _customDisplayNameText.text = $"\"{_customDisplayName}\"";
        }

        public void HideCustomDisplayName()
        {
            _customDisplayNameText.text = "";
        }

        #endregion

        public void Awake()
        {
            GetSingleton();
        }

        private void Start()
        {
            GetReferences();

            SetPlayerUsername(ActorManager.instance.player.name);
        }

        private void Update()
        {
            if (_playerStaminaSlider.value <= 0)
            {
                _playerStaminaSliderCanvasGroup.alpha = Mathf.Lerp(_playerStaminaSliderCanvasGroup.alpha, 0, Time.deltaTime * 3f);
            }
            else
            {
                _playerStaminaSliderCanvasGroup.alpha = Mathf.Lerp(_playerStaminaSliderCanvasGroup.alpha, 1, Time.deltaTime * 3f);
            }
        }

        private void GetSingleton()
        {
            instance = this;
        }

        private void GetReferences()
        {
            _weaponInfoContainer = transform.Find("Weapon Info");
            _customDisplayNameText = _weaponInfoContainer.Find("Custom Display Name").GetComponent<TMP_Text>();
            _currentAmmoText = _weaponInfoContainer.Find("Middle/Magazine/Current Ammo").GetComponent<TMP_Text>();
            _currentAmmoInReserveText = _weaponInfoContainer.Find("Middle/Magazine/Reserve Ammo").GetComponent<TMP_Text>();
            _currentFireModeText = _weaponInfoContainer.Find("Middle/Info/Fire Mode").GetComponent<TMP_Text>();
            _currentFireRangeText = _weaponInfoContainer.Find("Middle/Info/Shoot Distance").GetComponent<TMP_Text>();
            _bulletAttachmentImage = _weaponInfoContainer.Find("Attachments/List/Bullet").GetComponent<Image>();
            _sightsAttachmentImage = _weaponInfoContainer.Find("Attachments/List/Scope").GetComponent<Image>();
            _triggerAttachmentImage = _weaponInfoContainer.Find("Attachments/List/Trigger").GetComponent<Image>();
            _springAttachmentImage = _weaponInfoContainer.Find("Attachments/List/Spring").GetComponent<Image>();
            _boltAttachmentImage = _weaponInfoContainer.Find("Attachments/List/Bolt").GetComponent<Image>();
            _playerInfoContainer = transform.Find("Player");
            _playerUsernameText = _playerInfoContainer.Find("Username/Text").GetComponent<TMP_Text>();
            _playerStaminaSlider = _playerInfoContainer.Find("Stamina/Slider").GetComponent<Slider>();
            _playerStaminaIconImage = _playerInfoContainer.Find("Stamina/Icon").GetComponent<Image>();
            _playerStaminaSliderCanvasGroup = _playerStaminaSlider.gameObject.AddComponent<CanvasGroup>();
            _healthImages = new Image[]
            {
                _playerInfoContainer.Find("Health/Health Bars/0").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/1").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/2").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/3").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/4").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/5").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/6").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/7").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/8").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/9").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/10").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/11").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/12").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/13").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/14").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/15").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/16").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/17").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/18").GetComponent<Image>(),
                _playerInfoContainer.Find("Health/Health Bars/19").GetComponent<Image>(),
            };
        }
    }
}