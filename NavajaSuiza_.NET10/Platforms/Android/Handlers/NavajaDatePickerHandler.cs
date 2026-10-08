using Android.App;
using Android.Content;
using AndroidColor = Android.Graphics.Color;
using Microsoft.Maui.Handlers;

namespace NavajaSuiza_.NET10.Platforms.Android.Handlers;

public class NavajaDatePickerHandler : DatePickerHandler
{
    private const string DialogButtonColor = "#FF990A";

    protected override DatePickerDialog CreateDatePickerDialog(int year, int month, int day)
    {
        var dialog = base.CreateDatePickerDialog(year, month, day);
        dialog.ShowEvent += (_, _) => PaintDialogButtons(dialog);
        return dialog;
    }

    private static void PaintDialogButtons(AlertDialog dialog)
    {
        var color = AndroidColor.ParseColor(DialogButtonColor);
        dialog.GetButton((int)DialogButtonType.Positive)?.SetTextColor(color);
        dialog.GetButton((int)DialogButtonType.Negative)?.SetTextColor(color);
    }
}
