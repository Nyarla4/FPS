using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class LocalVolumeContoller : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private Volume _localVolume;
    [SerializeField] private Slider _exposureSlider;
    [SerializeField] private Slider _constrastSlider;
    [SerializeField] private Slider _hueShiftSlider;
    [SerializeField] private Slider _saturationSlider;

    ColorAdjustments _color;

    private float _defaultExposure = 0f;
    private float _defaultConstrast = 0f;
    private float _defaultHueShift = 0f;
    private float _defaultSaturation = 0f;

    private void Awake()
    {
        if (_localVolume == null)
        {
            Debug.LogError("[LocalVolumeContoller] local volume is missing");
        }

        if (_localVolume != null && _localVolume.profile != null)
        {
            _localVolume.profile.TryGet(out _color);

            if (_color != null)
            {
                if (_color.postExposure.overrideState)
                {
                    _defaultExposure = _color.postExposure.value;
                }

                if (_color.contrast.overrideState)
                {
                    _defaultConstrast = _color.contrast.value;
                }

                if(_color.hueShift.overrideState)
                {
                    _defaultHueShift = _color.hueShift.value;
                }

                if(_color.saturation.overrideState)
                {
                    _defaultSaturation = _color.saturation.value;
                }
            }

            if (_exposureSlider != null)
            {
                _exposureSlider.minValue = -2f;
                _exposureSlider.maxValue = 2f;
                _exposureSlider.value = _defaultExposure;
                _exposureSlider.onValueChanged.AddListener(ExposureOnChanged);
            }

            if (_constrastSlider != null)
            {
                _constrastSlider.minValue = -100f;
                _constrastSlider.maxValue = 100f;
                _constrastSlider.value = _defaultConstrast;
                _constrastSlider.onValueChanged.AddListener(ConstrastOnChanged);
            }
            if (_hueShiftSlider != null)
            {
                _hueShiftSlider.minValue = -180f;
                _hueShiftSlider.maxValue = 180f;
                _hueShiftSlider.value = _defaultHueShift;
                _hueShiftSlider.onValueChanged.AddListener(HueShiftOnChanged);
            }
            if (_saturationSlider != null)
            {
                _saturationSlider.minValue = -100f;
                _saturationSlider.maxValue = 100f;
                _saturationSlider.value = _defaultSaturation;
                _saturationSlider.onValueChanged.AddListener(SaturationOnChanged);
            }
            ApplyAll();
        }
    }

    private void ExposureOnChanged(float v)
    {
        _color.postExposure.Override(v);
    }

    private void ConstrastOnChanged(float v)
    {
        _color.contrast.Override(v);
    }
    
    private void HueShiftOnChanged(float v)
    {
        _color.hueShift.Override(v);
    }
    
    private void SaturationOnChanged(float v)
    {
        _color.saturation.Override(v);
    }

    void ApplyAll()
    {
        if (_color != null)
        {
            _color.postExposure.Override(_defaultExposure);
            if (_exposureSlider != null)
            {
                _exposureSlider.value = _defaultExposure;
            }
            _color.contrast.Override(_defaultConstrast);
            if (_constrastSlider != null)
            {
                _constrastSlider.value = _defaultConstrast;
            }
            _color.hueShift.Override(_defaultHueShift);
            if (_hueShiftSlider != null)
            {
                _hueShiftSlider.value = _defaultHueShift;
            }
            _color.saturation.Override(_defaultSaturation);
            if (_saturationSlider != null)
            {
                _saturationSlider.value = _defaultSaturation;
            }
        }
    }
}
