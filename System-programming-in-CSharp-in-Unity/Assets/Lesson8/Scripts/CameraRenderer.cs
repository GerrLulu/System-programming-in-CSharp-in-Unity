using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Lesson8
{
    public class CameraRenderer
    {
        private ScriptableRenderContext _context;
        private Camera _camera;

        private readonly CommandBuffer _commandBuffer = new CommandBuffer{ name = bufferName };
        private const string bufferName = "Camera Render";


        public void Render(ScriptableRenderContext context, Camera camera)
        {
            _camera = camera;
            _context = context;

            DrawVisible();
            Settings();
            Submit();
        }

        private void DrawVisible()
        {
            _context.SetupCameraProperties(_camera);
            _commandBuffer.ClearRenderTarget(true, true, Color.clear);
            _commandBuffer.BeginSample(bufferName);
            ExecuteCommandBuffer();
            _context.DrawSkybox(_camera);
        }

        private void Settings()
        {
            _context.SetupCameraProperties(_camera);
        }

        private void Submit()
        {
            _commandBuffer.EndSample(bufferName);
            ExecuteCommandBuffer();
            _context.Submit();
        }

        private void ExecuteCommandBuffer()
        {
            _context.ExecuteCommandBuffer(_commandBuffer);
            _commandBuffer.Clear();
        }
    }
}