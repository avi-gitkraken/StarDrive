﻿using System;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SDUtils;
using SgMotion;
using Ship_Game.Data.Mesh;
using Ship_Game.Data.Texture;

namespace Ship_Game.Data
{
    /// <summary>
    /// Helper class for GameContentManager
    /// Allows loading FBX, OBJ and PNG files instead of .XNB content
    /// </summary>
    public class RawContentLoader
    {
        readonly GameContentManager Content;
        public readonly TextureImporter TexImport;
        public readonly TextureExporter TexExport;
        readonly MeshImporter MeshImport;
        readonly MeshExporter MeshExport;

        public RawContentLoader(GameContentManager content)
        {
            Content = content;
            TexImport = new TextureImporter(content);
            TexExport = new TextureExporter(content);
            MeshImport = new MeshImporter(content);
            MeshExport = new MeshExporter(content);
        }

        public static bool IsSupportedMesh(string modelNameWithExtension)
        {
            return IsSupportedMeshExtension(Path.GetExtension(modelNameWithExtension));
        }

        public static bool IsSupportedMeshExtension(string extension)
        {
            if (extension.IsEmpty())
                return false;
            if (extension[0] == '.')
                return extension.Equals(".fbx", StringComparison.OrdinalIgnoreCase)
                    || extension.Equals(".obj", StringComparison.OrdinalIgnoreCase);
            return extension.Equals("fbx", StringComparison.OrdinalIgnoreCase)
                || extension.Equals("obj", StringComparison.OrdinalIgnoreCase);
        }
        
        public static string GetContentPath(string contentName)
        {
            if (contentName.StartsWith("Mods/", StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(contentName))
                    return contentName;
            }
            else if (GlobalStats.HasMod)
            {
                string modPath = GlobalStats.ModPath + contentName;
                if (File.Exists(modPath)) return modPath;
            }
            else if (contentName.StartsWith("Content/"))
            {
                return contentName;
            }
            return "Content/" + contentName;
        }

        public object LoadAsset(Type type, string fileNameWithExt, string ext)
        {
            if (IsSupportedMeshExtension(ext))
            {
                string meshPath = GetContentPath(fileNameWithExt);
                if (type == typeof(StaticMesh))
                {
                    Log.Info(ConsoleColor.Magenta, $"Raw LoadStaticMesh: {fileNameWithExt}");
                    return MeshImport.ImportStaticMesh(meshPath, fileNameWithExt);
                }
                else if (type == typeof(Model))
                {
                    return MeshImport.ImportModel(meshPath, fileNameWithExt);
                }
            }

            //Log.Info(ConsoleColor.Magenta, $"Raw LoadTexture: {fileNameWithExt}");
            return LoadContentTexture(fileNameWithExt);
        }

        ///////////////////////////////////////////////////

        // converts `fileNameWithExt` into Content relative path
        public Texture2D LoadContentTexture(string fileNameWithExt)
        {
            string contentPath = GetContentPath(fileNameWithExt);
            return TexImport.Load(contentPath);
        }

        public Texture2D LoadTexture(FileInfo file)
        {
            return TexImport.Load(file);
        }

        public Texture2D LoadTexture(string fileNameWithExt)
        {
            string contentPath = GetContentPath(fileNameWithExt);
            return TexImport.Load(contentPath);
        }

        // loads an RGB texture and converts it to an AlphaMap, from RGB luminosity
        /// <summary>
        /// loads an RGB texture and converts it to an AlphaMap, from RGB luminosity
        /// </summary>
        /// <param name="fileNameWithExt"></param>
        /// <param name="toPreMultipliedAlpha">If true, pixel=[A,A,A,A] if false, pixel=[255,255,255,A]</param>
        public Texture2D LoadAlphaTexture(string fileNameWithExt, bool toPreMultipliedAlpha)
        {
            string contentPath = GetContentPath(fileNameWithExt);
            Texture2D tex = TexImport.Load(contentPath);
            ImageUtils.ConvertToAlphaMap(tex, toPreMultipliedAlpha: toPreMultipliedAlpha);
            return tex;
        }

        ///////////////////////////////////////////////////

        public StaticMesh LoadStaticMesh(string meshName)
        {
            string meshPath = GetContentPath(meshName);
            return MeshImport.ImportStaticMesh(meshPath, meshName);
        }

        public Model LoadModel(string meshName)
        {
            string meshPath = GetContentPath(meshName);
            return MeshImport.ImportModel(meshPath, meshName);
        }

        public Array<FileInfo> GetAllXnbModelFiles(string folder)
        {
            var files = new Array<FileInfo>();
            files.AddRange(Dir.GetFiles($"Content/{folder}", "*.xnb", SearchOption.AllDirectories));
            if (GlobalStats.HasMod)
                files.AddRange(Dir.GetFiles($"{GlobalStats.ModPath}{folder}", "*.xnb", SearchOption.AllDirectories));

            var modelFiles = new Array<FileInfo>();
            for (int i = 0; i < files.Count; ++i)
            {
                FileInfo file = files[i];
                string name = file.Name;
                if (name.EndsWith("_d.xnb") || name.EndsWith("_g.xnb") ||
                    name.EndsWith("_n.xnb") || name.EndsWith("_s.xnb") ||
                    name.EndsWith("_d_0.xnb") || name.EndsWith("_g_0.xnb") ||
                    name.EndsWith("_n_0.xnb") || name.EndsWith("_s_0.xnb"))
                {
                    continue;
                }
                modelFiles.Add(file);
            }
            return modelFiles;
        }

        public void ExportXnbMesh(FileInfo file, string meshExtension, bool alwaysOverwrite = true)
        {
            string relativePath = file.RelPath();
            Log.Write(relativePath);

            if (relativePath.StartsWith("Content\\"))
                relativePath = relativePath.Substring(8);

            string savePath = "MeshExport\\" + Path.ChangeExtension(relativePath, meshExtension);
            if (!alwaysOverwrite && File.Exists(savePath))
                return;

            string nameNoExt = Path.GetFileNameWithoutExtension(file.Name);
            bool isTexture2D = false;
            bool isTexture3D = false;
            try
            {
                Model model = Content.LoadModel(relativePath);
                Log.Write(ConsoleColor.Blue, $"  Export StaticMesh: {savePath}");
                GameLoadingScreen.SetStatus("ExportMesh", savePath);
                MeshExport.Export(model, nameNoExt, savePath);
                return;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("File contains Microsoft.Xna.Framework.Graphics.Texture2D") ||
                    e.Message.Contains("already loaded as 'Microsoft.Xna.Framework.Graphics.Texture2D"))
                    isTexture2D = true;
                else if(e.Message.Contains("File contains Microsoft.Xna.Framework.Graphics.Texture3D") ||
                    e.Message.Contains("already loaded as 'Microsoft.Xna.Framework.Graphics.Texture3D"))
                    isTexture3D = true;
                else if (e.Message.Contains("File contains Microsoft.Xna.Framework.Graphics.") ||
                    e.Message.Contains("already loaded as 'Microsoft.Xna.Framework.Graphics."))
                    return; // ignore this one
            }

            if (isTexture2D || isTexture3D)
            {
                // but then just export it as texture instead
                // because we might need it later
                try
                {
                    if (isTexture2D)
                    {
                        var tex = Content.Load<Texture2D>(relativePath);
                        if (!MeshExport.IsAlreadySavedTexture(tex))
                        {
                            string texSavePath = TexExport.GetSaveAutoFormatPath(tex, savePath);
                            texSavePath = texSavePath.Replace("_0.", ".");
                            Log.Write(ConsoleColor.Green, $"  Export Lone Texture: {texSavePath}");
                            GameLoadingScreen.SetStatus("ExportTexture", texSavePath);
                            TexExport.SaveAutoFormat(tex, texSavePath);
                            MeshExport.AddAlreadySavedTexture(tex, texSavePath);
                        }
                    }
                    else
                    {
                        var tex3d = Content.Load<Texture3D>(relativePath);
                        string texSavePath = Path.ChangeExtension(savePath, "dds");
                        Log.Write(ConsoleColor.DarkYellow, $"  Export Lone Texture3D: {texSavePath}");
                        GameLoadingScreen.SetStatus("ExportTexture", texSavePath);
                        TexExport.Save(tex3d, texSavePath);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to load Texture");
                }
                return;
            }

            try
            {
                SkinnedModel model = Content.LoadSkinnedModel(relativePath);
                Log.Write(ConsoleColor.Cyan, $"  Export AnimatedMesh: {savePath}");
                GameLoadingScreen.SetStatus("ExportMesh", savePath);
                MeshExport.Export(model, nameNoExt, savePath);
            }
            catch (ContentLoadException e)
            {
                Log.Warning($"Failed to export {relativePath}: {e.Message}");
            }
        }

        public void ExportAllXnbMeshes(string extension)
        {
            var files = new Array<FileInfo>();
            //files.AddRange(GetAllXnbModelFiles("Effects"));
            //files.AddRange(GetAllXnbModelFiles("mod models"));
            //files.AddRange(GetAllXnbModelFiles("Model"));
            files.AddRange(GetAllXnbModelFiles("Model/SpaceObjects"));

            void ExportMeshes(int start, int end)
            {
                for (int i = start; i < end; ++i)
                {
                    ExportXnbMesh(files[i], extension);
                }
            }
            //Parallel.For(files.Count, ExportMeshes, Parallel.NumPhysicalCores * 2);
            ExportMeshes(0, files.Count);
            MeshExport.Reset();
        }

        public void ExportAllTextures()
        {
            string outDir = Path.GetFullPath("ExportedTextures");
            Log.Write(ConsoleColor.Blue, $"ExportTextures to: {outDir}");

            var files = new Array<FileInfo>();
            files.AddRange(Dir.GetFiles("Content/", "xnb"));
            //files.AddRange(Dir.GetFiles("Content/", "png"));
            if (GlobalStats.HasMod)
            {
                files.AddRange(Dir.GetFiles(GlobalStats.ModPath, "xnb"));
                //files.AddRange(Dir.GetFiles(GlobalStats.ModPath, "png"));
            }

            //foreach (var f in files) ExportTexture(f, outDir);
            Parallel.ForEach(files, f => ExportTexture(f, outDir));
        }

        void ExportTexture(FileInfo file, string outDir)
        {
            string relPath = file.RelPath();
            string outFile = Path.Combine(outDir, relPath);
            try
            {
                GameLoadingScreen.SetStatus("ExportTexture", outFile);
                string ext = file.Extension.Remove(0, 1).ToLower(); // '.Xnb' -> 'xnb'
                using Texture2D tex = Content.LoadUncachedTexture(file, ext);
                if (TexExport.SaveAutoFormat(tex, outFile))
                    Log.Write(ConsoleColor.Green, $"Saved {outFile}");
                else
                    Log.Write(ConsoleColor.DarkYellow, $"Ignored {relPath}");
            }
            catch (Exception e) // not a texture
            {
                if (e.Message.Contains("File contains Microsoft.Xna.Framework.Graphics."))
                    Log.Write(ConsoleColor.DarkYellow, $"Ignored NotATexture2D {relPath}");
                else
                    Log.Write(ConsoleColor.Red, $"ExportFail {relPath} : {e.Message}");
            }
        }
    }
}
