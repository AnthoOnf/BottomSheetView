using System;
namespace BottomSheetView.Sources
{
    public class SheetSize
    {
        public static SheetSize FullScreen = new SheetSize(0);
        public static SheetSize HalfScreen = new SheetSize(-1);

        protected SheetSize(float v)
        {
            this.Value = v;
        }

        public static SheetSize Fixed(float v)
        {
            return new SheetSize(v);
        }

        public float Value { get; private set; }
    }
}
