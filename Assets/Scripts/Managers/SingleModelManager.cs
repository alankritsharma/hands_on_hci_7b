using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleModelManager : AbstractModelManager
{
    [System.Serializable]
    private struct MaterialOverride
    {
        public MeshRenderer meshRenderer;
        public Material[] opaqueMaterial;
        public Material[] transparentMaterial;
        public Color color;

        public MaterialOverride(MeshRenderer mr, Material[] opaque, Material[] transparent, Color color)
        {
            this.meshRenderer = mr;
            this.opaqueMaterial = opaque;
            this.transparentMaterial = transparent;
            this.color = color;
        }

        public void UpdateMaterialAlpha(float alpha)
        {
            this.color.a = alpha;
            foreach (Material material in this.meshRenderer.materials) material.color = this.color;
        }
    }

    [Header("References")]
    [SerializeField] private GameObject model;
    [SerializeField] private Transform modelParent;
    [SerializeField] private GameObject[] paintings;
    [SerializeField] private Material fineTuningMaterialTranslation;
    [SerializeField] private Material fineTuningMaterialRotation;

    [Header("Materials")]
    [SerializeField] private Material[] opaqueModelMaterial;
    [SerializeField] private Material[] transparentModelMaterial;

    [Header("Settings")]
    [SerializeField] private float modelFadeDuration;
    [SerializeField] private MaterialOverride[] materialOverrides;

    // References
    private MeshRenderer[] modelMeshRenderers;

    // Fading
    private FadingState fadingState = FadingState.none;
    private float fadeDurationRemaining;

    private void Start()
    {
        // get references
        this.modelMeshRenderers = this.model.GetComponentsInChildren<MeshRenderer>();
    }

    private void Update()
    {
        if (this.fadingState == FadingState.none) return;

        // update the alpha based on the remaining time
        this.UpdateModelMaterialAlpha();
        this.fadeDurationRemaining -= Time.deltaTime;

        // deactivate the model if it finished fading out, change it to the opaque material if it finished fading in
        if (this.fadeDurationRemaining <= 0)
        {
            if (this.fadingState == FadingState.fadingIn) this.ApplyOpaqueMaterial();
            else this.model.SetActive(false);

            // reset the fading state
            this.fadingState = FadingState.none;

            // send state update to the BLE remote
            if (BLEReceiver.Instance != null) BLEReceiver.Instance.SendStateUpdate();
        }
    }

    // update the alpha of the model's material based on the remaining fading duration
    private void UpdateModelMaterialAlpha()
    {
        float fraction = (this.modelFadeDuration - this.fadeDurationRemaining) / this.modelFadeDuration;
        foreach (Material material in this.modelMeshRenderers[0].materials)
        {
            Color color = material.color;
            color.a = this.fadingState == FadingState.fadingIn ? fraction : 1f - fraction;
            material.color = color;
        }
        // foreach (MaterialOverride mo in this.materialOverrides) mo.UpdateMaterialAlpha(color.a);
    }


    // toggle visibility of the model
    public override void ToggleModelVisibility(bool fade=false)
    {
        if (fade)
        {
            this.fadingState = this.model.activeInHierarchy ? FadingState.fadingOut : FadingState.fadingIn;
            this.fadeDurationRemaining = this.modelFadeDuration;
            this.model.SetActive(true);
            this.ApplyTransparentMaterial();
        }
        else
        {
            // toggle model visibility and ensure the opaque material is used
            Utils.ToggleGameObject(this.model);
            if (this.model.activeInHierarchy) this.ApplyOpaqueMaterial();

            // send state update to the BLE remote
            if (BLEReceiver.Instance != null) BLEReceiver.Instance.SendStateUpdate();
        }
    }

    // check whether the model is currently visible
    public override bool IsModelVisible()
    {
        return this.model.activeInHierarchy;
    }

    // (de-)activate the paintings
    public override void TogglePaintings()
    {
        foreach (GameObject painting in this.paintings) Utils.ToggleGameObject(painting);
    }

    // (de-)activate the paintings
    public override void SetPaintingsActive(bool active)
    {
        foreach (GameObject painting in this.paintings) painting.SetActive(active);
    }

    // show the model with the fine tuning material
    public override void ShowWithFineTuningMaterial()
    {
        if (!this.IsModelVisible()) this.ToggleModelVisibility();
        foreach (MeshRenderer mr in this.modelMeshRenderers) mr.material = this.fineTuningMaterialTranslation;
    }

    // switch between the two fine tuning materials
    public override void AssignFineTuningMaterial(bool rotationMode)
    {
        foreach (MeshRenderer mr in this.modelMeshRenderers)
        {
            Material[] materials = mr.materials;
            Material currentFineTuningMaterial = rotationMode ? this.fineTuningMaterialRotation : this.fineTuningMaterialTranslation;
            
            // Iterate through the materials and assign the correct material based on rotationMode
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = currentFineTuningMaterial;
            }

            // Update the materials back to the MeshRenderer
            mr.materials = materials;
        }
    }

    // restore the model's original opaque material
    public override void ApplyOpaqueMaterial()
    {
        foreach (MeshRenderer mr in this.modelMeshRenderers) mr.materials = this.opaqueModelMaterial;
        foreach (MaterialOverride mo in this.materialOverrides) mo.meshRenderer.materials = mo.opaqueMaterial;
    }

    // apply the models' transparent materials
    public override void ApplyTransparentMaterial()
    {
        foreach (MeshRenderer mr in this.modelMeshRenderers) mr.materials = this.transparentModelMaterial;
        foreach (MaterialOverride mo in this.materialOverrides) mo.meshRenderer.materials = mo.transparentMaterial;
    }

    // get the fade duration
    public override float GetModelFadeDuration()
    {
        return this.modelFadeDuration;
    }

    public override void SetModelFadeDuration(float duration)
    {
        this.modelFadeDuration = duration;
    }

    public override void LoadStyleTransferModel() { }

    public override void LockModel(int index) { }

    public override int GetModelCount()
    {
        return 1;
    }

    public override void LoadHighResModel() { }

    public override void LockCurrentModel() { }

    public override void UnlockModel() { }

    public override void TogglePosterMask() { }
}
