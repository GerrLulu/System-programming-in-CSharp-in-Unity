using System.Collections;
using UnityEngine;

namespace LessonOne.TaskOne
{
    public class Unit : MonoBehaviour
    {
        [SerializeField] private int _currentHealth;
        [SerializeField] private int _maxHealth;
        [SerializeField] private int _healingPerSecond;

        private float _timeHealing = 0;
        [SerializeField] private float _perSecond;
        [SerializeField] private float _maxTimeHealing;


        private void Start()
        {
            Debug.Log($"Unit health = {_currentHealth}");

            StartCoroutine(UnitHealing());
        }

        private void ReceiveHealing(int healingPerSecond)
        {
            _currentHealth += healingPerSecond;
            Debug.Log($"Unit have health = {_currentHealth}");

        }

        private IEnumerator UnitHealing()
        {
            while (!(_maxTimeHealing <= _timeHealing))
            {
                if (!(_maxHealth <= _currentHealth))
                {
                    yield return new WaitForSeconds(_perSecond);
                    _timeHealing += _perSecond;
                    ReceiveHealing(_healingPerSecond);
                }
                else
                {
                    _currentHealth = _maxHealth;
                    Debug.Log($"Unit have max health = {_maxHealth}");
                    _timeHealing = 0;
                    yield break;
                }
            }
            yield break;
        }
    }
}