using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    public int textureSize = 512;
    public Material waterTextureMaterial;
    public Material noiseTextureMaterial;

    void Start()
    {
        GenerateTextures();
    }

    void GenerateTextures()
    {
        // Генеруємо текстуру води
        RenderTexture waterRT = new RenderTexture(textureSize, textureSize, 0);
        waterRT.Create();
        Graphics.Blit(null, waterRT, waterTextureMaterial);
        Texture2D waterTexture = new Texture2D(textureSize, textureSize);
        RenderTexture.active = waterRT;
        waterTexture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
        waterTexture.Apply();
        RenderTexture.active = null;
        waterRT.Release();

        // Генеруємо текстуру шуму
        RenderTexture noiseRT = new RenderTexture(textureSize, textureSize, 0);
        noiseRT.Create();
        Graphics.Blit(null, noiseRT, noiseTextureMaterial);
        Texture2D noiseTexture = new Texture2D(textureSize, textureSize);
        RenderTexture.active = noiseRT;
        noiseTexture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
        noiseTexture.Apply();
        RenderTexture.active = null;
        noiseRT.Release();

        // Зберігаємо текстури
        byte[] waterBytes = waterTexture.EncodeToPNG();
        byte[] noiseBytes = noiseTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/WaterTexture.png", waterBytes);
        System.IO.File.WriteAllBytes(Application.dataPath + "/NoiseTexture.png", noiseBytes);

        Debug.Log("Текстури згенеровано та збережено в папці Assets");
    }
} 