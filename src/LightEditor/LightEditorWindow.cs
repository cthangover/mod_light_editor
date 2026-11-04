using System;
using System.Collections.Generic;
using System.Linq;
using Cthangover.Core.Factories.Impls;
using Cthangover.Core.UI.Theming;
using Cthangover.Core.Utils;
using Cthangover.Mods.Interface.Theming;
using Cthangover.Mods.Interface.Tool;
using Cthangover.Tools.Common;
using Cthangover.Tools.Services;
using Godot;

namespace Cthangover.Core.UI.Tool.LightEditor
{
    /// <summary>
    /// Editor of the static lights of a scene. The author picks a scenario —
    /// the ones that already set lights come first — and edits its
    /// <c>light_set</c> line visually: drag lights on a night-time preview of
    /// the scene background, double-click to add one, tune radius, influence
    /// and colour. Saving rewrites that line in the scenario (or inserts it
    /// after the header). The background follows the scenario's scene.
    /// </summary>
    public partial class LightEditorWindow : ToolWindow
    {
        private readonly LightEditorController _controller = new();
        private readonly List<ScenarioFileInfo> _scenarios = new();
        private readonly List<string> _backgroundIds = new();

        private ScenarioFileInfo _scenario;
        private string _scenarioText;
        private bool _suppress;

        private OptionButton _scenarioSelect;
        private LightEditorCanvas _canvas;
        private ItemList _lightList;
        private Button _addBtn;
        private Button _deleteBtn;
        private VBoxContainer _propsBody;
        private Label _propsEmpty;
        private SpinBox _x;
        private SpinBox _y;
        private SpinBox _radius;
        private SpinBox _influence;
        private ColorPickerButton _color;
        private FilteredList _bgList;
        private CodeEdit _lineEdit;

        public LightEditorWindow() : base("tools/light_editor/title")
        {
            CthangoverTheme.Apply(this);
            BuildUI();
            LoadBackgrounds();
            LoadScenarioList();
            InstallTargets();
            SetHint(ToolUi.Tr("tools/light_editor/hint"));
        }

        // ── UI construction ────────────────────────────────────────

        private void BuildUI()
        {
            var outer = CreateFillContainer();
            AddChild(outer);

            var toolbar = CreateToolbar();
            outer.AddChild(toolbar);
            BuildTargetPicker(toolbar);
            _scenarioSelect = new OptionButton { CustomMinimumSize = new Vector2(420, 0), TooltipText = ToolUi.Tr("tools/light_editor/scenario_tip") };
            _scenarioSelect.ItemSelected += OnScenarioPicked;
            toolbar.AddChild(_scenarioSelect);
            toolbar.AddChild(ToolUi.Button("tools/common/save", Save, ButtonKind.Primary, "tools/light_editor/save_tip"));
            _copyButton = ToolUi.Button("tools/light_editor/copy_line", CopyLine, ButtonKind.Secondary, "tools/light_editor/copy_line_tip");
            toolbar.AddChild(_copyButton);
            toolbar.AddChild(UiKit.Spacer());
            _lineToggle = ToolUi.Button("tools/light_editor/show_line", null, ButtonKind.Tab);
            _lineToggle.ToggleMode = true;
            _lineToggle.Toggled += on => _lineEdit.Visible = on;
            toolbar.AddChild(_lineToggle);

            var main = new HSplitContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            outer.AddChild(main);

            var side = new VBoxContainer();
            main.AddChild(CreateSidebar(side, 300));
            BuildSidebar(side);

            var canvasColumn = new VSplitContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            main.AddChild(canvasColumn);

            _canvasPanel = new PanelContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            _canvasPanel.AddThemeStyleboxOverride("panel", UiKit.Frame(UiTokens.RadiusMd, UiTokens.Well, UiTokens.Line));
            canvasColumn.AddChild(_canvasPanel);

            _canvas = new LightEditorCanvas
            {
                Controller = _controller,
                PreviewMaterial = LightEditorController.CreatePreviewMaterial()
            };
            _canvas.Changed += OnCanvasChanged;
            _canvas.SelectionChanged += RefreshAll;
            _canvasPanel.AddChild(_canvas);

            _lineEdit = ToolUi.Code(editable: false);
            _lineEdit.SyntaxHighlighter = ScenarioSyntaxService.CreateHighlighter();
            _lineEdit.CustomMinimumSize = new Vector2(0, 70);
            _lineEdit.WrapMode = TextEdit.LineWrappingMode.Boundary;
            _lineEdit.Visible = false;
            canvasColumn.AddChild(_lineEdit);
        }

        private void BuildSidebar(VBoxContainer column)
        {
            column.AddChild(ToolUi.Section("tools/light_editor/static_lights"));
            _lightList = new ItemList { CustomMinimumSize = new Vector2(0, 150) };
            _lightList.ItemSelected += index => { _controller.Select((int)index); RefreshAll(); };
            column.AddChild(_lightList);

            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", UiTokens.Space1);
            column.AddChild(row);
            _addBtn = ToolUi.Button("tools/light_editor/add_light", OnAdd);
            _addBtn.Name = "AddLight";
            _addBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            row.AddChild(_addBtn);
            _deleteBtn = ToolUi.Button("tools/light_editor/delete", OnDelete, ButtonKind.Danger);
            _deleteBtn.Name = "DeleteLight";
            _deleteBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            row.AddChild(_deleteBtn);

            column.AddChild(ToolUi.Section("tools/light_editor/selected_props"));
            _propsEmpty = ToolUi.Hint(ToolUi.Tr("tools/light_editor/none_selected"));
            column.AddChild(_propsEmpty);
            _propsBody = new VBoxContainer();
            _propsBody.AddThemeConstantOverride("separation", UiTokens.Space2);
            column.AddChild(_propsBody);

            _x = ToolUi.Field(_propsBody, "tools/light_editor/x", ToolUi.Number(0, 1, 0.001, _ => OnFieldsChanged("x")));
            _x.Name = "LightX";
            _y = ToolUi.Field(_propsBody, "tools/light_editor/y", ToolUi.Number(0, 1, 0.001, _ => OnFieldsChanged("y")));
            _y.Name = "LightY";
            _radius = ToolUi.Field(_propsBody, "tools/light_editor/radius", ToolUi.Number(10, 3000, 10, _ => OnFieldsChanged("radius")), "tools/light_editor/radius_tip");
            _radius.Name = "LightRadius";
            _influence = ToolUi.Field(_propsBody, "tools/light_editor/influence", ToolUi.Number(0, 1, 0.05, _ => OnFieldsChanged("influence")), "tools/light_editor/influence_tip");
            _influence.Name = "LightInfluence";
            _color = ToolUi.Field(_propsBody, "tools/light_editor/color", new ColorPickerButton { CustomMinimumSize = new Vector2(0, 28), EditAlpha = false });
            _color.Name = "LightColor";
            _color.ColorChanged += _ => OnFieldsChanged("color");

            _backgroundTitle = ToolUi.Section("tools/light_editor/background");
            column.AddChild(_backgroundTitle);
            _bgList = new FilteredList(180);
            _bgList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _bgList.Chosen += ShowBackground;
            column.AddChild(_bgList);
        }

        // ── loading ────────────────────────────────────────────────

        private void LoadBackgrounds()
        {
            _backgroundIds.AddRange(ModResourceService.GetBackgroundIds());
            _bgList.SetItems(_backgroundIds);
        }

        /// <summary>Scenarios with a <c>light_set</c> line first (marked ●), then all others.</summary>
        private void LoadScenarioList()
        {
            var withLights = new List<ScenarioFileInfo>();
            var others = new List<ScenarioFileInfo>();
            foreach (var file in ScenarioFileService.ScanScenarioFiles().SelectMany(m => m.Groups).SelectMany(g => g.Files))
            {
                var text = ScenarioFileService.ReadAllText(file.ModId, file.RelativePath);
                (text != null && LightEditorController.HasLights(text) ? withLights : others).Add(file);
            }

            _scenarios.Clear();
            _scenarios.AddRange(withLights);
            _scenarios.AddRange(others);

            _scenarioSelect.Clear();
            _scenarioSelect.AddItem(ToolUi.Tr("tools/light_editor/pick_scenario"));
            foreach (var file in _scenarios)
                _scenarioSelect.AddItem((withLights.Contains(file) ? "●  " : "    ") + file.ModId + ": " + file.RelativePath);
            _scenarioSelect.Select(0);

            if (withLights.Count > 0)
            {
                _scenarioSelect.Select(1);
                OpenScenario(withLights[0]);
            }
            else
            {
                RefreshAll();
            }
        }

        private void OnScenarioPicked(long index)
        {
            var file = index > 0 && index - 1 < _scenarios.Count ? _scenarios[(int)index - 1] : null;
            RunDiscardingChanges(() => OpenScenario(file), () => _scenarioSelect.Select(_scenario == null ? 0 : _scenarios.IndexOf(_scenario) + 1));
        }

        private void OpenScenario(ScenarioFileInfo file)
        {
            _scenario = file;
            _scenarioText = null;
            MarkClean();

            if (file != null)
            {
                _scenarioText = ScenarioFileService.ReadAllText(file.ModId, file.RelativePath) ?? string.Empty;
                try
                {
                    var found = _controller.LoadFromScenario(_scenarioText);
                    SetStatus(string.Format(ToolUi.Tr(found ? "tools/light_editor/loaded" : "tools/light_editor/no_lights_yet"), file.RelativePath));
                }
                catch (Exception ex)
                {
                    SetStatus($"{ToolUi.Tr("tools/common/load_error")}: {ex.Message}", ToolStatusKind.Error);
                    GameLogger.Log("LIGHT_EDITOR", $"light_set parse failed in '{file.Key}': {ex.Message}", LogLevel.Error);
                }

                var scene = ScenarioFileService.ParseMetadata(_scenarioText).Scene;
                var background = SceneBackgroundService.ForScene(scene);
                if (background != null && ModResourceService.LoadBackgroundTexture(background) == null)
                    SetStatus(string.Format(ToolUi.Tr("tools/common/background_missing"), background, scene), ToolStatusKind.Error);
                else if (background != null)
                    ShowBackground(background);
            }
            else
            {
                _controller.LoadFromScenario(string.Empty);
            }

            RefreshAll();
        }

        /// <summary>Shows a background with its optional <c>_depth</c>/<c>_albedo</c> masks in the shader.</summary>
        private void ShowBackground(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            _bgList.Select(id);
            _canvas.SetBackground(ModResourceService.LoadBackgroundTexture(id));
            LightEditorController.SetPreviewTextures(_canvas.PreviewMaterial,
                ModResourceService.LoadBackgroundTexture(id + "_depth"),
                ModResourceService.LoadBackgroundTexture(id + "_albedo"));
            _canvas.Refresh();
        }

        // ── saving ─────────────────────────────────────────────────

        /// <summary>Ctrl+S.</summary>
        protected override void OnSaveShortcut() => Save();

        private void Save()
        {
            if (_target != null)
            {
                _target.Commit();
                if (!_target.Save(out var error)) { SetStatus(error, ToolStatusKind.Error); return; }
                MarkClean();
                SetStatus(ToolUi.Tr("tools/light_editor/target_saved"), ToolStatusKind.Ok);
                return;
            }
            if (_scenario == null)
            {
                SetStatus(ToolUi.Tr("tools/light_editor/pick_scenario"), ToolStatusKind.Error);
                return;
            }

            var updated = _controller.WriteToScenario(_scenarioText);
            if (!ScenarioFileService.WriteAllText(_scenario.ModId, _scenario.RelativePath, updated))
            {
                SetStatus(ToolUi.Tr("tools/common/readonly"), ToolStatusKind.Error);
                return;
            }

            _scenarioText = updated;
            MarkClean();
            SetStatus(string.Format(ToolUi.Tr("tools/common/saved"), _scenario.ModId + "/" + _scenario.RelativePath), ToolStatusKind.Ok);
        }

        private void CopyLine()
        {
            DisplayServer.ClipboardSet(CurrentSourceText());
            SetStatus(ToolUi.Tr(_target == null ? "tools/light_editor/copied" : "tools/light_editor/data_copied"), ToolStatusKind.Ok);
        }

        // ── editing ────────────────────────────────────────────────

        private void OnAdd()
        {
            if (_controller.Add(new Vector2(0.5f, 0.5f)) == null)
            {
                SetStatus(string.Format(ToolUi.Tr("tools/light_editor/limit"), _controller.Limit), ToolStatusKind.Error);
                return;
            }

            _target?.Commit();
            SetDirty();
            RefreshAll();
        }

        private void OnDelete()
        {
            if (_controller.Selected == null)
                return;

            _controller.RemoveSelected();
            _target?.Commit();
            SetDirty();
            RefreshAll();
        }

        /// <summary>Delete removes the selected light.</summary>
        protected override bool HandleKey(InputEventKey key)
        {
            if (_target?.HandleKey(key) == true) { RefreshAll(); SyncTargetDirty(); return true; }
            if (key.Keycode != Key.Delete || _controller.Selected == null)
                return false;
            OnDelete();
            return true;
        }

        private void OnFieldsChanged(string field)
        {
            var light = _controller.Selected;
            if (_suppress || light == null)
                return;

            if (field == "x") light.X = (float)_x.Value;
            if (field == "y") light.Y = (float)_y.Value;
            if (field == "radius") light.Radius = (float)_radius.Value;
            if (field == "influence") light.Influence = (float)_influence.Value;
            if (field == "color") light.ColorHex = "#" + _color.Color.ToHtml(light.ColorHex?.Length == 9);
            _target?.Commit();
            SetDirty();
            RefreshList();
            _lineEdit.Text = CurrentSourceText();
            RefreshCanvas();
        }

        private void OnCanvasChanged()
        {
            _target?.Commit();
            if (_target != null) { RefreshAll(); SyncTargetDirty(); return; }
            SyncTargetDirty();
            _suppress = true;
            WriteProperties();
            _suppress = false;
            RefreshList();
            _lineEdit.Text = CurrentSourceText();
        }

        // ── refresh ────────────────────────────────────────────────

        private void RefreshAll()
        {
            RefreshList();
            _suppress = true;
            WriteProperties();
            _suppress = false;
            _lineEdit.Text = CurrentSourceText();
            _addBtn.Disabled = _controller.Lights.Count >= _controller.Limit;
            _deleteBtn.Disabled = _controller.Selected == null;
            RefreshCanvas();
        }

        private void RefreshList()
        {
            _lightList.Clear();
            for (var i = 0; i < _controller.Lights.Count; i++)
            {
                var light = _controller.Lights[i];
                var index = _lightList.AddItem($"●  {i + 1}.  {light.X:0.000}, {light.Y:0.000}   r {light.Radius:0.##}");
                _lightList.SetItemCustomFgColor(index, light.ToColor());
            }

            if (_controller.SelectedIndex >= 0)
                _lightList.Select(_controller.SelectedIndex);
        }

        private void WriteProperties()
        {
            var light = _controller.Selected;
            _propsEmpty.Visible = light == null;
            _propsBody.Visible = light != null;
            if (light == null)
                return;

            _x.Value = light.X;
            _y.Value = light.Y;
            _radius.Value = light.Radius;
            _influence.Value = light.Influence;
            _color.Color = light.ToColor();
        }
    }
}
