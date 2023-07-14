using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace LessonTwo.TaskOne
{
    public class ForMonoBehaviour : MonoBehaviour
    {
        [SerializeField] private int _sizeArray;
        [SerializeField] private int _min;
        [SerializeField] private int _max;

        private NativeArray<int> _array;
        private JobHandle _jobHandle;


        private void Start()
        {
            _array = new NativeArray<int>(_sizeArray, Allocator.TempJob);
            for(int i = 0;  i < _sizeArray; i++)
            {
                _array[i] = Random.Range(_min, _max);
                Debug.Log($"Befor [{i}] = {_array[i]}");
            }


            TaskOneLessonTwo taskOneLessonTwo = new TaskOneLessonTwo();
            taskOneLessonTwo.Array = _array;

            _jobHandle = taskOneLessonTwo.Schedule();
            _jobHandle.Complete();

            PrintResult(taskOneLessonTwo.Array);
            _array.Dispose();
        }

        private void PrintResult(NativeArray<int> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                Debug.Log($"After [{i}] = {array[i]}");
            }
        }
    }
}