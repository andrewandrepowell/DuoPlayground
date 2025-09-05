//Code for Containers/Title/titleButton (Container)
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
partial class titleButton : MonoGameGum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new MonoGameGum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Containers/Title/titleButton");
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new titleButton(visual);
            return visual;
        });
        MonoGameGum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(titleButton)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Containers/Title/titleButton", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public titleButtonContainer buttonContainer { get; protected set; }

    public string Message
    {
        get => buttonContainer.Message;
        set => buttonContainer.Message = value;
    }

    public float ButtonRotation
    {
        get => buttonContainer.Visual.Rotation;
        set => buttonContainer.Visual.Rotation = value;
    }

    public titleButton(InteractiveGue visual) : base(visual) { }
    public titleButton()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        buttonContainer = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<titleButtonContainer>(this.Visual,"buttonContainer");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
