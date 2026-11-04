namespace Cthangover.Core.UI.Tool.LightEditor
{
    /// <summary>Optional mod extension discovered by the light editor through explicit lifetime-bound registration.</summary>
    public interface ILightEditorTargetProvider
    {
        /// <summary>Stable target kind id, independent of a localized display name.</summary>
        string Id { get; }
        /// <summary>Display name key owned by the adapter mod.</summary>
        string LocaleKey { get; }
        /// <summary>Creates one document/canvas session per editor window.</summary>
        ILightEditorTarget Create();
    }
}
