using UnityEditor;

namespace VirtualBeings
{
    public class BeingAssetDataImporter : AssetPostprocessor
    {
        void OnPreprocessAsset()
        {
            if (assetImporter.importSettingsMissing)
            {
                AssetImporter modelImporter = assetImporter as AssetImporter;
            }
        }
    }
}