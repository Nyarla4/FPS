using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PostVolulmeController : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private Volume _globalVolume;
    [SerializeField] private Slider _bloomSlider;
    [SerializeField] private Slider _exposureSlider;
    [SerializeField] private Slider _dofFocusSlider;

    Bloom _bloom;
    ColorAdjustments _color;
    DepthOfField _dof;

    float defaultBloomIntensity = 2f;
    float defaultExposure = 0f;
    float defaultFocusDistance = 5f;

    private void Awake()
    {
        if (_globalVolume == null)
        {
            var gv = GameObject.Find("Global Volume");
            if (gv != null)
            {
                _globalVolume = gv.GetComponent<Volume>();
            }
        }

        if (_globalVolume != null && _globalVolume.profile != null)
        {
            _globalVolume.profile.TryGet(out _bloom);
            _globalVolume.profile.TryGet(out _color);
            _globalVolume.profile.TryGet(out _dof);

            if (_bloom != null && _bloom.intensity.overrideState)
            {
                defaultBloomIntensity = _bloom.intensity.value;
            }

            if (_color != null && _color.postExposure.overrideState)
            {
                defaultExposure = _color.postExposure.value;
            }

            if (_dof != null && _dof.focusDistance.overrideState)
            {
                defaultFocusDistance = _dof.focusDistance.value;
            }

            if (_bloomSlider != null)
            {
                _bloomSlider.minValue = 0f;
                _bloomSlider.maxValue = 5f;
                _bloomSlider.value = defaultBloomIntensity;
                _bloomSlider.onValueChanged.AddListener(BloomIntensityOnChanged);
            }

            if (_exposureSlider != null)
            {
                _exposureSlider.minValue = -2f;
                _exposureSlider.maxValue = 2f;
                _exposureSlider.value = defaultExposure;
                _exposureSlider.onValueChanged.AddListener(ExposureOnChanged);
            }

            if (_dofFocusSlider != null)
            {
                _dofFocusSlider.minValue = 0.5f;
                _dofFocusSlider.maxValue = 20f;
                _dofFocusSlider.value = defaultFocusDistance;
                _dofFocusSlider.onValueChanged.AddListener(FocusDistanceOnChanged);
            }

            ApplyAll();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleBloom();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleExposure();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleDof();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ResetDefault();
        }
    }

    private void ToggleBloom()
    {
        if (_bloom != null)
        {
            _bloom.active = !_bloom.active;
        }
    }

    private void ToggleExposure()
    {
        if (_color != null)
        {
            _color.active = !_color.active;
        }
    }

    private void ToggleDof()
    {
        if (_dof != null)
        {
            _dof.active = !_dof.active;
        }
    }

    private void ResetDefault()
    {
        if (_bloom != null)
        {
            _bloom.intensity.Override(defaultBloomIntensity);
            if (_bloomSlider != null)
            {
                _bloomSlider.value = defaultBloomIntensity;
            }
        }
        if (_color != null)
        {
            _color.postExposure.Override(defaultExposure);
            if (_exposureSlider != null)
            {
                _exposureSlider.value = defaultExposure;
            }
        }
        if (_dof != null)
        {
            _dof.focusDistance.Override(defaultFocusDistance);
            if (_dofFocusSlider != null)
            {
                _dofFocusSlider.value = defaultFocusDistance;
            }
        }
    }

    private void BloomIntensityOnChanged(float v)
    {
        _bloom.intensity.Override(v);
    }

    private void ExposureOnChanged(float v)
    {
        _color.postExposure.Override(v);
    }

    private void FocusDistanceOnChanged(float v)
    {
        _dof.focusDistance.Override(v);
    }

    void ApplyAll()
    {
        if (_bloom != null && _bloomSlider != null)
        {
            _bloom.intensity.Override(_bloomSlider.value);
        }

        if (_color != null && _exposureSlider != null)
        {
            _color.postExposure.Override(_exposureSlider.value);
        }

        if (_dof != null && _dofFocusSlider != null)
        {
            _dof.focusDistance.Override(_dofFocusSlider.value);
        }
    }
}
