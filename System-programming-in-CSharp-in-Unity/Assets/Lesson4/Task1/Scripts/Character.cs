using System;
using UnityEngine;
using UnityEngine.Networking;

namespace LessonFour
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class Character : NetworkBehaviour
    {
        [SyncVar] protected Vector3 _serverPosition;

        protected Action _onUpdateAction { get; set; }
        protected abstract FireAction _fireAction { get; set; }


        protected virtual void Initiate()
        {
            _onUpdateAction += Movement;
        }

        private void Update()
        {
            OnUpdate();
        }

        private void OnUpdate()
        {
            _onUpdateAction?.Invoke();
        }

        [Command]
        protected void CmdUpdatePosition(Vector3 position)
        {
            _serverPosition = position;
        }
        
        public abstract void Movement();
    }
}