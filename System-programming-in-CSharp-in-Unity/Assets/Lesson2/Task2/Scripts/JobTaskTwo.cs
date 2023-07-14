using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace LessonTwo.TaskTwo
{
    public class JobTaskTwo : MonoBehaviour
    {
        [SerializeField] private int _sizeArray;
        [SerializeField] private float _min;
        [SerializeField] private float _max;

        private NativeArray<Vector3> _position;
        private NativeArray<Vector3> _velocities;
        private NativeArray<Vector3> _finalPositions;

        private JobHandle _jobHandle;


        private void Start()
        {
            _position = new NativeArray<Vector3>(_sizeArray, Allocator.TempJob);
            _velocities = new NativeArray<Vector3>(_sizeArray, Allocator.TempJob);
            _finalPositions = new NativeArray<Vector3>(_sizeArray, Allocator.TempJob);

            for(int i =0 ; i < _sizeArray; i++)
            {
                _position[i] = new Vector3(Random.Range(_min, _max), Random.Range(_min, _max), Random.Range(_min, _max));
                Debug.Log($"Start position [{i}]: {_position[i]}");

                _velocities[i] = new Vector3(Random.Range(_min, _max), Random.Range(_min, _max), Random.Range(_min, _max));
                Debug.Log($"Velocity [{i}]: {_velocities[i]}");
            }

            TaskTwoLessonTwo taskTwoLessonTwo = new TaskTwoLessonTwo();
            taskTwoLessonTwo.Positions = _position;
            taskTwoLessonTwo.Velocities = _velocities;
            taskTwoLessonTwo.FinalPositions = _finalPositions;

            _jobHandle = taskTwoLessonTwo.Schedule(_sizeArray, 5);
            _jobHandle.Complete();

            PrintResult(taskTwoLessonTwo.FinalPositions);
        }

        private void PrintResult(NativeArray<Vector3> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                Debug.Log($"Result [{i}]: {array[i]}");
            }
        }

        private void OnDestroy()
        {
            _position.Dispose();
            _velocities.Dispose();
            _finalPositions.Dispose();
        }
    }
}