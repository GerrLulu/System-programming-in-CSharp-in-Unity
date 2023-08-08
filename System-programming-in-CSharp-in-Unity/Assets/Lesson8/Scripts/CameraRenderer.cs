using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Lesson8
{
    public class CameraRenderer
    {
        private ScriptableRenderContext _context;
        private Camera _camera;

        private CommandBuffer _commandBuffer;
        private const string BUFFER_NAME = "Camera Render";
        private CullingResults _cullingResults;
        private static readonly List<ShaderTagId> _drawingShaderTagId =
            new List<ShaderTagId> { new ShaderTagId("ShaderTag"), };

#if UNITY_EDITOR
        private static readonly ShaderTagId[] _legacyShaderTagIds =
        {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM")
        };

        private static Material _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
#endif

        public void Render(ScriptableRenderContext context, Camera camera)
        {
            _camera = camera;
            _context = context;

            UIGO();
            if (!Cull(out ScriptableCullingParameters parameters))
                return;


            Settings(parameters);
            DrawVisible();
            DrawUnsupportedShaders();
            DrawGizmos();
            Submit();
        }


        private void UIGO()
        {
            if (_camera.cameraType == CameraType.SceneView)
                ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }

        private bool Cull(out ScriptableCullingParameters parameters)
        {
            return _camera.TryGetCullingParameters(out parameters);
        }

        private void Settings(ScriptableCullingParameters parameters)
        {
            _commandBuffer = new CommandBuffer { name = _camera.name };
            _cullingResults = _context.Cull(ref parameters);
            _context.SetupCameraProperties(_camera);
            _commandBuffer.ClearRenderTarget(true, true, Color.clear);
            _commandBuffer.BeginSample(BUFFER_NAME);
            _commandBuffer.SetGlobalColor("_GlobalCal", Color.blue);
            ExecuteCommandBuffer();
        }

        private void DrawVisible()
        {
            //opaque
            DrawingSettings drawingSettings = CreateDrawingSettings(_drawingShaderTagId, SortingCriteria.CommonOpaque,
                out SortingSettings sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
            _context.DrawSkybox(_camera);

            //transpan
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        }

#if UNITY_EDITOR
        private void DrawUnsupportedShaders()
        {
            var drawingSettings = new DrawingSettings(_legacyShaderTagIds[0], new SortingSettings(_camera))
            {
                overrideMaterial = _errorMaterial,
            };

            for(var i = 1; i < _legacyShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, _legacyShaderTagIds[i]);
            }

            var filteringSettings = FilteringSettings.defaultValue;

            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        }

        private void DrawGizmos()
        {
            if(!Handles.ShouldRenderGizmos())
                return;

            _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
        }
#endif

        private void Submit()
        {
            _commandBuffer.EndSample(BUFFER_NAME);
            ExecuteCommandBuffer();
            _context.Submit();
        }

        private void ExecuteCommandBuffer()
        {
            _context.ExecuteCommandBuffer(_commandBuffer);
            _commandBuffer.Clear();
        }

        private DrawingSettings CreateDrawingSettings(List<ShaderTagId> shaderTags, SortingCriteria sortingCriteria,
            out SortingSettings sortingSettings)
        {
            sortingSettings = new SortingSettings(_camera)
            {
                criteria = sortingCriteria,
            };

            var drawingSettings = new DrawingSettings(shaderTags[0], sortingSettings);
            for(var i = 1; i < shaderTags.Count; i++)
            {
                drawingSettings.SetShaderPassName(i , shaderTags[i]);
            }

            return drawingSettings;
        }
    }
}