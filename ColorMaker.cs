using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class VerticalColorMaker : MonoBehaviour
{
    public RawImage rawImage; // Gradyan gösterecek RawImage

    public Color color1 = Color.red; // İlk renk
    public Color color2 = Color.blue; // İkinci renk

    [PercentageSlider]
    [Range(-200, 200)]
    public float colorSlider = 50f;

    private Texture2D gradientTexture;
    private Texture2D oldTexture;

    public enum GradientType
    {
        Vertical,
        Horizontal,
        Cross
    }

    public GradientType type;

    private void Start()
    {
        // Sadece Play Mode'da değil, her durumda çalıştırılacak
        UpdateGradient();
    }

    private void OnValidate()
    {
        // Editor'de değerler değiştirildiğinde otomatik çalışır
        UpdateGradient();
    }

    public void UpdateGradient()
    {
        if (type == GradientType.Vertical)
        {
            GradientVertical();
        }
        else if (type == GradientType.Horizontal)
        {
            GradientHorizontal();
        }
        else if (type == GradientType.Cross)
        {
            GradientCross();
        }
    }

    public void GradientVertical()
    {
        if (rawImage == null) return;

        // 1 piksel genişlikte, 256 piksel yükseklikte bir Texture2D oluştur
        gradientTexture = new Texture2D(1, 256);

        for (int y = 0; y < gradientTexture.height; y++)
        {
            // Piksel pozisyonunu normalize et (0 ile 1 arasında)
            float t = (float)y / (gradientTexture.height - 1);

            // BlendAmount'a göre renk geçişini hesapla
            Color gradientColor = CalculateColor(t);
            gradientTexture.SetPixel(0, y, gradientColor);
        }

        gradientTexture.Apply();
        rawImage.texture = gradientTexture;
    }

    public void GradientHorizontal()
    {
        if (rawImage == null) return;

        // 256 piksel genişlikte, 1 piksel yükseklikte bir Texture2D oluştur
        gradientTexture = new Texture2D(256, 1);

        for (int x = 0; x < gradientTexture.width; x++)
        {
            // Piksel pozisyonunu normalize et (0 ile 1 arasında)
            float t = (float)x / (gradientTexture.width - 1);

            // BlendAmount'a göre renk geçişini hesapla
            Color gradientColor = CalculateColor(t);
            gradientTexture.SetPixel(x, 0, gradientColor);
        }

        gradientTexture.Apply();
        rawImage.texture = gradientTexture;
    }

    public void GradientCross()
    {
        if (rawImage == null) return;

        // Yeni diyagonal gradyan oluştur
        gradientTexture = new Texture2D(
            Mathf.FloorToInt(rawImage.rectTransform.rect.width),
            Mathf.FloorToInt(rawImage.rectTransform.rect.height),
            TextureFormat.RGBA32,
            false
        );
        gradientTexture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < gradientTexture.height; y++)
        {
            for (int x = 0; x < gradientTexture.width; x++)
            {
                // X ve Y pozisyonlarını normalize et
                float tX = (float)x / (gradientTexture.width - 1);
                float tY = (float)y / (gradientTexture.height - 1);

                // X ve Y pozisyonlarını harmanla
                float t = Mathf.Lerp(tX, tY, 0.5f);

                // CalculateColor metodunu kullanarak colorSlider etkisini ekle
                Color gradientColor = CalculateColor(t);
                gradientTexture.SetPixel(x, y, gradientColor);
            }
        }

        gradientTexture.Apply();
        rawImage.texture = gradientTexture;

        // Boyut ayarlama ayrı bir metodda
        AdjustRawImageSize();
    }

    private void AdjustRawImageSize()
    {
        if (rawImage != null)
        {
            rawImage.rectTransform.sizeDelta = new Vector2(gradientTexture.width, gradientTexture.height);
        }
    }

    private Color CalculateColor(float normalizedPosition)
    {
        float offset = (colorSlider + 100f) / 200f;

        // Normalleştirilmiş pozisyonu offset'e göre kaydır
        float t = Mathf.Clamp01(normalizedPosition + (offset - 0.5f));

        // İki renk arasında geçiş yap
        return Color.Lerp(color2, color1, t);
    }

    public void SwapColors()
    {
        Color temp = color1;
        color1 = color2;
        color2 = temp;

        UpdateGradient();
    }

    [ContextMenu("Swap Colors")]
    private void SwapColorsButton()
    {
        SwapColors();
        UpdateGradient();
    }
}

// PercentageSliderAttribute
public class PercentageSliderAttribute : PropertyAttribute
{
    public PercentageSliderAttribute() { }
}

#if UNITY_EDITOR
// Özel Property Drawer
[CustomPropertyDrawer(typeof(PercentageSliderAttribute))]
public class PercentageSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        if (property.propertyType == SerializedPropertyType.Float)
        {
            // %'lik değeri hesapla
            float percentValue = property.floatValue / 2f; // Gerçek değer -> % değerine
            percentValue = EditorGUI.Slider(position, label, percentValue, -100f, 100f);
            property.floatValue = percentValue * 2f; // % değeri -> Gerçek değere
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Use with float only.");
        }
        EditorGUI.EndProperty();
    }
}

[CustomEditor(typeof(VerticalColorMaker))]
public class VerticalColorMakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Varsayılan Inspector
        DrawDefaultInspector();

        VerticalColorMaker colorMaker = (VerticalColorMaker)target;

        // Ekstra Buton
        if (GUILayout.Button("Swap Colors"))
        {
            colorMaker.SwapColors();
        }
    }
}
#endif
