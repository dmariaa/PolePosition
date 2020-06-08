using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    public GameObject ColorButtonPrefab;
    public Material CarMaterial;

    public static List<Color32> Colors = new List<Color32>()
    {
        new Color32(93,14,24,255),
        new Color32(105,14,24,255),
        new Color32(118,14,24,255),
        new Color32(131,14,24,255),
        new Color32(143,14,24,255),
        new Color32(156,13,23,255),
        new Color32(169,13,23,255),
        new Color32(181,13,23,255),
        new Color32(194,13,23,255),
        new Color32(207,12,22,255),
        new Color32(211,38,47,255),
        new Color32(215,56,64,255),
        new Color32(219,74,81,255),
        new Color32(223,92,98,255),
        new Color32(227,110,115,255),
        new Color32(231,128,133,255),
        new Color32(235,146,150,255),
        new Color32(239,164,167,255),
        new Color32(243,182,184,255),
        new Color32(247,200,202,255),
        new Color32(84,52,15,255),
        new Color32(101,59,14,255),
        new Color32(119,67,12,255),
        new Color32(137,75,10,255),
        new Color32(155,83,9,255),
        new Color32(173,90,7,255),
        new Color32(191,98,5,255),
        new Color32(209,106,4,255),
        new Color32(227,114,2,255),
        new Color32(245,122,0,255),
        new Color32(231,138,16,255),
        new Color32(232,148,37,255),
        new Color32(234,158,59,255),
        new Color32(235,169,81,255),
        new Color32(237,179,103,255),
        new Color32(238,189,125,255),
        new Color32(240,200,147,255),
        new Color32(241,210,169,255),
        new Color32(243,220,191,255),
        new Color32(245,231,213,255),
        new Color32(71,67,30,255),
        new Color32(87,81,28,255),
        new Color32(103,96,26,255),
        new Color32(119,110,24,255),
        new Color32(135,125,22,255),
        new Color32(151,139,20,255),
        new Color32(167,154,18,255),
        new Color32(183,168,16,255),
        new Color32(199,183,14,255),
        new Color32(215,198,11,255),
        new Color32(227,216,7,255),
        new Color32(229,219,29,255),
        new Color32(232,223,52,255),
        new Color32(235,227,75,255),
        new Color32(238,230,98,255),
        new Color32(240,234,121,255),
        new Color32(243,238,144,255),
        new Color32(246,241,167,255),
        new Color32(249,245,190,255),
        new Color32(252,249,213,255),
        new Color32(50,87,34,255),
        new Color32(50,94,32,255),
        new Color32(51,102,30,255),
        new Color32(51,109,27,255),
        new Color32(52,117,25,255),
        new Color32(52,124,23,255),
        new Color32(53,132,20,255),
        new Color32(53,139,18,255),
        new Color32(54,147,16,255),
        new Color32(55,155,13,255),
        new Color32(50,179,7,255),
        new Color32(67,185,28,255),
        new Color32(84,191,50,255),
        new Color32(102,197,72,255),
        new Color32(119,203,93,255),
        new Color32(137,210,115,255),
        new Color32(154,216,137,255),
        new Color32(172,222,158,255),
        new Color32(189,228,180,255),
        new Color32(207,235,202,255),
        new Color32(3,76,88,255),
        new Color32(3,81,97,255),
        new Color32(3,86,107,255),
        new Color32(3,91,117,255),
        new Color32(3,96,126,255),
        new Color32(3,101,136,255),
        new Color32(3,106,146,255),
        new Color32(3,111,155,255),
        new Color32(3,116,165,255),
        new Color32(4,121,175,255),
        new Color32(6,137,207,255),
        new Color32(28,147,211,255),
        new Color32(50,158,215,255),
        new Color32(73,169,219,255),
        new Color32(95,180,223,255),
        new Color32(118,190,228,255),
        new Color32(140,201,232,255),
        new Color32(163,212,236,255),
        new Color32(185,223,240,255),
        new Color32(208,234,245,255),
        new Color32(16,50,92,255),
        new Color32(15,51,99,255),
        new Color32(14,52,107,255),
        new Color32(13,53,115,255),
        new Color32(12,54,123,255),
        new Color32(10,55,131,255),
        new Color32(9,56,139,255),
        new Color32(8,57,147,255),
        new Color32(7,58,155,255),
        new Color32(5,60,163,255),
        new Color32(34,69,191,255),
        new Color32(55,87,198,255),
        new Color32(76,105,205,255),
        new Color32(98,124,212,255),
        new Color32(119,142,219,255),
        new Color32(140,160,226,255),
        new Color32(162,179,233,255),
        new Color32(183,197,240,255),
        new Color32(204,215,247,255),
        new Color32(226,234,255,255),
        new Color32(25,21,92,255),
        new Color32(28,21,100,255),
        new Color32(32,21,108,255),
        new Color32(36,21,117,255),
        new Color32(39,21,125,255),
        new Color32(43,21,133,255),
        new Color32(47,21,142,255),
        new Color32(50,21,150,255),
        new Color32(54,21,158,255),
        new Color32(58,22,167,255),
        new Color32(78,0,187,255),
        new Color32(93,22,193,255),
        new Color32(109,44,199,255),
        new Color32(124,67,205,255),
        new Color32(140,89,211,255),
        new Color32(155,111,217,255),
        new Color32(171,134,223,255),
        new Color32(186,156,229,255),
        new Color32(202,178,235,255),
        new Color32(218,201,242,255),
        new Color32(92,35,82,255),
        new Color32(95,37,87,255),
        new Color32(99,40,93,255),
        new Color32(103,43,99,255),
        new Color32(107,46,105,255),
        new Color32(111,49,111,255),
        new Color32(115,52,117,255),
        new Color32(119,55,123,255),
        new Color32(123,58,129,255),
        new Color32(127,61,135,255),
        new Color32(158,80,162,255),
        new Color32(167,93,171,255),
        new Color32(177,107,181,255),
        new Color32(187,121,191,255),
        new Color32(197,135,201,255),
        new Color32(207,149,210,255),
        new Color32(217,163,220,255),
        new Color32(227,177,230,255),
        new Color32(237,191,240,255),
        new Color32(247,205,250,255)
    };

    private ToggleGroup _toggleGroup;

    private Color _selectedColor;

    public Color SelectedColor
    {
        get => _selectedColor;
        set
        {
            _selectedColor = value;
            CarMaterial.color = _selectedColor;
        }
    }

    private void Awake()
    {
        _toggleGroup = GetComponent<ToggleGroup>();
    }

    private void Start()
    {
        CreateColorPalette();
        _selectedColor = Colors[0];
        
    }

    void CreateColorPalette()
    {
        foreach (var color in Colors)
        {
            CreateColorButton(color);
        }
    }

    void CreateColorButton(Color color)
    {
        GameObject colorButton = Instantiate(ColorButtonPrefab, transform, true);
        Toggle toggle = colorButton.GetComponent<Toggle>();
        toggle.group = _toggleGroup;
        
        Image image = colorButton.transform.Find("Button").GetComponent<Image>();
        image.color = color;

        toggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                SelectedColor = image.color;
            }
        });
    }
}
