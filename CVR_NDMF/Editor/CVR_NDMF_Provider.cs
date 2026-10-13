using System;
using System.Linq;
using System.Reflection;
using ABI.CCK.Components;
using nadena.dev.ndmf.platform;
using UnityEngine;
using UnityEditor;

namespace CVR_NDMF
{
    [NDMFPlatformProvider]
    public class CVR_NDMF_Provider : INDMFPlatformProvider
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            // AmbientPlatform.DefaultPlatform = Instance;
            // using reflection due to 'internal' modifier
            var prop = typeof(AmbientPlatform)
                .GetProperty(nameof(AmbientPlatform.DefaultPlatform),
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            
            if (prop == null)
            {
                Debug.LogError("Cannot set NDMF DefaultPlatform, property not found");
                return;
            }

            prop.SetValue(null, Instance);
        }

        public static INDMFPlatformProvider Instance { get; } = new CVR_NDMF_Provider();

        public const string PlatformName = "com.chilloutvr";
        public string QualifiedName => PlatformName;

        public string DisplayName => "ChilloutVR";

        // todo: icon
        public bool HasNativeConfigData => true;

        public Type AvatarRootComponentType => typeof(CVRAvatar);

        // TODO: buildui?
        public bool HasNativeUI => true;

        public void OpenNativeUI()
        {
            var openFunc = typeof(CCKControlPanel)
                .GetMethod("ShowWindow", BindingFlags.Static | BindingFlags.NonPublic);
            if (openFunc == null)
            {
                Debug.LogError("Failed to open native ChilloutVR window.");
                return;
            }

            openFunc.Invoke(null, null);
        }

        private static readonly string[] _visemeNames =
            { "sil", "PP", "FF", "TH", "DD", "kk", "CH", "SS", "nn", "RR", "aa", "E", "ih", "oh", "ou" };

        public CommonAvatarInfo ExtractCommonAvatarInfo(GameObject avatarRoot)
        {
            var cvrComponent = avatarRoot.GetComponent<CVRAvatar>();
            if (!cvrComponent) return new CommonAvatarInfo();

            var avInfo = new CommonAvatarInfo()
            {
                EyePosition = avatarRoot.transform.InverseTransformVector(cvrComponent.viewPosition),
                VisemeRenderer = cvrComponent.bodyMesh
            };
            if (cvrComponent.visemeMode == CVRAvatar.CVRAvatarVisemeMode.Visemes &&
                cvrComponent.visemeBlendshapes.Length == _visemeNames.Length)
            {
                foreach (var (viseme, blenshape) in
                         _visemeNames.Zip(cvrComponent.visemeBlendshapes, (a, b) => (a, b)))
                {
                    avInfo.VisemeBlendshapes.Add(viseme, blenshape);
                }
            }

            return avInfo;
        }

        // TODO: add it
        public bool CanInitFromCommonAvatarInfo(GameObject avatarRoot, CommonAvatarInfo info) => false;

        public void InitFromCommonAvatarInfo(GameObject avatarRoot, CommonAvatarInfo info)
        {
        }

        // TODO: this func replaces all platform-physbone with generic physbone
        public void GeneratePortableComponents(GameObject avatarRoot, bool registerUndo)
        {
        }
    }
}