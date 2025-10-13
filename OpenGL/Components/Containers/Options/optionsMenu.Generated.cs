//Code for Containers/Options/optionsMenu (Container)
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
partial class optionsMenu : MonoGameGum.Forms.Controls.Menu
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new MonoGameGum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Containers/Options/optionsMenu");
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new optionsMenu(visual);
            return visual;
        });
        MonoGameGum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(optionsMenu)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Containers/Options/optionsMenu", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public optionsButton back { get; protected set; }
    public optionsSlider music { get; protected set; }
    public optionsSlider sfx { get; protected set; }
    public optionsRadio fw { get; protected set; }

    public optionsMenu(InteractiveGue visual) : base(visual) { }
    public optionsMenu()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        back = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<optionsButton>(this.Visual,"back");
        music = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<optionsSlider>(this.Visual,"music");
        sfx = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<optionsSlider>(this.Visual,"sfx");
        fw = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<optionsRadio>(this.Visual,"fw");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
