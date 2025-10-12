//Code for Containers/Options/optionsCheck (Container)
using GumRuntime;
using MonoGameGum.GueDeriving;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;

using RenderingLibrary.Graphics;

using System.Linq;

namespace DuoGum.Components;
partial class optionsCheck : MonoGameGum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new MonoGameGum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Containers/Options/optionsCheck");
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new optionsCheck(visual);
            return visual;
        });
        MonoGameGum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(optionsCheck)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Containers/Options/optionsCheck", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public ContainerRuntime checkbox { get; protected set; }
    public SpriteRuntime box { get; protected set; }
    public SpriteRuntime checkmark { get; protected set; }
    public TextRuntime text { get; protected set; }

    public bool checkmarkVisible
    {
        get => checkmark.Visible;
        set => checkmark.Visible = value;
    }

    public string textText
    {
        get => text.Text;
        set => text.Text = value;
    }

    public optionsCheck(InteractiveGue visual) : base(visual) { }
    public optionsCheck()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        checkbox = this.Visual?.GetGraphicalUiElementByName("checkbox") as ContainerRuntime;
        box = this.Visual?.GetGraphicalUiElementByName("box") as SpriteRuntime;
        checkmark = this.Visual?.GetGraphicalUiElementByName("checkmark") as SpriteRuntime;
        text = this.Visual?.GetGraphicalUiElementByName("text") as TextRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
