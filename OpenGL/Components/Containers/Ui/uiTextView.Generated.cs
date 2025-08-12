//Code for Containers/Ui/uiTextView (Container)
using GumRuntime;
using MonoGameGum.GueDeriving;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;

using RenderingLibrary.Graphics;

using System.Linq;

namespace DuoGum.Components;
partial class uiTextView : MonoGameGum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new MonoGameGum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Containers/Ui/uiTextView");
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new uiTextView(visual);
            return visual;
        });
        MonoGameGum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(uiTextView)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Containers/Ui/uiTextView", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public TextRuntime text { get; protected set; }

    public string Text
    {
        get => text.Text;
        set => text.Text = value;
    }

    public uiTextView(InteractiveGue visual) : base(visual) { }
    public uiTextView()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        text = this.Visual?.GetGraphicalUiElementByName("text") as TextRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
