using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using VolFx;
using static VolFx.OldMoviePass;

public class NoiseManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume noiseVolume;
    [SerializeField] private Camera cam;

    [Header("Settings")]
    [SerializeField] private bool moveOppositeToCamera;
    [SerializeField] private float cameraMovementRayLength;
    [SerializeField] private float cameraMovementMultiplier;
    [SerializeField] private bool cumulativeCountermovement;
    [SerializeField] private bool activateNoiseOnStart;

    // references
    private VolumeProfile noiseProfile;
    private OldMovieVol oldMovieVol;

    // mappings for cycling through the different GrainTex options
    private Dictionary<GrainTex, GrainTex> nextGrainTex = new Dictionary<GrainTex, GrainTex>
    {
        { GrainTex.Large_A, GrainTex.Large_B },
        { GrainTex.Large_B, GrainTex.Medium_A },
        { GrainTex.Medium_A, GrainTex.Medium_B },
        { GrainTex.Medium_B, GrainTex.Medium_C },
        { GrainTex.Medium_C, GrainTex.Medium_D },
        { GrainTex.Medium_D, GrainTex.Medium_E },
        { GrainTex.Medium_E, GrainTex.Medium_F },
        { GrainTex.Medium_F, GrainTex.Thin_A},
        { GrainTex.Thin_A, GrainTex.Thin_B },
        { GrainTex.Thin_B, GrainTex.Large_A }
    };
    private Dictionary<GrainTex, GrainTex> previousGrainTex = new Dictionary<GrainTex, GrainTex>
    {
        { GrainTex.Large_A, GrainTex.Thin_B },
        { GrainTex.Large_B, GrainTex.Large_A },
        { GrainTex.Medium_A, GrainTex.Large_B},
        { GrainTex.Medium_B, GrainTex.Medium_A },
        { GrainTex.Medium_C, GrainTex.Medium_B },
        { GrainTex.Medium_D, GrainTex.Medium_C },
        { GrainTex.Medium_E, GrainTex.Medium_D },
        { GrainTex.Medium_F, GrainTex.Medium_E },
        { GrainTex.Thin_A, GrainTex.Medium_F},
        { GrainTex.Thin_B, GrainTex.Thin_A }
    };

    private Vector3 lastCameraMovementRayPos;

    private void Start()
    {
        // get references
        this.noiseProfile = this.noiseVolume.profile;
        this.noiseProfile.TryGet<OldMovieVol>(out this.oldMovieVol);

        // set grain offset override according to the chosen setting
        this.SetGrainOffsetOverride(this.moveOppositeToCamera);

        // activate noise by default
        if (this.activateNoiseOnStart) this.ToggleNoiseVolume();
    }

    private void Update()
    {
        if (this.moveOppositeToCamera) this.CalculateGrainOffset();
    }


    // calculate grain offset to counteract the cameras movement, making the grain appear as static in the world
    private void CalculateGrainOffset()
    {
        // define a few vectors based on old position, new position and the camera position
        Vector3 newPos = this.cam.transform.position + this.cam.transform.forward * this.cameraMovementRayLength;
        Vector3 oldToNew = newPos - this.lastCameraMovementRayPos;
        Vector3 middle = this.lastCameraMovementRayPos + (oldToNew / 2);
        Vector3 normal = middle - this.cam.transform.position;

        // calculate movement projected on the plane with the calculated normal, throw away z-value because grain only moves along x and y of the screen
        Vector2 camMovement = Vector3.ProjectOnPlane(oldToNew, normal) * this.cameraMovementMultiplier;

        // apply calculated movement to the grain offset
        if (this.cumulativeCountermovement) this.oldMovieVol.m_GrainOffset.value += camMovement;
        else this.oldMovieVol.m_GrainOffset.value = camMovement;

        // update the stored position
        this.lastCameraMovementRayPos = newPos;
    }

    // apply the updated volume profile
    private void ApplyProfile()
    {
        this.noiseVolume.profile = this.noiseProfile;
    }

    // toggle the complete noise volume
    public void ToggleNoiseVolume()
    {
        this.noiseVolume.enabled = !this.noiseVolume.enabled;
        if (BLEReceiver.Instance != null) BLEReceiver.Instance.SendStateUpdate();
    }

    // check if the noise volume is currently active
    public bool GetNoiseVolumeActive()
    {
        return this.noiseVolume.enabled;
    }

    // dis-/enable grain override
    public void SetGrainOverride(bool value)
    {
        this.oldMovieVol.m_Grain.overrideState = value;
        this.ApplyProfile();
    }


    // adjust the grain setting
    public void SetGrain(float grain)
    {
        this.oldMovieVol.m_Grain.value = grain;
        this.ApplyProfile();
    }

    // dis-/enable fps override
    public void SetFpsOverride(bool value)
    {
        this.oldMovieVol.m_Fps.overrideState = value;
        this.ApplyProfile();
    }


    // adjust the fps setting
    public void SetFps(float fps)
    {
        this.oldMovieVol.m_Fps.value = fps;
        this.ApplyProfile();
    }

    // dis-/enable grain texture override
    public void SetGrainTextureOverride(bool value)
    {
        this.oldMovieVol.m_GrainTex.overrideState = value;
        this.ApplyProfile();
    }

    // go to the next grain texture
    public void NextGrainTexture()
    {
        GrainTex newTex = this.nextGrainTex[this.oldMovieVol.m_GrainTex.value];
        this.oldMovieVol.m_GrainTex.value = newTex;

        this.ApplyProfile();
    }

    // go to the previous grain texture
    public void PreviousGrainTexture()
    {
        GrainTex newTex = this.previousGrainTex[this.oldMovieVol.m_GrainTex.value];
        this.oldMovieVol.m_GrainTex.overrideState = true;
        this.oldMovieVol.m_GrainTex.value = newTex;

        this.ApplyProfile();
    }

    // dis-/enable grain offset override
    public void SetGrainOffsetOverride(bool value)
    {
        this.moveOppositeToCamera = value;
        this.oldMovieVol.m_GrainOffset.overrideState = value;
        this.ApplyProfile();
    }

    // update the multiplier for the countermovement
    public void SetCameraMovementMultiplier(float value)
    {
        this.cameraMovementMultiplier = value;
    }
}
