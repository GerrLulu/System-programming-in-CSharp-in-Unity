using System;
using UnityEngine;
using UnityEngine.Networking;

namespace LessonFour
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class Character : NetworkBehaviour
    {
        [SyncVar] protected Vector3 _serverPosition;
        [SyncVar] protected Quaternion _serverRotation;
        [SyncVar] protected int _serverHealth;

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
        protected void CmdUpdatePosition(Vector3 position, Quaternion rotation)
        {
            _serverPosition = position;
            _serverRotation = rotation;
        }
        
        public abstract void Movement();

        [Command]
        protected void CmdUpdateHealth(int health)
        {
            _serverHealth = health;
        }

        private void OnDestroy()
        {
            _onUpdateAction -= Movement;
        }
    }
}