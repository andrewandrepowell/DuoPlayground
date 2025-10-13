//Code for Containers/Options/optionsButton (Container)
using GumRuntime;
using MonoGameGum.GueDeriving;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;

using RenderingLibrary.Graphics;

using System.Linq;

namespace DuoGum.Components;
partial class optionsButton : MonoGameGum.Forms.Controls.Button
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new MonoGameGum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Containers/Options/optionsButton");
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new optionsButton(visual);
            return visual;
        });
        MonoGameGum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(optionsButton)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Containers/Options/optionsButton", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public enum ButtonCategory
    {
        Enabled,
        Disabled,
        Highlighted,
        Pushed,
        HighlightedFocused,
        Focused,
        DisabledFocused,
    }

    ButtonCategory? _buttonCategoryState;
    public ButtonCategory? ButtonCategoryState
    {
        get => _buttonCategoryState;
        set
        {
            _buttonCategoryState = value;
            if(value != null)
            {
                if(Visual.Categories.ContainsKey("ButtonCategory"))
                {
                    var category = Visual.Categories["ButtonCategory"];
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
                else
                {
                    var category = ((Gum.DataTypes.ElementSave)this.Visual.Tag).Categories.FirstOrDefault(item => item.Name == "ButtonCategory");
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
            }
        }
    }
    public SpriteRuntime background_a { get; protected set; }
    public TextRuntime text_b { get; protected set; }
    public ContainerRuntime container_a { get; protected set; }
    public ContainerRuntime container { get; protected set; }

    public int background_aTextureTop
    {
        get => background_a.TextureTop;
        set => background_a.TextureTop = value;
    }

    public float containerRotation
    {
        get => container.Rotation;
        set => container.Rotation = value;
    }

    public int text_bFontSize
    {
        get => text_b.FontSize;
        set => text_b.FontSize = value;
    }

    public string text_bText
    {
        get => text_b.Text;
        set => text_b.Text = value;
    }

    public optionsButton(InteractiveGue visual) : base(visual) { }
    public optionsButton()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        background_a = this.Visual?.GetGraphicalUiElementByName("background_a") as SpriteRuntime;
        text_b = this.Visual?.GetGraphicalUiElementByName("text_b") as TextRuntime;
        container_a = this.Visual?.GetGraphicalUiElementByName("container_a") as ContainerRuntime;
        container = this.Visual?.GetGraphicalUiElementByName("container") as ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
