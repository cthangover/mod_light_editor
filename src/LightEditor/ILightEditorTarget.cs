namespace Cthangover.Core.UI.Tool.LightEditor
{
    using System;
    using Godot;

    /// <summary>A mod-supplied surface and persistence adapter for the existing 2D light editor.</summary>
    public interface ILightEditorTarget : IDisposable
    {
        /// <summary>The adapter-owned canvas and source selectors, transferred to the window for its lifetime.</summary>
        Control View { get; }
        /// <summary>Raised after a canvas edit; the window updates its shared fields and dirty state.</summary>
        event Action Changed;
        /// <summary>Raised when a canvas handle selects a different shared light.</summary>
        event Action SelectionChanged;
        /// <summary>True when the resource document has unsaved changes.</summary>
        bool IsDirty { get; }
        /// <summary>Target-specific radius units and range for the common property controls.</summary>
        /// <summary>Whether the source coordinates must remain within the surface's normalized bounds.</summary>
        bool ClampPosition { get; }
        float RadiusMinimum { get; }
        float RadiusMaximum { get; }
        float RadiusStep { get; }
        string RadiusHintKey { get; }
        /// <summary>Localized instructions for this surface.</summary>
        string HintKey { get; }
        /// <summary>The resource data shown by the source panel and copied to the clipboard.</summary>
        string SourceText { get; }
        /// <summary>Opaque text naming the opened resource, or null when nothing is open; the window keeps it across a mod reload.</summary>
        string Location { get; }
        /// <summary>Initializes a document and configures the shared controller; called before attaching View.</summary>
        void Open(LightEditorController controller);
        /// <summary>Opens the resource named by an earlier Location; does nothing when it no longer exists.</summary>
        void Reopen(string location);
        /// <summary>Commits the shared light list through the resource document's history.</summary>
        void Commit();
        /// <summary>Re-presents the target using the controller's current values.</summary>
        void Refresh();
        /// <summary>Writes only changed resources or patches, then refreshes the running scene.</summary>
        bool Save(out string error);
        /// <summary>Handles target-specific shortcuts, such as undo and redo.</summary>
        bool HandleKey(InputEventKey key);
    }
}
