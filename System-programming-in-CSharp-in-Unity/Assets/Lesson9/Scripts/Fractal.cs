using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Lesson9
{
    public class Fractal : MonoBehaviour
    {
        private struct FractalPart
        {
            public Vector3 Direction;
            public Quaternion Rotation;
            public Vector3 WorldPosition;
            public Quaternion WorldRotation;
            public float SpinAngle;
        }

        private struct UpdateFractalLevelJob : IJobFor
        {
            public float SpinAngleDelta;
            public float Scale;

            [ReadOnly]
            public NativeArray<FractalPart> Parents;
            public NativeArray<FractalPart> Parts;

            [WriteOnly]
            public NativeArray<Matrix4x4> Matrices;

            public void Execute(int index)
            {
                FractalPart parent = Parents[index / CHILD_COUNT];
                FractalPart part = Parts[index];

                part.SpinAngle += SpinAngleDelta;
                part.WorldRotation = parent.WorldRotation * (part.Rotation * Quaternion.Euler(0f, part.SpinAngle, 0f));
                part.WorldPosition = parent.WorldPosition + parent.WorldRotation * (POSITION_OFFSET * Scale * part.Direction);
                Parts[index] = part;
                Matrices[index] = Matrix4x4.TRS(part.WorldPosition, part.WorldRotation, Scale * Vector3.one);
            }
        }


        private const float POSITION_OFFSET = 1.5f;
        private const float SCALE_BIAS = .5f;
        private const int CHILD_COUNT = 5;

        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;
        [SerializeField, Range(1, 8)] private int _depth = 4;
        [SerializeField, Range(1, 360)] private int _rotationSpeed;

        //private FractalPart[][] _parts;
        //private Matrix4x4[][] _matrices;
        private ComputeBuffer[] _matricesBuffers;

        private NativeArray<FractalPart>[] _parts;
        private NativeArray<Matrix4x4>[] _matrices;

        private static readonly int _matricesId = Shader.PropertyToID("_Matrices");
        private static MaterialPropertyBlock _propertyBlock;

        private static readonly Vector3[] _directions = new Vector3[]
        {
            Vector3.up,
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.back,
        };
        private static readonly Quaternion[] _rotations = new Quaternion[]
        {
            Quaternion.identity,
            Quaternion.Euler(0f, 0f, 90f),
            Quaternion.Euler(0f, 0f, -90f),
            Quaternion.Euler(90f, 0f, 0f),
            Quaternion.Euler(-90f, 0f, 0f),
        };


        private void OnEnable()
        {
            //_parts = new FractalPart[_depth][];
            //_matrices = new Matrix4x4[_depth][];
            _matricesBuffers = new ComputeBuffer[_depth];
            var stride = 16 * 4;

            for (int i = 0, length = 1; i < _parts.Length; i++, length *= CHILD_COUNT)
            {
                _parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
                _matrices[i] = new NativeArray<Matrix4x4>(length, Allocator.Persistent);
                _matricesBuffers[i] = new ComputeBuffer(length, stride);
            }

            _parts[0][0] = CreatePart(0);
                
            for (var li = 1; li < _parts.Length; li++)
            {
                NativeArray<FractalPart> levelParts = _parts[li];
                for (var fpi = 0; fpi < levelParts.Length; fpi += CHILD_COUNT)
                {
                    for (var ci = 0; ci < CHILD_COUNT; ci++)
                    {
                        levelParts[fpi + ci] = CreatePart(ci);
                    }
                }
            }

            _propertyBlock ??= new MaterialPropertyBlock();
        }

        private void OnDisable()
        {
            for (var i = 0; i < _matricesBuffers.Length; i++)
            {
                _matricesBuffers[i].Release();
                _parts[i].Dispose();
                _matrices[i].Dispose();
            }

            _parts = null;
            _matrices = null;
            _matricesBuffers = null;
        }

        private void OnValidate()
        {
            if (_parts is null || !enabled)
            {
                return;
            }

            OnDisable();
            OnEnable();
        }

        private FractalPart CreatePart(int childIndex) => new FractalPart
        {
            Direction = _directions[childIndex],
            Rotation = _rotations[childIndex],
        };

        private void Update()
        {
            var scale = 1f;
            float spinAngleDelta = _rotationSpeed * Time.deltaTime;

            var rootPart = _parts[0][0];
            rootPart.SpinAngle += spinAngleDelta;
            rootPart.WorldRotation = rootPart.Rotation * Quaternion.Euler(0f, rootPart.SpinAngle, 0f);

            _parts[0][0] = rootPart;
            _matrices[0][0] = Matrix4x4.TRS(rootPart.WorldPosition, rootPart.WorldRotation, scale * Vector3.one);

            JobHandle jobHandle = default;
            for (var li = 1; li < _parts.Length; li++)
            {
                scale *= SCALE_BIAS;

                jobHandle = new UpdateFractalLevelJob
                {
                    SpinAngleDelta = spinAngleDelta,
                    Scale = scale,
                    Parents = _parts[li - 1],
                    Parts = _parts[li],
                    Matrices = _matrices[li]
                }.Schedule(_parts[li].Length, jobHandle);
            }
            jobHandle.Complete();

            var bounds = new Bounds(rootPart.WorldPosition, 3f * Vector3.one);

            for (var i = 0; i < _matricesBuffers.Length; i++)
            {
                ComputeBuffer buffer = _matricesBuffers[i];
                buffer.SetData(_matrices[i]);
                _propertyBlock.SetBuffer(_matricesId, buffer);
                _material.SetBuffer(_matricesId, buffer);
                Graphics.DrawMeshInstancedProcedural(_mesh, 0, _material, bounds, buffer.count, _propertyBlock);
            }
        }
    }
}