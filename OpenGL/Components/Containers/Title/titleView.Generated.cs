//Code for Containers/Title/titleView (Container)
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
partial class titleView : MonoGameGum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new MonoGameGum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Containers/Title/titleView");
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new titleView(visual);
            return visual;
        });
        MonoGameGum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(titleView)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Containers/Title/titleView", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public titleBar upperBar { get; protected set; }
    public titleTitle titleArt { get; protected set; }
    public titleBar lowerBar { get; protected set; }
    public titleButtons buttons { get; protected set; }

    public titleView(InteractiveGue visual) : base(visual) { }
    public titleView()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        upperBar = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<titleBar>(this.Visual,"upperBar");
        titleArt = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<titleTitle>(this.Visual,"titleArt");
        lowerBar = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<titleBar>(this.Visual,"lowerBar");
        buttons = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<titleButtons>(this.Visual,"buttons");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
