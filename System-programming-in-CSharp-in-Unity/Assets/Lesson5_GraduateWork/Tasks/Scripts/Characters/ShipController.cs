using Main;
using Mechanics;
using Network;
using UI;
using UnityEngine;
using UnityEngine.Networking;
using Data;

namespace Characters
{
    public class ShipController : NetworkMovableObject
    {
        [SerializeField] private Transform _cameraAttach;

        [SyncVar] private string _playerName;

        private CameraOrbit _cameraOrbit;
        private PlayerLabel _playerLabel;
        private float _shipSpeed;
        private Rigidbody _rb;

        public string PlayerName
        {
            get => _playerName;
            set => _playerName = value;
        }
        protected override float _speed => _shipSpeed;

        private void OnGUI()
        {
            if (_cameraOrbit == null)
            {
                return;
            }
            _cameraOrbit.ShowPlayerLabels(_playerLabel);
        }

        public override void OnStartAuthority()
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb == null)
            {
                return;
            }

            gameObject.name = _playerName;
            _cameraOrbit = FindObjectOfType<CameraOrbit>();
            _cameraOrbit.Initiate(_cameraAttach == null ? transform : _cameraAttach);
            _playerLabel = GetComponentInChildren<PlayerLabel>();

            base.OnStartAuthority();
        }

        protected override void HasAuthorityMovement()
        {
            SpaceShipSettings spaceShipSettings = SettingsContainer.Instance?.SpaceShipSettings;
            if (spaceShipSettings == null)
            {
                return;
            }

            bool isFaster = Input.GetKey(KeyCode.LeftShift);
            float speed = spaceShipSettings.ShipSpeed;
            float faster = isFaster ? spaceShipSettings.Faster : 1.0f;
            _shipSpeed = Mathf.Lerp(_shipSpeed, speed * faster, SettingsContainer.Instance.SpaceShipSettings.Acceleration);

            float currentFov = isFaster ? SettingsContainer.Instance.SpaceShipSettings.FasterFov :
                SettingsContainer.Instance.SpaceShipSettings.NormalFov;
            _cameraOrbit.SetFov(currentFov, SettingsContainer.Instance.SpaceShipSettings.ChangeFovSpeed);

            var velocity = _cameraOrbit.transform.TransformDirection(Vector3.forward) * _shipSpeed;
            _rb.velocity = velocity * Time.deltaTime;

            if (!Input.GetKey(KeyCode.C))
            {
                Quaternion targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(_cameraOrbit.LookAngle,
                    -transform.right) * velocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
            }
        }

        protected override void FromServerUpdate() { }
        protected override void SendToServer() { }

        [ClientCallback]
        private void LateUpdate()
        {
            _cameraOrbit?.CameraMovement();
            gameObject.name = _playerName;
        }

        [ServerCallback]
        public void OnTriggerEnter(Collider collider)
        {
            if(hasAuthority)
            {
                Debug.LogAssertion("Столкновение вашего короБЛЯ");
                NetworkManager.singleton.StopClient();

                Destroy(gameObject);
            }
            else
            {
                Debug.LogAssertion("Столкновение вражеского короБЛЯ");
                Destroy(gameObject);
            }

            gameObject.SetActive(false);
            Transform spawnTransform = NetworkManager.singleton.GetStartPosition();
            gameObject.transform.position = spawnTransform.position;
            gameObject.transform.rotation = spawnTransform.rotation;
            gameObject.SetActive(true);
        }
    }
}