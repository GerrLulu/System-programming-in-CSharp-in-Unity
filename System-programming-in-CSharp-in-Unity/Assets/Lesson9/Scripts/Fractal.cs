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


        private const float POSITION_OFFSET = 1.5f;
        private const float SCALE_BIAS = .5f;
        private const int CHILD_COUNT = 5;

        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;
        [SerializeField, Range(1, 8)] private int _depth = 4;
        [SerializeField, Range(1, 360)] private int _rotationSpeed;

        private FractalPart[][] _parts;
        private Matrix4x4[][] _matrices;
        private ComputeBuffer[] _matricesBuffers;

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
            _parts = new FractalPart[_depth][];
            _matrices = new Matrix4x4[_depth][];
            _matricesBuffers = new ComputeBuffer[_depth];
            var stride = 16 * 4;

            for (int i = 0, length = 1; i < _parts.Length; i++, length *= CHILD_COUNT)
            {
                _parts[i] = new FractalPart[length];
                _matrices[i] = new Matrix4x4[length];
                _matricesBuffers[i] = new ComputeBuffer(length, stride);
            }

            _parts[0][0] = CreatePart(0);
                
            for (var li = 1; li < _parts.Length; li++)
            {
                FractalPart[] levelParts = _parts[li];
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

            FractalPart rootPart = _parts[0][0];
            rootPart.SpinAngle += spinAngleDelta;
            rootPart.WorldRotation = rootPart.Rotation * Quaternion.Euler(0f, rootPart.SpinAngle, 0f);

            _parts[0][0] = rootPart;
            _matrices[0][0] = Matrix4x4.TRS(rootPart.WorldPosition, rootPart.WorldRotation, scale * Vector3.one);

            for (var li = 1; li < _parts.Length; li++)
            {
                scale *= SCALE_BIAS;
                FractalPart[] parentParts = _parts[li - 1];
                FractalPart[] levelParts = _parts[li];
                Matrix4x4[] levelMatrices = _matrices[li];
                for (var fpi = 0; fpi < levelParts.Length; fpi++)
                {
                    FractalPart parent = parentParts[fpi / CHILD_COUNT];
                    FractalPart part = levelParts[fpi];
                    part.SpinAngle += spinAngleDelta;
                    part.WorldRotation = parent.WorldRotation * (part.Rotation * Quaternion.Euler(0f, part.SpinAngle, 0f));
                    part.WorldPosition = parent.WorldPosition + parent.WorldRotation * (POSITION_OFFSET * scale * part.Direction);
                    levelParts[fpi] = part;
                    levelMatrices[fpi] = Matrix4x4.TRS(part.WorldPosition, part.WorldRotation, scale * Vector3.one);
                }
            }

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