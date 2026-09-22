using UnityEditor;
public class BetaArtImport : AssetPostprocessor
{
    void OnPreprocessTexture(){if(!assetPath.Contains("AshfallAtlas")&&!assetPath.Contains("AshfallHero")&&!assetPath.Contains("AshfallWeapon"))return;var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.filterMode=UnityEngine.FilterMode.Point;t.maxTextureSize=assetPath.Contains("AshfallWeapon")||assetPath.Contains("Frames")||assetPath.Contains("Details")?2048:1024;t.textureCompression=TextureImporterCompression.Uncompressed;t.isReadable=true;}
}
