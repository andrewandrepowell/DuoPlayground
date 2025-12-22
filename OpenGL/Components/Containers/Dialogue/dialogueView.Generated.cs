//Code for Containers/Dialogue/dialogueView (Container)
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
partial class dialogueView : MonoGameGum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new MonoGameGum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Containers/Dialogue/dialogueView");
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new dialogueView(visual);
            return visual;
        });
        MonoGameGum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(dialogueView)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Containers/Dialogue/dialogueView", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public ContainerRuntime leftSpacer { get; protected set; }
    public dialogueBox dialogueBoxInstance { get; protected set; }

    public dialogueView(InteractiveGue visual) : base(visual) { }
    public dialogueView()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        leftSpacer = this.Visual?.GetGraphicalUiElementByName("leftSpacer") as ContainerRuntime;
        dialogueBoxInstance = MonoGameGum.Forms.GraphicalUiElementFormsExtensions.GetFrameworkElementByName<dialogueBox>(this.Visual,"dialogueBoxInstance");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
