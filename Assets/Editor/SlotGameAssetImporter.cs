using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using SlotMachineGame.Data;

namespace SlotMachineGame.Editor
{
    /// <summary>
    /// Automates the importation, sprite slicing, and ScriptableObject creation for slot machine assets.
    /// </summary>
    public static class SlotGameAssetImporter
    {
        private const string ProvidedArtPath = "Assets/Art/ProvidedAssets";
        private const string ScriptableObjectsPath = "Assets/ScriptableObjects";

        [MenuItem("SlotMachine/Import & Configure All Assets", false, 1)]
        public static void ImportAndConfigureAll()
        {
            EnsureDirectories();
            ConfigureTextureImporters();
            CreateOrUpdateScriptableObjects();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SlotGameAssetImporter] Asset import, slicing, and ScriptableObject setup complete!");
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        private static void ConfigureTextureImporters()
        {
            // Configure single sprites
            string[] singleSprites = new string[]
            {
                "bg_gradient.png",
                "slot-machine1.png",
                "slot-machine2.png",
                "slot-machine3.png",
                "slot-machine4.png",
                "slot-machine5.png",
                "slot_machine_Middle_box.png",
                "slot-symbol1.png",
                "slot-symbol2.png",
                "slot-symbol3.png",
                "slot-symbol4.png",
                "popup.png"
            };

            foreach (var file in singleSprites)
            {
                string path = $"{ProvidedArtPath}/{file}";
                if (File.Exists(path))
                {
                    ConfigureSingleSprite(path);
                }
            }

            // Configure button strips (256x1024, 4 vertical slices of 256x256)
            string[] buttonStrips = new string[]
            {
                "slot_machine_buttons-02.png",
                "slot_machine_buttons-03.png",
                "slot_machine_buttons-04.png"
            };

            foreach (var strip in buttonStrips)
            {
                string path = $"{ProvidedArtPath}/{strip}";
                if (File.Exists(path))
                {
                    ConfigureButtonStrip(path, Path.GetFileNameWithoutExtension(strip));
                }
            }

            // Configure Yes/No button sheet (990x690, 2 columns x 3 rows of 495x230)
            string ynPath = $"{ProvidedArtPath}/Yes_No_Btn.png";
            if (File.Exists(ynPath))
            {
                ConfigureYesNoSheet(ynPath);
            }
        }

        private static void ConfigureSingleSprite(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            bool modified = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                modified = true;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                modified = true;
            }
            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                modified = true;
            }
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                modified = true;
            }

            if (modified)
            {
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }

        private static void ConfigureButtonStrip(string assetPath, string baseName)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;

            // In Unity 6 / modern Unity, slice metadata can be set via spritesheet
            var sheet = new SpriteMetaData[4];
            string[] states = new string[] { "Normal", "Hover", "Pressed", "Disabled" };

            // In Unity textures, Y = 0 is at bottom
            for (int i = 0; i < 4; i++)
            {
                sheet[i] = new SpriteMetaData
                {
                    name = $"{baseName}_{states[i]}",
                    rect = new Rect(0, (3 - i) * 256, 256, 256),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                };
            }

            importer.spritesheet = sheet;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        private static void ConfigureYesNoSheet(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;

            // 2 cols x 3 rows: width 495, height 230
            var sheet = new SpriteMetaData[6];
            string[] cols = new string[] { "Yes", "No" };
            string[] states = new string[] { "Normal", "Hover", "Pressed" };

            int index = 0;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 2; col++)
                {
                    sheet[index] = new SpriteMetaData
                    {
                        name = $"Btn_{cols[col]}_{states[row]}",
                        rect = new Rect(col * 495, (2 - row) * 230, 495, 230),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    };
                    index++;
                }
            }

            importer.spritesheet = sheet;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        private static void CreateOrUpdateScriptableObjects()
        {
            // Load symbol sprites
            Sprite s1 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ProvidedArtPath}/slot-symbol1.png");
            Sprite s2 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ProvidedArtPath}/slot-symbol2.png");
            Sprite s3 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ProvidedArtPath}/slot-symbol3.png");
            Sprite s4 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ProvidedArtPath}/slot-symbol4.png");

            // Symbol Database
            string dbPath = $"{ScriptableObjectsPath}/SymbolDatabase.asset";
            SymbolDatabase db = AssetDatabase.LoadAssetAtPath<SymbolDatabase>(dbPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<SymbolDatabase>();
                AssetDatabase.CreateAsset(db, dbPath);
            }

            var symbolList = new List<SymbolData>
            {
                new SymbolData(1, "Lucky 7", s1, 10),
                new SymbolData(2, "Cherries", s2, 5),
                new SymbolData(3, "Bell", s3, 4),
                new SymbolData(4, "BAR", s4, 3)
            };
            db.SetSymbols(symbolList);
            EditorUtility.SetDirty(db);

            // Payout Configuration
            string payoutPath = $"{ScriptableObjectsPath}/PayoutConfiguration.asset";
            PayoutConfiguration payout = AssetDatabase.LoadAssetAtPath<PayoutConfiguration>(payoutPath);
            if (payout == null)
            {
                payout = ScriptableObject.CreateInstance<PayoutConfiguration>();
                AssetDatabase.CreateAsset(payout, payoutPath);
            }

            var entries = new List<SymbolPayoutEntry>
            {
                new SymbolPayoutEntry(1, 10), // Lucky 7: 10x
                new SymbolPayoutEntry(2, 5),  // Cherries: 5x
                new SymbolPayoutEntry(3, 4),  // Bell: 4x
                new SymbolPayoutEntry(4, 3)   // BAR: 3x
            };
            payout.SetEntries(entries);
            EditorUtility.SetDirty(payout);
        }
    }
}

