using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;

public class DebugViewController : MonoBehaviour
{
    public enum SettingType { Material, Rendering }
    public SettingType settingType = SettingType.Material;

    [Header("Material")]
    [SerializeField] int gBuffer = 0;

    //DebugItemHandlerIntEnum(MaterialDebugSettings.debugViewMaterialGBufferStrings, MaterialDebugSettings.debugViewMaterialGBufferValues)
    [Header("Rendering")]
    [SerializeField] int fullScreenDebugMode = 0;

    [SerializeField] int waitForFrames = 2;

    [ContextMenu("Set Debug View")]
    public void SetDebugView()
    {
        HDRenderPipeline hdPipeline = RenderPipelineManager.currentPipeline as HDRenderPipeline;

        switch ( settingType )
        {
            case SettingType.Material:
                hdPipeline.debugDisplaySettings.SetDebugViewGBuffer(gBuffer);
                hdPipeline.debugDisplaySettings.fullScreenDebugMode = FullScreenDebugMode.None;
                break;
            case SettingType.Rendering:
                hdPipeline.debugDisplaySettings.SetDebugViewGBuffer(0);
                hdPipeline.debugDisplaySettings.fullScreenDebugMode = (FullScreenDebugMode) fullScreenDebugMode;
                break;
        }
    }

    IEnumerator Start()
    {
        for (int i=0 ; i<waitForFrames ; ++i)
            yield return null;

        SetDebugView();
    }
}
