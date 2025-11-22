using System;
using System.Linq;
using UnityEngine;

public class PM_Greyscale : PerceptualManipulation
{
    [SerializeField] private MeshRenderer[] modelMeshRenderers;
    [SerializeField] private Material greyscaleMaterial;

    private Material[] originalMaterials;
    private float gain;
    private bool currentlyChangingGreyscaleDegree;
    private bool appliedGreyscaleMaterial;

    private void Update()
    {
        if (this.currentlyChangingGreyscaleDegree)
        {
            if (!this.appliedGreyscaleMaterial)
            {
                // apply the greyscale material, if it isn't in use yet
                foreach (MeshRenderer mr in this.modelMeshRenderers) mr.material = this.greyscaleMaterial;
                this.appliedGreyscaleMaterial = true;
            }

            foreach (MeshRenderer mr in this.modelMeshRenderers)
            {
                // get the current effect amount and increase it by the current gain
                float newEffectAmount = mr.material.GetFloat("_EffectAmount") + this.gain * Time.deltaTime;
                if (newEffectAmount >= 0 && newEffectAmount <= 1) mr.material.SetFloat("_EffectAmount", newEffectAmount);
            }
        }
    }

    public override void StartPM() 
    {
        // collect the original materials before the PM started
        this.originalMaterials = new Material[this.modelMeshRenderers.Length];
        for (int i = 0; i < this.modelMeshRenderers.Length; i++) this.originalMaterials[i] = this.modelMeshRenderers[i].material;
    }

    public override void StopPM(bool resolve)
    {
        this.currentlyChangingGreyscaleDegree = false;

        if (resolve)
        {
            // restore the original materials
            for (int i = 0; i < this.modelMeshRenderers.Length; i++) this.modelMeshRenderers[i].material = this.originalMaterials[i];
            this.appliedGreyscaleMaterial = false;
        }
    }

    public override void UpdatePM(float gain)
    {
        this.gain = gain;
        this.currentlyChangingGreyscaleDegree = true;
    }
}
