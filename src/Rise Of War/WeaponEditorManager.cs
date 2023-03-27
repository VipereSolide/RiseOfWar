using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiseOfWar
{
    public class WeaponEditorManager : MonoBehaviour
    {
        public static WeaponEditorManager Instance { get; private set; }

        private Transform _modelProperties;

        private Transform _positionFields;
        private TMP_Text _positionFieldsX;
        private TMP_Text _positionFieldsY;
        private TMP_Text _positionFieldsZ;

        private Transform _rotationFields;
        private TMP_Text _rotationFieldsX;
        private TMP_Text _rotationFieldsY;
        private TMP_Text _rotationFieldsZ;

        private Transform _visualRecoilFields;
        private TMP_Text _visualRecoilFieldsX;
        private TMP_Text _visualRecoilFieldsY;
        private TMP_Text _visualRecoilFieldsZ;

        private Transform _visualRotationalRecoilFields;
        private TMP_Text _visualRotationalRecoilFieldsX;
        private TMP_Text _visualRotationalRecoilFieldsY;
        private TMP_Text _visualRotationalRecoilFieldsZ;

        private GameObject _modeAimingBackground;
        private GameObject _modeRecoilBackground;
        private GameObject _toolMoveBackground;
        private GameObject _toolRotateBackground;

        private Button _modeAimingButton;
        private Button _modeRecoilButton;
        private Button _toolMoveButton;
        private Button _toolRotateButton;

        private Slider _scrollSensitivitySlider;
        private CanvasGroup _canvasGroup;

        public Configuration configuration = new Configuration();

        private int _currentMode = 0;
        private int _currentTool = 0;
        private float _currentSpeedMultiplier = 1;

        private bool _isActive = false;
        private bool _isVisible = false;

        public Weapon activeWeapon;

        public bool isActive { get { return _isActive; } }

        public void Disable()
        {
            _isActive = false;
            activeWeapon = null;

            UpdateState();
        }

        public void Enable(Weapon _weapon)
        {
            _isActive = true;
            activeWeapon = _weapon;

            UpdateState();
        }

        public void SetVisible(bool value)
        {
            _isVisible = value;
            UpdateState();
        }

        private void UpdateState()
        {
            _canvasGroup.SetActive(isActive && _isVisible);
        }

        public Vector3 AimingPosition
        {
            get
            {
                float _x = MathHelper.FloatHelper.Parse(_positionFieldsX.text);
                float _y = MathHelper.FloatHelper.Parse(_positionFieldsY.text);
                float _z = MathHelper.FloatHelper.Parse(_positionFieldsZ.text);

                return new Vector3(_x, _y, _z);
            }
            set
            {
                SetAimingPosition(value);
            }
        }

        public void SetAimingPosition(Vector3 _position)
        {
            _positionFieldsX.text = _position.x.ToString();
            _positionFieldsY.text = _position.y.ToString();
            _positionFieldsZ.text = _position.z.ToString();
        }

        public Vector3 AimingEulerAngles
        {
            get
            {
                float _x = MathHelper.FloatHelper.Parse(_rotationFieldsX.text);
                float _y = MathHelper.FloatHelper.Parse(_rotationFieldsY.text);
                float _z = MathHelper.FloatHelper.Parse(_rotationFieldsZ.text);

                return new Vector3(_x, _y, _z);
            }
            set
            {
                SetAimingEulerAngles(value);
            }
        }

        public void SetAimingEulerAngles(Vector3 _rotation)
        {
            _rotationFieldsX.text = _rotation.x.ToString();
            _rotationFieldsY.text = _rotation.y.ToString();
            _rotationFieldsZ.text = _rotation.z.ToString();
        }

        public Vector3 VisualRecoilPosition
        {
            get
            {
                float _x = MathHelper.FloatHelper.Parse(_visualRecoilFieldsX.text);
                float _y = MathHelper.FloatHelper.Parse(_visualRecoilFieldsY.text);
                float _z = MathHelper.FloatHelper.Parse(_visualRecoilFieldsZ.text);

                return new Vector3(_x, _y, _z);
            }
            set
            {
                SetVisualRecoilPosition(value);
            }
        }

        public void SetVisualRecoilPosition(Vector3 _visualRecoil)
        {
            _visualRecoilFieldsX.text = _visualRecoil.x.ToString();
            _visualRecoilFieldsY.text = _visualRecoil.y.ToString();
            _visualRecoilFieldsZ.text = _visualRecoil.z.ToString();
        }

        public Vector3 VisualRecoilEulerAngles
        {
            get
            {
                float _x = MathHelper.FloatHelper.Parse(_visualRotationalRecoilFieldsX.text);
                float _y = MathHelper.FloatHelper.Parse(_visualRotationalRecoilFieldsY.text);
                float _z = MathHelper.FloatHelper.Parse(_visualRotationalRecoilFieldsZ.text);

                return new Vector3(_x, _y, _z);
            }
            set
            {
                SetVisualRecoilEulerAngles(value);
            }
        }

        public void SetVisualRecoilEulerAngles(Vector3 _visualRotationalRecoil)
        {
            _visualRotationalRecoilFieldsX.text = _visualRotationalRecoil.x.ToString();
            _visualRotationalRecoilFieldsY.text = _visualRotationalRecoil.y.ToString();
            _visualRotationalRecoilFieldsZ.text = _visualRotationalRecoil.z.ToString();
        }

        public void SetCurrentMode(int currentMode)
        {
            _currentMode = currentMode;
        }

        public void SetCurrentTool(int currentTool)
        {
            _currentTool = currentTool;
        }

        public void Awake()
        {
            Instance = this;

            _canvasGroup = this.GetOrCreateComponent<CanvasGroup>();
            SetVisible(false);

            Start();
        }

        private void Start()
        {
            GetReferences();
            SetupButtonEvents();
        }

        private void ToggleVisibleState()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.W))
            {
                SetVisible(!_isVisible);
            }
        }

        public void Update()
        {
            ToggleVisibleState();

            if (!_isVisible)
            {
                return;
            }

            if (Input.GetKey(KeyCode.Keypad1))
            {
                SetCurrentMode(0);
            }

            if (Input.GetKey(KeyCode.Keypad2))
            {
                SetCurrentMode(1);
            }

            if (Input.GetKey(KeyCode.Keypad4))
            {
                SetCurrentTool(0);
            }

            if (Input.GetKey(KeyCode.Keypad5))
            {
                SetCurrentTool(1);
            }

            UpdateModeVisuals();
            UpdateToolsVisuals();
            UpdateScrollSensitivity();
            UpdateScrollSensitivitySlider();
            UpdateWeaponProperties();
            UpdateInputTexts();
        }

        private void GetReferences()
        {
            _modelProperties = transform.Find("Canvas").Find("Weapon Editor").Find("Model Properties");
            _positionFields = _modelProperties.Find("List").Find("Position Fields");
            _rotationFields = _modelProperties.Find("List").Find("Rotation Fields");
            _visualRecoilFields = _modelProperties.Find("List").Find("Visual Recoil Fields");
            _visualRotationalRecoilFields = _modelProperties.Find("List").Find("Visual Rotational Recoil Fields");
            _positionFieldsX = _positionFields.Find("Background").Find("Fields").Find("X").Find("Content").GetComponent<TMP_Text>();
            _positionFieldsY = _positionFields.Find("Background").Find("Fields").Find("Y").Find("Content").GetComponent<TMP_Text>();
            _positionFieldsZ = _positionFields.Find("Background").Find("Fields").Find("Z").Find("Content").GetComponent<TMP_Text>();
            _rotationFieldsX = _rotationFields.Find("Background").Find("Fields").Find("X").Find("Content").GetComponent<TMP_Text>();
            _rotationFieldsY = _rotationFields.Find("Background").Find("Fields").Find("Y").Find("Content").GetComponent<TMP_Text>();
            _rotationFieldsZ = _rotationFields.Find("Background").Find("Fields").Find("Z").Find("Content").GetComponent<TMP_Text>();
            _visualRecoilFieldsX = _visualRecoilFields.Find("Background").Find("Fields").Find("X").Find("Content").GetComponent<TMP_Text>();
            _visualRecoilFieldsY = _visualRecoilFields.Find("Background").Find("Fields").Find("Y").Find("Content").GetComponent<TMP_Text>();
            _visualRecoilFieldsZ = _visualRecoilFields.Find("Background").Find("Fields").Find("Z").Find("Content").GetComponent<TMP_Text>();
            _visualRotationalRecoilFieldsX = _visualRotationalRecoilFields.Find("Background").Find("Fields").Find("X").Find("Content").GetComponent<TMP_Text>();
            _visualRotationalRecoilFieldsY = _visualRotationalRecoilFields.Find("Background").Find("Fields").Find("Y").Find("Content").GetComponent<TMP_Text>();
            _visualRotationalRecoilFieldsZ = _visualRotationalRecoilFields.Find("Background").Find("Fields").Find("Z").Find("Content").GetComponent<TMP_Text>();
            _toolMoveBackground = transform.Find("Canvas").Find("Weapon Editor").Find("Tools").Find("List").Find("Move").Find("Background").gameObject;
            _toolRotateBackground = transform.Find("Canvas").Find("Weapon Editor").Find("Tools").Find("List").Find("Rotate").Find("Background").gameObject;
            _modeAimingBackground = transform.Find("Canvas").Find("Weapon Editor").Find("Mode").Find("List").Find("Aiming Properties").Find("Background").gameObject;
            _modeRecoilBackground = transform.Find("Canvas").Find("Weapon Editor").Find("Mode").Find("List").Find("Recoil Properties").Find("Background").gameObject;
            _toolMoveButton = transform.Find("Canvas").Find("Weapon Editor").Find("Tools").Find("List").Find("Move").GetComponent<Button>();
            _toolRotateButton = transform.Find("Canvas").Find("Weapon Editor").Find("Tools").Find("List").Find("Rotate").GetComponent<Button>();
            _modeAimingButton = transform.Find("Canvas").Find("Weapon Editor").Find("Mode").Find("List").Find("Aiming Properties").GetComponent<Button>();
            _modeRecoilButton = transform.Find("Canvas").Find("Weapon Editor").Find("Mode").Find("List").Find("Recoil Properties").GetComponent<Button>();
            _scrollSensitivitySlider = transform.Find("Canvas").Find("Weapon Editor").Find("Scroll Sensitivity").Find("Slider").GetComponent<Slider>();
        }

        private void UpdateModeVisuals()
        {
            if (_currentMode == 0)
            {
                if (!_modeAimingBackground.activeSelf)
                {
                    _modeAimingBackground.SetActive(true);
                }

                if (_modeRecoilBackground.activeSelf)
                {
                    _modeRecoilBackground.SetActive(false);
                }
            }

            if (_currentMode == 1)
            {
                if (_modeAimingBackground.activeSelf)
                {
                    _modeAimingBackground.SetActive(false);
                }

                if (!_modeRecoilBackground.activeSelf)
                {
                    _modeRecoilBackground.SetActive(true);
                }
            }
        }

        private void UpdateToolsVisuals()
        {
            if (_currentTool == 0)
            {
                if (!_toolMoveBackground.activeSelf)
                {
                    _toolMoveBackground.SetActive(true);
                }

                if (_toolRotateBackground.activeSelf)
                {
                    _toolRotateBackground.SetActive(false);
                }
            }

            if (_currentTool == 1)
            {
                if (_toolMoveBackground.activeSelf)
                {
                    _toolMoveBackground.SetActive(false);
                }

                if (!_toolRotateBackground.activeSelf)
                {
                    _toolRotateBackground.SetActive(true);
                }
            }
        }

        private void UpdateScrollSensitivitySlider()
        {
            _scrollSensitivitySlider.maxValue = 1;
            _scrollSensitivitySlider.minValue = 0;
            _scrollSensitivitySlider.value = _currentSpeedMultiplier;
        }

        private void UpdateScrollSensitivity()
        {
            if (Input.GetKey(KeyCode.KeypadMinus))
            {
                _currentSpeedMultiplier -= Time.deltaTime * configuration.scrollSensitivity;
            }

            if (Input.GetKey(KeyCode.KeypadPlus))
            {
                _currentSpeedMultiplier += Time.deltaTime * configuration.scrollSensitivity;
            }

            _currentSpeedMultiplier += Input.GetAxisRaw("Mouse ScrollWheel") * Time.deltaTime * configuration.scrollSensitivity;
            _currentSpeedMultiplier = Mathf.Clamp(_currentSpeedMultiplier, 0.01f, 1);
        }

        private void UpdateWeaponProperties()
        {
            Vector3 _target = new Vector3(0, 0, 0);
            float _speed = configuration.sensitivity * _currentSpeedMultiplier;

            float _inputX = 0;
            float _inputY = 0;
            float _inputZ = 0;

            if (Input.GetKey(KeyCode.RightArrow)) _inputX += 1;
            if (Input.GetKey(KeyCode.LeftArrow)) _inputX -= 1;

            if (Input.GetKey(KeyCode.UpArrow)) _inputZ += 1;
            if (Input.GetKey(KeyCode.DownArrow)) _inputZ -= 1;

            if (Input.GetKey(KeyCode.RightShift)) _inputY += 1;
            if (Input.GetKey(KeyCode.RightControl)) _inputY -= 1;

            _inputX = Mathf.Clamp(_inputX, -1, 1);
            _inputY = Mathf.Clamp(_inputY, -1, 1);
            _inputZ = Mathf.Clamp(_inputZ, -1, 1);

            _target += new Vector3(_inputX, _inputY, _inputZ) * _speed * Time.deltaTime;

            if (_currentMode == 0)
            {
                if (_currentTool == 0)
                {
                    Vector3 _base = activeWeapon.weaponProperties().aiming.GetVector3(WeaponXMLProperties.Aiming.POSITION);
                    activeWeapon.weaponProperties().aiming.SetVector3(WeaponXMLProperties.Aiming.POSITION, _base + _target);
                }
                else if (_currentTool == 1)
                {
                    Vector3 _base = activeWeapon.weaponProperties().aiming.GetVector3(WeaponXMLProperties.Aiming.ROTATION);
                    activeWeapon.weaponProperties().aiming.SetVector3(WeaponXMLProperties.Aiming.ROTATION, _base + _target);
                }
            }
            else if (_currentMode == 1)
            {
                if (_currentTool == 0)
                {
                    Vector3 _base = activeWeapon.weaponProperties().visualRecoil.GetVector3(WeaponXMLProperties.VisualRecoil.POSITION);
                    activeWeapon.weaponProperties().visualRecoil.SetVector3(WeaponXMLProperties.VisualRecoil.POSITION, _base + _target);
                }
                else if (_currentTool == 1)
                {
                    Vector3 _base = activeWeapon.weaponProperties().visualRecoil.GetVector3(WeaponXMLProperties.VisualRecoil.ROTATION);
                    activeWeapon.weaponProperties().visualRecoil.SetVector3(WeaponXMLProperties.VisualRecoil.ROTATION, _base + _target);
                }
            }

            ResourceManager.Instance.WriteChangesToFile(activeWeapon.weaponProperties());
        }

        private void UpdateInputTexts()
        {
            SetAimingPosition(activeWeapon.weaponProperties().aiming.GetVector3(WeaponXMLProperties.Aiming.POSITION));
            SetAimingEulerAngles(activeWeapon.weaponProperties().aiming.GetVector3(WeaponXMLProperties.Aiming.ROTATION));
            SetVisualRecoilPosition(activeWeapon.weaponProperties().visualRecoil.GetVector3(WeaponXMLProperties.VisualRecoil.POSITION));
            SetVisualRecoilEulerAngles(activeWeapon.weaponProperties().visualRecoil.GetVector3(WeaponXMLProperties.VisualRecoil.ROTATION));
        }

        private void SetupButtonEvents()
        {
            _toolMoveButton.onClick.AddListener(() => { SetCurrentTool(0); });
            _toolRotateButton.onClick.AddListener(() => { SetCurrentTool(1); });
            _modeAimingButton.onClick.AddListener(() => { SetCurrentMode(0); });
            _modeRecoilButton.onClick.AddListener(() => { SetCurrentTool(1); });
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(100, 100, 100, 100), $"Is Visible = {_isVisible}\nIs Active = {_isActive}\nIs Weapon Null = {activeWeapon == null}");
        }

        [System.Serializable]
        public class Configuration
        {
            public float sensitivity = 1;
            public float scrollSensitivity = 10;
        }
    }
}