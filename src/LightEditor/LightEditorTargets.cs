namespace Cthangover.Core.UI.Tool.LightEditor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>Extensions belong to their supplying mod and are removed before its assembly unloads.</summary>
    public static class LightEditorTargets
    {
        private static readonly Dictionary<string, ILightEditorTargetProvider> providers = new(StringComparer.Ordinal);
        /// <summary>Allows open windows to release an adapter while its owner is still callable.</summary>
        public static event Action<string> Removing;
        /// <summary>Raised after a provider registers, so open windows can list it again after a mod reload.</summary>
        public static event Action<string> Added;
        /// <summary>Current providers, without a persistent cache across games.</summary>
        public static IReadOnlyList<ILightEditorTargetProvider> All => providers.Values.OrderBy(ItemId).ToArray();
        private static string ItemId(ILightEditorTargetProvider provider) => provider.Id;
        /// <summary>Registers one extension; a duplicate id is a real mod conflict.</summary>
        public static void Register(ILightEditorTargetProvider provider)
        {
            if (provider == null || string.IsNullOrWhiteSpace(provider.Id) || !providers.TryAdd(provider.Id, provider))
                throw new InvalidOperationException("Duplicate or invalid light editor target.");
            Added?.Invoke(provider.Id);
        }
        /// <summary>Removes only the provider instance owned by the unloading mod.</summary>
        public static void Unregister(ILightEditorTargetProvider provider)
        {
            if (provider == null || !providers.TryGetValue(provider.Id, out var current) || !ReferenceEquals(current, provider)) return;
            Removing?.Invoke(provider.Id);
            providers.Remove(provider.Id);
        }
    }
}
