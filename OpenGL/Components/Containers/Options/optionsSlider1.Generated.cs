//Code for Containers/Options/optionsSlider1 (Container)
using GumRuntime;
using MonoGameGum.GueDeriving;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;

using RenderingLibrary.Graphics;

using System.Linq;

namespace DuoGum.Components;
partial class optionsSlider1 : MonoGameGum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new MonoGameGum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Containers/Options/optionsSlider1");
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new optionsSlider1(visual);
            return visual;
        });
        MonoGameGum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(optionsSlider1)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Containers/Options/optionsSlider1", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public SpriteRuntime background_a { get; protected set; }
    public TextRuntime text_b { get; protected set; }
    public ContainerRuntime container_a { get; protected set; }
    public ContainerRuntime container_b { get; protected set; }
    public SpriteRuntime background_c { get; protected set; }
    public ContainerRuntime container_c { get; protected set; }
    public SpriteRuntime pointer { get; protected set; }
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

    public float pointerX
    {
        get => pointer.X;
        set => pointer.X = value;
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

    public optionsSlider1(InteractiveGue visual) : base(visual) { }
    public optionsSlider1()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        background_a = this.Visual?.GetGraphicalUiElementByName("background_a") as SpriteRuntime;
        text_b = this.Visual?.GetGraphicalUiElementByName("text_b") as TextRuntime;
        container_a = this.Visual?.GetGraphicalUiElementByName("container_a") as ContainerRuntime;
        container_b = this.Visual?.GetGraphicalUiElementByName("container_b") as ContainerRuntime;
        background_c = this.Visual?.GetGraphicalUiElementByName("background_c") as SpriteRuntime;
        container_c = this.Visual?.GetGraphicalUiElementByName("container_c") as ContainerRuntime;
        pointer = this.Visual?.GetGraphicalUiElementByName("pointer") as SpriteRuntime;
        container = this.Visual?.GetGraphicalUiElementByName("container") as ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
