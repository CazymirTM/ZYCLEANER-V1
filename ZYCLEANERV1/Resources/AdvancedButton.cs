using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace NMSModToggler.Resources // <--by CeZeY
{
    public class AdvancedButton : Button
    {
        private bool isHovered = false;
        private bool isPressed = false;

        // ====== Designer Properties ======
        [Category("Custom Style")]
        public Color ButtonColor1 { get; set; } = Color.MediumSlateBlue;

        [Category("Custom Style")]
        public Color ButtonColor2 { get; set; } = Color.MediumPurple;

        [Category("Custom Style")]
        public Color HoverColor1 { get; set; } = Color.SlateBlue;

        [Category("Custom Style")]
        public Color HoverColor2 { get; set; } = Color.DarkSlateBlue;

        [Category("Custom Style")]
        public Color PressedColor1 { get; set; } = Color.Indigo;

        [Category("Custom Style")]
        public Color PressedColor2 { get; set; } = Color.MidnightBlue;

        [Category("Custom Style")]
        public Color TextColor { get; set; } = Color.White;

        [Category("Custom Style")]
        public int BorderRadius { get; set; } = 20;

        [Category("Custom Style")]
        public int BorderSize { get; set; } = 2;

        [Category("Custom Style")]
        public Color BorderColor { get; set; } = Color.White;

        [Category("Custom Style")]
        public System.Drawing.Image Icon { get; set; } = null;

        [Category("Custom Style")]
        public int IconSize { get; set; } = 20;

        [Category("Custom Style")]
        public int IconSpacing { get; set; } = 5;

        [Category("Custom Style")]
        public LinearGradientMode GradientDirection { get; set; } = LinearGradientMode.Vertical;

        // ====== Constructor ======
        public AdvancedButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            DoubleBuffered = true;
            ForeColor = TextColor;
        }

        // ====== Painting ======
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = ClientRectangle;

            // Pick colors depending on state
            Color c1 = ButtonColor1, c2 = ButtonColor2;
            if (isPressed) { c1 = PressedColor1; c2 = PressedColor2; }
            else if (isHovered) { c1 = HoverColor1; c2 = HoverColor2; }

            using (var path = GetRoundedPath(rect, BorderRadius))
            using (var brush = new LinearGradientBrush(rect, c1, c2, GradientDirection))
            using (var pen = new Pen(BorderColor, BorderSize))
            {
                // Background
                Region = new Region(path);
                e.Graphics.FillPath(brush, path);

                // Border
                if (BorderSize > 0)
                    e.Graphics.DrawPath(pen, path);

                // Text + Icon
                DrawContent(e.Graphics, rect);
            }
        }

        private void DrawContent(Graphics g, Rectangle rect)
        {
            SizeF textSize = g.MeasureString(Text, Font);
            int totalWidth = (int)textSize.Width;

            if (Icon != null)
                totalWidth += IconSize + IconSpacing;

            int startX = (rect.Width - totalWidth) / 2;
            int centerY = rect.Height / 2;

            // Draw Icon
            if (Icon != null)
            {
                Rectangle iconRect = new Rectangle(startX, centerY - IconSize / 2, IconSize, IconSize);
                g.DrawImage(Icon, iconRect);
                startX += IconSize + IconSpacing;
            }

            // Draw Text
            using (Brush textBrush = new SolidBrush(TextColor))
            {
                g.DrawString(Text, Font, textBrush,
                    new PointF(startX, centerY - textSize.Height / 2));
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        // ====== Mouse Events ======
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovered = false;
            isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isPressed = false;
            Invalidate();
        }
    }
}
