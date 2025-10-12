//Code for Containers/Options/optionsRadio (Container)
using GumRuntime;
using MonoGameGum.GueDeriving;
using DuoGum.Components;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;

using RenderingLibrary.Graphics;

using System.Linq;

namespace DuoGum.Components;
partial class optionsRadio : MonoGameGum.Forms.Controls.Button
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new MonoGameGum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Containers/Options/optionsRadio");
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new optionsRadio(visual);
            return visual;
        });
        MonoGameGum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(optionsRadio)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Containers/Options/optionsRadio", () => 
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
    public ContainerRuntime container_b { get; protected set; }
    public optionsCheck box_c_0 { get; protected set; }
    public optionsCheck box_c_1 { get; protected set; }

    public int background_aTextureTop
    {
        get => background_a.TextureTop;
        set => background_a.TextureTop = value;
    }

    public string box_c_0textText
    {
        get => box_c_0.textText;
        set => box_c_0.textText = value;
    }

    public string box_c_1textText
    {
        get => box_c_1.textText;
        set => box_c_1.textText = value;
    }

    public float containerRotation
    {
        get => container.Rotation;
        set => container.Rotation = value;
    }

    public float text_bFontScale
    {
        get => text_b.FontScale;
        set => text_b.FontScale = value;
    }

    public string text_bText
    {
        get => text_b.Text;
        set => text_b.Text = value;
    }

    public optionsRadio(InteractiveGue visual) : base(visual) { }
    public optionsRadio()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        background_a = this.Visual?.GetGraphicalUiElementByName("background_a") as SpriteRuntime;
        text_b = this.Visual?.GetGraphicalUiElementByName("text_b") as TextRuntime;
        container_a = this.Visual?.GetGraphicalUiElementByName("container_a") as ContainerRuntime;
        container = this.Visual?.GetGraphicalUiElementByName("container") as ContainerRuntime;
        container_b = this.Visual?.GetGraphicalUiElementByName("container_b") as ContainerRuntime;
        box_c_0 = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<optionsCheck>(this.Visual,"box_c_0");
        box_c_1 = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<optionsCheck>(this.Visual,"box_c_1");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
