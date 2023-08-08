using UnityEngine;
using UnityEngine.Rendering;

namespace Lesson8.CustomRenderPipeline
{
    public class SpaceRunPipelineRender : RenderPipeline
    {
        private CameraRenderer _cameraRenderer;


        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            _cameraRenderer = new CameraRenderer();

            CamerasRender(context, cameras);
        }

        private void CamerasRender(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (Camera camera in cameras)
            {
                _cameraRenderer.Render(context, camera);
            }
        }
    }
}