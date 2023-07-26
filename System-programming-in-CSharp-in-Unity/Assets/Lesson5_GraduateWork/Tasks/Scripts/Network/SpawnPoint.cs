using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
#pragma warning disable 618
    public class SpawnPoint : NetworkStartPosition
#pragma warning restore 618
    {
        [SerializeField] private Transform _lookTarget;


        private void OnEnable()
        {
            transform.rotation = Quaternion.LookRotation(_lookTarget.position - transform.position);
        }
    }
}