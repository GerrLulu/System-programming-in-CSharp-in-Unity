using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class NetworkCameraVisibleChecker : NetworkBehaviour
    {
        [SerializeField] private float _updatePeriod = .1f;
        [SerializeField] private Camera _cam;

        private float _timer;
        private NetworkIdentity _networkIdentity;
        private NetworkIdentity _cameraIdentity;


        private void Start()
        {
            _networkIdentity = GetComponent<NetworkIdentity>();
            _cameraIdentity = _cam.GetComponent<NetworkIdentity>();
        }

        private void Update()
        {
            if (!NetworkServer.active)
                return;

            if (Time.time - _timer > _updatePeriod)
            {
                _cam ??= Camera.current;
                _cameraIdentity ??= _cam.GetComponent<NetworkIdentity>();

                _networkIdentity.RebuildObservers(false);
            }
        }

        //public override bool OnCheckObserver(NetworkConnection conn)
        //{
        //    return _cam.Visible(GetComponent<Collider>());
        //}

        public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
        {
            return false;
        }

        public override void OnSetLocalVisibility(bool vis)
        {
            SetVis(gameObject, vis);
        }

        private static void SetVis(GameObject gameObject, bool vis)
        {
            foreach(Renderer r in gameObject.GetComponents<Renderer>())
                r.enabled = vis;

            for(int i = 0; i < gameObject.transform.childCount; i++)
            {
                Transform t = gameObject.transform.GetChild(i);
                SetVis(t.gameObject, vis);
            }
        }
    }
}