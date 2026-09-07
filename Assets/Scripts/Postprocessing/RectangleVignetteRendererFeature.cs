using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

//렌더링 과정에 추가하기 위한 ScriptableRendererFeature 상속
public class RectangleVignetteRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        //인스펙터 상에서 전달받을 전용 셰이더
        public Shader rectangleVignetteShader;
    }
    
    //Settings 및 렌더 패스 선언
    public Settings settings = new Settings();
    private RectangleVignetteRenderPass rectangleVignettePass;
    
    public override void Create()
    {
        //렌더패스 생성 이후, 기존 포스트 프로세싱 실행 전 단계로 순서 지정
        rectangleVignettePass = new RectangleVignetteRenderPass(settings.rectangleVignetteShader);
        rectangleVignettePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    //렌더패스 객체를 실제로 렌더링 작업에 등록해주는 과정
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.rectangleVignetteShader == null) return;
        
        //Volume 시스템에서, 만들어둔 VolumeComponent 설정값을 가져옴
        var stack = VolumeManager.instance.stack;
        var customEffect = stack.GetComponent<RectangleVignetteVolumeComponent>();
        
        //만약 설정에 문제가 있다면 실행하지 않음
        if (customEffect == null || !customEffect.IsActive()) return;
        
        //렌더 패스에 추가 설정을 진행한 이후, 실제 렌더링 과정으로 등록
        rectangleVignettePass.Setup(customEffect);
        renderer.EnqueuePass(rectangleVignettePass);
    }

    class RectangleVignetteRenderPass : ScriptableRenderPass
    {
        private Material m_Material;
        private RectangleVignetteVolumeComponent m_Component;
        
        public RectangleVignetteRenderPass(Shader shader)
        {
            if(shader != null)
                m_Material = new Material(shader);
        }

        public void Setup(RectangleVignetteVolumeComponent component)
        {
            m_Component = component;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if(m_Material == null || m_Component == null) return;
            
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            
            //원본 화면 가져오기
            TextureHandle source = resourceData.activeColorTexture;
            
            if(source.IsValid() == false) return;
            
            //원본 화면 그대로 가져와서 dest buffer용 descriptor 생성
            TextureDesc destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = "RectangleVignette";
            destinationDesc.clearBuffer = false;
            destinationDesc.depthBufferBits = 0; //깊이 버퍼는 후처리에서 사용하지 않는다.
            
            //dest buffer 실제 생성
            TextureHandle destBuffer = renderGraph.CreateTexture(destinationDesc);
            
            //볼륨의 파라미터 적용
            m_Material.SetColor("_Color", m_Component.color.value);
            m_Material.SetFloat("_Intensity", m_Component.intensity.value * 3f);
            m_Material.SetFloat("_Smoothness", m_Component.smoothness.value * 7f);
            
            //드로우콜 삽입
            RenderGraphUtils.BlitMaterialParameters blitParams = 
                new RenderGraphUtils.BlitMaterialParameters(source, destBuffer, m_Material, 0);
            renderGraph.AddBlitPass(blitParams, passName: "RectangleVignette Render Graph Pass");

            //다음에 진행할 렌더 패스 타겟을 우리의 데스트 버퍼로 치환한다는 의미
            resourceData.cameraColor = destBuffer;
        }
    }
}
