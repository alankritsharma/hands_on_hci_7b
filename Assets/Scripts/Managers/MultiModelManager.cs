using System;
using UnityEngine;

namespace Managers
{
    public class MultiModelManager: AbstractModelManager
    {
        
        [System.Serializable]
        public struct Model
        {
            public string name;
            public GameObject model;
            public MeshRenderer meshRenderer;
            public Material[] opaqueMaterials;
            public Material[] transparentMaterials;
        }
        
        // ----------------------------------------------- Variables ---------------------------------------------------
        
        [Header("References")]
        [SerializeField] private Transform modelParent;
        [SerializeField] private Material fineTuningMaterialTranslation;
        [SerializeField] private Material fineTuningMaterialRotation;
        
        [Header("Models")]
        [SerializeField] private Model[] models;
        public Model[] Models => models;
        
        [Header("Settings")]
        [SerializeField] private float modelFadeDuration;
        
        // References
        private Model currentModel;
        
        // Fading
        private FadingState fadingState = FadingState.none;
        private float fadeDurationRemaining;
        
        // ----------------------------------------------- Methods -----------------------------------------------------

        private void Awake()
        {
            if (models.Length == 0) throw new MissingReferenceException("At least one model has to be assigned.");
            currentModel = models[0];
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
                else this.currentModel.model.SetActive(false);
                
                // reset the fading state
                this.fadingState = FadingState.none;
            }
        }

        // update the alpha of the current model's materials based on the remaining fading duration
        private void UpdateModelMaterialAlpha()
        {
            // calculate remaining percentage
            var fraction = (this.modelFadeDuration - this.fadeDurationRemaining) / this.modelFadeDuration;
            // apply to each material in the current model's mesh renderer
            foreach (var material in this.currentModel.meshRenderer.materials)
            {
                var color = material.color;
                color.a = this.fadingState == FadingState.fadingIn ? fraction : 1f - fraction;
                material.color = color;
            }
        }

        public void ChooseCurrentModel(int index)
        {
            var visible = IsModelVisible();
            // deactivate the first model
            this.currentModel.model.SetActive(false);
            // change the current model
            this.currentModel = models[index];
            // activate the second model if the first was visible
            if (visible) this.currentModel.model.SetActive(true);
        }

        // toggle visibility of the model
        public override void ToggleModelVisibility(bool fade = false)
        {
            if (fade)
            {
                this.fadingState = this.currentModel.model.activeInHierarchy ? FadingState.fadingOut : FadingState.fadingIn;
                this.fadeDurationRemaining = this.modelFadeDuration;
                this.currentModel.model.SetActive(true);
                this.ApplyTransparentMaterial();
            }
            else
            {
                // toggle model visibility and ensure the opaque material is used if active
                Utils.ToggleGameObject(this.currentModel.model);
                if (this.currentModel.model.activeInHierarchy) this.ApplyOpaqueMaterial();
            }
        }

        // check whether the model is currently visible
        public override bool IsModelVisible()
        {
            return this.currentModel.model.activeInHierarchy;
        }

        // show the model with the fine-tuning material
        public override void ShowWithFineTuningMaterial()
        {
            if (!this.IsModelVisible()) this.ToggleModelVisibility();
            
            // iterate through all mesh renderer materials and set them to the fine-tuning material
            var materials = this.currentModel.meshRenderer.materials;
            for (var i = 0; i < this.currentModel.meshRenderer.materials.Length; i++)
                materials[i] = this.fineTuningMaterialTranslation;
            // update the materials back to the mesh renderer
            this.currentModel.meshRenderer.materials = materials;
        }

        // switch between the two fine-tuning materials
        public override void AssignFineTuningMaterial(bool rotationMode)
        {
            var currentFineTuningMaterial = rotationMode ? this.fineTuningMaterialRotation : this.fineTuningMaterialTranslation;
            
            // from here to basically the same as in ShowWithFineTuningMaterial()
            var materials = this.currentModel.meshRenderer.materials;
            for (var i = 0; i < this.currentModel.meshRenderer.materials.Length; i++)
                materials[i] = currentFineTuningMaterial;
            this.currentModel.meshRenderer.materials = materials;
        }

        // restore the model's original opaque material
        public override void ApplyOpaqueMaterial()
        {
            this.currentModel.meshRenderer.materials = this.currentModel.opaqueMaterials;
        }

        // apply the model's transparent material
        public override void ApplyTransparentMaterial()
        {
            this.currentModel.meshRenderer.materials = this.currentModel.transparentMaterials;
        }
        
        // get the number of models
        public override int GetModelCount()
        {
            return models.Length;
        }
        
        // get the fade duration
        public override float GetModelFadeDuration()
        {
            return this.modelFadeDuration;
        }

        // set the fade duration
        public override void SetModelFadeDuration(float duration)
        {
            this.modelFadeDuration = duration;
        }

        public override void TogglePaintings() { }
        
        public override void SetPaintingsActive(bool active) { }

        public override void LoadStyleTransferModel() { }

        public override void LockModel(int index) { }

        public override void LoadHighResModel() { }

        public override void LockCurrentModel() { }

        public override void UnlockModel() { }

        public override void TogglePosterMask() { }
    }
}