using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractModelManager: MonoBehaviour
{
    protected enum FadingState { none, fadingIn, fadingOut }
    
    public abstract void ToggleModelVisibility(bool fade=false);
    public abstract void LoadHighResModel();
    public abstract void LoadStyleTransferModel();
    public abstract void LockModel(int index);
    public abstract void LockCurrentModel();
    public abstract void UnlockModel();
    public abstract void TogglePosterMask();
    public abstract void TogglePaintings();
    public abstract void SetPaintingsActive(bool active);
    public abstract bool IsModelVisible();
    public abstract int GetModelCount();
    public abstract void AssignFineTuningMaterial(bool rotationMode);
    public abstract void ShowWithFineTuningMaterial();
    public abstract void ApplyOpaqueMaterial();
    public abstract void ApplyTransparentMaterial();
    public abstract float GetModelFadeDuration();
    public abstract void SetModelFadeDuration(float duration);
}
