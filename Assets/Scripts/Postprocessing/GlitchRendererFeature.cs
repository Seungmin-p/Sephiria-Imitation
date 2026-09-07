using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class GlitchRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Shader glitchShader;
    }
    
    public Settings settings = new Settings();
    private GlitchRenderPass glitchPass;
    
    public override void Create()
    {
        glitchPass = new GlitchRenderPass(settings.glitchShader);
        glitchPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.glitchShader == null) return;
        
        var stack = VolumeManager.instance.stack;
        var customEffect = stack.GetComponent<GlitchVolumeComponent>();

        if (customEffect == null || !customEffect.IsActive()) return;
        
        glitchPass.Setup(customEffect);
        renderer.EnqueuePass(glitchPass);
    }

    class GlitchRenderPass : ScriptableRenderPass
    {
        private Material m_Material;
        private GlitchVolumeComponent m_Component;
        
        public GlitchRenderPass(Shader shader)
        {
            if (shader != null)
                m_Material = new Material(shader);
        }

        public void Setup(GlitchVolumeComponent component)
        {
            m_Component = component;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_Material == null || m_Component == null) return;
            
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            
            //원본 화면 가져오기
            TextureHandle source = resourceData.activeColorTexture;
            
            if (source.IsValid() == false) return;
            
            //원본 화면과 동일한 규격의 출력 텍스처 Descriptor 생성
            TextureDesc destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = "Glitch";
            destinationDesc.clearBuffer = false;
            destinationDesc.depthBufferBits = 0; //깊이 버퍼는 후처리에서 사용하지 않는다.
            
            //dest buffer 실제 생성
            TextureHandle destBuffer = renderGraph.CreateTexture(destinationDesc);
            
            //볼륨의 파라미터 적용
            m_Material.SetFloat("_SplitAmount", m_Component.splitAmount.value);
            
            //드로우콜 삽입
            RenderGraphUtils.BlitMaterialParameters blitParams =
                new RenderGraphUtils.BlitMaterialParameters(source, destBuffer, m_Material, 0);
            renderGraph.AddBlitPass(blitParams, passName: "Glitch Render Graph Pass");

            //다음에 진행할 렌더 패스 타겟을 우리의 데스트 버퍼로 치환한다는 의미
            resourceData.cameraColor = destBuffer;
        }
    }
}