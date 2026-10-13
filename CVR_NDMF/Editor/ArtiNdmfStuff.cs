using ABI.CCK.Components;
using UnityEngine;
using nadena.dev.ndmf;
using NAK.SimpleAAS;
using NAK.SimpleAAS.Components;

[assembly: ExportsPlugin(typeof(CVR_NDMF.ArtiNdmfStuff))]

namespace CVR_NDMF
{
    [RunsOnPlatforms(CVR_NDMF_Provider.PlatformName)]
    public class ArtiNdmfStuff : Plugin<ArtiNdmfStuff>
    {
        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming).Run(CompileSimpleAAS.Instance);
            InPhase(BuildPhase.PlatformFinish).Run(FixAnimatorPass.Instance);
        }
    }

    [RunsOnPlatforms(CVR_NDMF_Provider.PlatformName)]
    internal class FixAnimatorPass : Pass<FixAnimatorPass>
    {
        protected override void Execute(BuildContext context)
        {
            var anim = context.AvatarRootTransform.GetComponent<Animator>()?.runtimeAnimatorController;
            if (!anim)
            {
                Debug.LogWarning("Failed to find animator component");
                return;
            }

            var avatar = context.AvatarRootTransform.GetComponent<CVRAvatar>();
            if (!avatar)
            {
                Debug.LogWarning("Failed to find CCK component");
                return;
            }

            var aoc = new AnimatorOverrideController(anim)
            {
                name = anim.name + " Override Controller"
            };
            context.AssetSaver.SaveAsset(aoc);
            avatar.overrides = aoc;
        }
    }
    
    [RunsOnPlatforms(CVR_NDMF_Provider.PlatformName)]
    internal class CompileSimpleAAS : Pass<CompileSimpleAAS>
    {
        protected override void Execute(BuildContext context)
        {
            Debug.Log("Compiling AAS " + context.PlatformProvider.QualifiedName);
            Debug.Log(context.AvatarRootObject.name);
            Debug.Log(context.AvatarRootTransform.GetChild(0).name);
            
            var comps = context.AvatarRootTransform.GetComponentsInChildren<NAKSimpleAAS>();
            foreach (var aas in comps)
            {
                ControllerCompiler.CompileControllers(aas);
                Object.DestroyImmediate(aas);
            }
        }
    }

}
