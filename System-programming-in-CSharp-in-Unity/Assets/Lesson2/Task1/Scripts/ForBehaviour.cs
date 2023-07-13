using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace LessonTwo.TaskOne
{
    public class ForBehaviour : MonoBehaviour
    {
        [SerializeField] private int _sizeArray;
        private NativeArray<int> _array;
        private JobHandle _jobHandle;

        void Start()
        {
            _array = new NativeArray<int>(_sizeArray, Allocator.TempJob);
            for(int i = 0;  i < _sizeArray; i++)
            {
                _array[i] = Random.Range(0, 100);
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