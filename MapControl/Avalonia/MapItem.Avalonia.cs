using Avalonia;
using Avalonia.Input;
using System;

namespace MapControl;

public partial class MapItem
{
    Point? pointerPressedPosition;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var point = e.GetCurrentPoint(null);

        if (point.Properties.IsLeftButtonPressed)
        {
            pointerPressedPosition = point.Position;
        }

        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (pointerPressedPosition.HasValue)
        {
            const double pointerMovementThreshold = 2d;
            var position = e.GetCurrentPoint(null).Position;

            // Perform selection only when no significant pointer movement occured.
            //
            if (Math.Abs(position.X - pointerPressedPosition.Value.X) <= pointerMovementThreshold &&
                Math.Abs(position.Y - pointerPressedPosition.Value.Y) <= pointerMovementThreshold)
            {
                base.OnPointerReleased(e);
            }

            pointerPressedPosition = null;
        }

        e.Handled = true;
    }
}
