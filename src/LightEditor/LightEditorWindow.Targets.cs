namespace Cthangover.Core.UI.Tool.LightEditor
{
    using System;
    using System.Collections.Generic;
    using Cthangover.Mods.Interface.Tool;
    using Cthangover.Tools.Common;
    using Godot;

    /// <summary>The existing window keeps its property controls while optional mods supply other 2D surfaces and resource writers.</summary>
    public partial class LightEditorWindow
    {
        private OptionButton _targetSelect;
        private Button _copyButton;
        private Button _lineToggle;
        private PanelContainer _canvasPanel;
        private Control _backgroundTitle;
        private ILightEditorTarget _target;
        private string _targetId;
        private readonly List<ILightEditorTargetProvider> _providers = new();
        private int _pendingTarget;
        /// <summary>The active optional surface adapter, or null for ordinary dialog backgrounds.</summary>
        public ILightEditorTarget ActiveTarget => _target;
        /// <summary>The shared light editing session for the current target.</summary>
        public LightEditorController Controller => _controller;
        private void BuildTargetPicker(HBoxContainer toolbar)
        {
            _targetSelect = new OptionButton { Name = "LightTargetKind" };
            _targetSelect.ItemSelected += OnTargetPicked;
            toolbar.AddChild(_targetSelect);
        }
        private void InstallTargets()
        {
            _targetSelect.AddItem(ToolUi.Tr("tools/light_editor/background_target"));
            _providers.AddRange(LightEditorTargets.All);
            foreach (var provider in _providers) _targetSelect.AddItem(ToolUi.Tr(provider.LocaleKey));
            LightEditorTargets.Removing += OnTargetRemoving;
        }
        private void OnTargetPicked(long index)
        {
            _pendingTarget = (int)index;
            RunDiscardingChanges(ApplyTarget, RestoreTargetSelection);
        }
        private void RestoreTargetSelection()
        {
            var index = _providers.FindIndex(IsActiveProvider);
            _targetSelect.Select(_target == null ? 0 : index + 1);
        }
        private bool IsActiveProvider(ILightEditorTargetProvider provider) => provider.Id == _targetId;
        private void ApplyTarget()
        {
            _suppress = true;
            DropTarget();
            _controller.Limit = LightEditorController.MaxLights;
            _controller.NewRadius = 400f;
            _controller.UseIds = false;
            _x.AllowLesser = _x.AllowGreater = false;
            _y.AllowLesser = _y.AllowGreater = false;
            _x.Step = 0.001;
            _y.Step = 0.001;
            _radius.MinValue = 10;
            _radius.MaxValue = 3000;
            _radius.Step = 10;
            _radius.TooltipText = ToolUi.Tr("tools/light_editor/radius_tip");
            if (_pendingTarget > 0 && _pendingTarget <= _providers.Count)
            {
                var provider = _providers[_pendingTarget - 1];
                _targetId = provider.Id;
                _target = provider.Create();
                _target.Open(_controller);
                _target.Changed += OnCanvasChanged;
                _target.SelectionChanged += RefreshAll;
                _canvasPanel.AddChild(_target.View);
                _x.AllowLesser = _x.AllowGreater = !_target.ClampPosition;
                _y.AllowLesser = _y.AllowGreater = !_target.ClampPosition;
                _x.Step = 0.000001;
                _y.Step = 0.000001;
                _radius.MinValue = _target.RadiusMinimum;
                _radius.MaxValue = _target.RadiusMaximum;
                _radius.Step = _target.RadiusStep;
                _radius.TooltipText = ToolUi.Tr(_target.RadiusHintKey);
                SetHint(ToolUi.Tr(_target.HintKey));
            }
            else OpenScenario(_scenario);
            _canvas.Visible = _target == null;
            _bgList.Visible = _target == null;
            _backgroundTitle.Visible = _target == null;
            _scenarioSelect.Visible = _target == null;
            var freePosition = _target != null && !_target.ClampPosition;
            _x.GetParent().GetChild<Label>(0).Text = ToolUi.Tr(freePosition ? "tools/light_editor/free_x" : "tools/light_editor/x");
            _y.GetParent().GetChild<Label>(0).Text = ToolUi.Tr(freePosition ? "tools/light_editor/free_y" : "tools/light_editor/y");
            _x.TooltipText = _y.TooltipText = freePosition ? ToolUi.Tr("tools/light_editor/free_position_tip") : "";
            _lineToggle.Text = ToolUi.Tr(_target == null ? "tools/light_editor/show_line" : "tools/light_editor/show_data");
            _copyButton.Text = ToolUi.Tr(_target == null ? "tools/light_editor/copy_line" : "tools/light_editor/copy_data");
            _copyButton.TooltipText = ToolUi.Tr(_target == null ? "tools/light_editor/copy_line_tip" : "tools/light_editor/copy_data");
            if (_target == null) SetHint(ToolUi.Tr("tools/light_editor/hint"));
            _suppress = false;
            MarkClean();
            RefreshAll();
        }
        private void RefreshCanvas()
        {
            if (_target != null) _target.Refresh();
            else _canvas.RebuildHandles();
        }
        private string CurrentSourceText() => _target?.SourceText ?? _controller.ToScenarioLine();
        private void SyncTargetDirty()
        {
            if (_target == null || _target.IsDirty) SetDirty();
            else MarkClean();
        }
        private void DropTarget()
        {
            if (_target == null) return;
            _target.Changed -= OnCanvasChanged;
            _target.SelectionChanged -= RefreshAll;
            var view = _target.View;
            _target.Dispose();
            if (view.GetParent() != null) view.GetParent().RemoveChild(view);
            view.Free();
            _target = null;
            _targetId = null;
        }
        private void OnTargetRemoving(string id)
        {
            if (_targetId == id) { _pendingTarget = 0; ApplyTarget(); }
            _providers.RemoveAll(provider => provider.Id == id);
            _targetSelect.Clear();
            _targetSelect.AddItem(ToolUi.Tr("tools/light_editor/background_target"));
            foreach (var provider in _providers) _targetSelect.AddItem(ToolUi.Tr(provider.LocaleKey));
        }
        /// <summary>Releases the adapter before either the editor or its supplying mod unloads.</summary>
        protected override void Cleanup()
        {
            LightEditorTargets.Removing -= OnTargetRemoving;
            DropTarget();
            _providers.Clear();
            base.Cleanup();
        }
    }
}
