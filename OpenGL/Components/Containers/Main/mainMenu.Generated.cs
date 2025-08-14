//Code for Containers/Main/mainMenu (Container)
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
partial class mainMenu : MonoGameGum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new MonoGameGum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Containers/Main/mainMenu");
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new mainMenu(visual);
            return visual;
        });
        MonoGameGum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(mainMenu)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Containers/Main/mainMenu", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public NineSliceRuntime background { get; protected set; }
    public mainButton resume { get; protected set; }
    public mainButton help { get; protected set; }
    public mainButton options { get; protected set; }
    public mainButton title { get; protected set; }
    public mainButton exit { get; protected set; }
    public ContainerRuntime buttons { get; protected set; }

    public mainMenu(InteractiveGue visual) : base(visual) { }
    public mainMenu()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        background = this.Visual?.GetGraphicalUiElementByName("background") as NineSliceRuntime;
        resume = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<mainButton>(this.Visual,"resume");
        help = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<mainButton>(this.Visual,"help");
        options = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<mainButton>(this.Visual,"options");
        title = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<mainButton>(this.Visual,"title");
        exit = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<mainButton>(this.Visual,"exit");
        buttons = this.Visual?.GetGraphicalUiElementByName("buttons") as ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
