using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class WeatherFX : MonoBehaviour
{
    public float arrowLength = 5;
    
    //Grass
    [Header("Grass")] 
    public Vector4 windDirection;
    public float windForce;
    public float windSpeed;
    public float fallDistance;
    
    //Clouds
    public Texture2D  worldMask;
    
    public Vector4 worldScrollTillingA;
    public Vector4 worldScrollTillingB;
    public float worldMaskIntesity;
    
    [Range(-1.0f,1.0f)]
    public float worldMaskSteapA;
    [Range(-1.0f,1.0f)]
    public float worldMaskSteapB;
    
    public float worldMaskShadowIntensity;
    public float worldMaskHightIntensity;
    
    //Grass
    private readonly int _shader_WindDirection = Shader.PropertyToID("_WindDirection");
    private readonly int _shader_WindForce = Shader.PropertyToID("_WindForce");
    private readonly int _shader_WindSpeed = Shader.PropertyToID("_WindSpeed");
    private readonly int _shader_FallDistance = Shader.PropertyToID("_FallDistance");
    
    //Shader Cache
    private readonly int _shader_WorldMask = Shader.PropertyToID("_WorldMask");
    private readonly int _shader_WorldScrollTillingA = Shader.PropertyToID("_WorldScrollTillingA");
    private readonly int _shader_WorldScrollTillingB = Shader.PropertyToID("_WorldScrollTillingB");
    private readonly int _shader_WorldCloudIntensity = Shader.PropertyToID("_WorldCloudIntensity");
    
    private readonly int _shader_WorldShadowCloudIntensity = Shader.PropertyToID("_WorldShadowCloudIntensity");
    private readonly int _shader_WorldLightCloudIntensity = Shader.PropertyToID("_WorldLightCloudIntensity");
    
    private readonly int _CloudStepA = Shader.PropertyToID("_CloudStepA");
    private readonly int _CloudStepB = Shader.PropertyToID("_CloudStepB");
 
    void Update()
    {
        SetWeatherProperties();
    }

    void SetWeatherProperties()
    {
        windDirection = (transform.rotation * Vector3.forward);
        
        //Grass
        Shader.SetGlobalVector(_shader_WindDirection, windDirection);
        Shader.SetGlobalFloat(_shader_WindForce, windForce);
        Shader.SetGlobalFloat(_shader_WindSpeed, windSpeed);
        Shader.SetGlobalFloat(_shader_FallDistance, fallDistance);
        
        //World Mask
        Shader.SetGlobalTexture(_shader_WorldMask, worldMask);
        Shader.SetGlobalVector(_shader_WorldScrollTillingA, worldScrollTillingA);
        Shader.SetGlobalVector(_shader_WorldScrollTillingB, worldScrollTillingB);
        Shader.SetGlobalFloat(_shader_WorldCloudIntensity, worldMaskIntesity);
        
        Shader.SetGlobalFloat(_CloudStepA, worldMaskSteapA);
        Shader.SetGlobalFloat(_CloudStepB, worldMaskSteapB);
        
        Shader.SetGlobalFloat(_shader_WorldShadowCloudIntensity, worldMaskShadowIntensity);
        Shader.SetGlobalFloat(_shader_WorldLightCloudIntensity, worldMaskHightIntensity);
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 wind = new Vector3(windDirection.x, windDirection.y, windDirection.z);
        Vector3 direction = wind;
        
        Gizmos.DrawRay(transform.position, direction * arrowLength);

        Handles.color = Color.yellow;
        Handles.ConeHandleCap(0, transform.position + (direction * arrowLength), transform.rotation, 1f, EventType.Repaint);
        
        Handles.Label(transform.position, "Wind Direction");
    }
    #endif
}
