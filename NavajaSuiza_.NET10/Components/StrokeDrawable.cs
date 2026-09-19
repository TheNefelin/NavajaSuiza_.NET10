using Microsoft.Maui.Graphics;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Components;

public partial class StrokeDrawable : IDrawable
{
    private readonly PizarraViewModel _viewModel;

    public StrokeDrawable(PizarraViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Color.FromArgb(_viewModel.BoardColor);
        canvas.FillRectangle(dirtyRect);

        foreach (var stroke in _viewModel.Strokes)
        {
            if (stroke.Points.Count < 2)
                continue;

            var path = new PathF();
            path.MoveTo(stroke.Points[0].X, stroke.Points[0].Y);
            for (var i = 1; i < stroke.Points.Count; i++)
                path.LineTo(stroke.Points[i].X, stroke.Points[i].Y);

            canvas.StrokeColor = Color.FromArgb(stroke.ColorHex);
            canvas.StrokeSize = stroke.Width;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeLineJoin = LineJoin.Round;
            canvas.DrawPath(path);
        }
    }
}