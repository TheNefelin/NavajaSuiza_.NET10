using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Selectors;

public class NotesItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? NoteTemplate { get; set; }

    public DataTemplate? TaskTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        return item is TaskGroupListItem ? TaskTemplate! : NoteTemplate!;
    }
}
