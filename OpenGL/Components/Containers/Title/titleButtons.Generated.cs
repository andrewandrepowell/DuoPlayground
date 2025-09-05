//Code for Containers/Title/titleButtons (Container)
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
partial class titleButtons : MonoGameGum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new MonoGameGum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Containers/Title/titleButtons");
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new titleButtons(visual);
            return visual;
        });
        MonoGameGum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(titleButtons)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Containers/Title/titleButtons", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public titleButton start { get; protected set; }
    public titleButton options { get; protected set; }
    public titleButton exit { get; protected set; }

    public titleButtons(InteractiveGue visual) : base(visual) { }
    public titleButtons()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        start = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<titleButton>(this.Visual,"start");
        options = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<titleButton>(this.Visual,"options");
        exit = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<titleButton>(this.Visual,"exit");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
