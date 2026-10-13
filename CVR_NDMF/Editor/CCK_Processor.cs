using System;
using System.Runtime.CompilerServices;
using CVR.CCKEditor.ContentBuilder;
using nadena.dev.ndmf;
using nadena.dev.ndmf.config;
using UnityEditor;
using UnityEngine;

namespace CVR_NDMF
{
    public class CCK_Processor : CCKBuildProcessor
    {
        public override int CallbackOrder => -11000;
        public override void OnPreProcessAvatar(GameObject avatar)
        {
            base.OnPreProcessAvatar(avatar);
            
            var isPlayMode = EditorApplication.isPlayingOrWillChangePlaymode;
            var shouldApply = isPlayMode ? Config.ApplyOnPlay : Config.ApplyOnBuild;
            if (!shouldApply) return;
            
            var state = HookDedup.RecordAvatar(avatar);
            if (state.ranEarlyHook) return;
            state.ranEarlyHook = true;
            
            
            try
            {
                AvatarProcessor.ProcessAvatar(avatar, CVR_NDMF_Provider.Instance);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
    }
    
    
    /// <summary>
    /// Ensures that we don't run hooks more than once. This is in preparation for coordinating with VRCFury to have all
    /// nondestructive utilities independently execute hooks as part of Apply on Play, while deduplicating to ensure that
    /// we don't rerun hooks on the same avatar.
    /// </summary>
    internal static class HookDedup
    {
        internal class State
        {
            internal bool ranEarlyHook;
            internal bool ranOptimization;
        }

        private static ConditionalWeakTable<GameObject, State> _avatars = new ConditionalWeakTable<GameObject, State>();
        
        public static State RecordAvatar(GameObject root)
        {
            if (_avatars.TryGetValue(root, out var state))
            {
                return state;
            }

            state = new State();
            _avatars.Add(root, state);

            return state;
        }

        public static bool HasAvatar(GameObject root)
        {
            return _avatars.TryGetValue(root, out _);
        }
    }
}