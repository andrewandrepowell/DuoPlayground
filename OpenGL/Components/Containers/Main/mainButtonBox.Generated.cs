//Code for Containers/Main/mainButtonBox (Container)
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
partial class mainButtonBox : MonoGameGum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new MonoGameGum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Containers/Main/mainButtonBox");
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new mainButtonBox(visual);
            return visual;
        });
        MonoGameGum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(mainButtonBox)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Containers/Main/mainButtonBox", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public mainButton button { get; protected set; }

    public string buttonMessage
    {
        get => button.Message;
        set => button.Message = value;
    }

    public float buttonRotation
    {
        get => button.Visual.Rotation;
        set => button.Visual.Rotation = value;
    }

    public int buttontextFontSize
    {
        get => button.textFontSize;
        set => button.textFontSize = value;
    }

    public mainButtonBox(InteractiveGue visual) : base(visual) { }
    public mainButtonBox()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        button = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<mainButton>(this.Visual,"button");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
