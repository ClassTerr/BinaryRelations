using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WindowsFormsAero;
using WindowsFormsAero.Dwm.Helpers;

namespace Binary_relations
{
    public partial class Form1 : Form
    {
        List<Pair> CurrentBR = new List<Pair>();
        Dictionary<string, bool> vert = new Dictionary<string, bool>();


        private double ArrowAngle = Math.PI / 12;
        private double ArrowSize = 15;

        public static float StringWidth(string text, Font f, Graphics g)
        {
            return g.MeasureString(text, f).Width;
        }

        void DrawArrow(Graphics g, Point t, double alpha)
        {
            PointF[] pts = new PointF[3];

            pts[0].X = (float)(t.X + Math.Cos(alpha + ArrowAngle) * ArrowSize);
            pts[0].Y = (float)(t.Y + Math.Sin(alpha + ArrowAngle) * ArrowSize);
            pts[1].X = (float)(t.X + Math.Cos(alpha - ArrowAngle) * ArrowSize);
            pts[1].Y = (float)(t.Y + Math.Sin(alpha - ArrowAngle) * ArrowSize);
            pts[2].X = t.X;
            pts[2].Y = t.Y;

            g.FillPolygon(Brushes.Green, pts);
            g.DrawPolygon(Pens.Black, pts);
        }

        void DrawArrow(Graphics g, Point f, Point t)
        {
            g.DrawLine(new Pen(Color.Black, 2.5f), f, t);
            g.DrawLine(new Pen(Color.Red, 2), f, t);

            PointF[] pts = new PointF[3];
            double alpha;
            if (t.X == f.X)
                alpha = Math.Atan((t.X - f.X + 0.0) / (t.Y - f.Y)) - Math.PI / 2;
            else alpha = Math.Atan((t.Y - f.Y + 0.0) / (t.X - f.X));
            if (t.X > f.X || (t.Y < f.Y && t.X == f.X))
                alpha += Math.PI;

            pts[0].X = (float)(t.X + Math.Cos(alpha + ArrowAngle) * ArrowSize);
            pts[0].Y = (float)(t.Y + Math.Sin(alpha + ArrowAngle) * ArrowSize);
            pts[1].X = (float)(t.X + Math.Cos(alpha - ArrowAngle) * ArrowSize);
            pts[1].Y = (float)(t.Y + Math.Sin(alpha - ArrowAngle) * ArrowSize);
            pts[2].X = t.X;
            pts[2].Y = t.Y;

            g.DrawPolygon(new Pen(Color.Black, 1.7f), pts);
            g.FillPolygon(Brushes.Red, pts);
        }
        void Draw()
        {
            Draw(painterControl1.Graphics);

            painterControl1.Graphics.Flush();
            painterControl1.Image = painterControl1.Bitmap;
            //painterControl1.RazorPaint();
        }

        void Draw(Graphics g)
        {
            HashSet<string> vertSet = new HashSet<string>();
            foreach (var item in CurrentBR)
            {
                vertSet.Add(item.X);
                vertSet.Add(item.Y);
            }

            List<string> vars = vertSet.ToList<string>();
            vars.Sort();

            g.Clear(Color.White);
            Point center = new Point(painterControl1.Width / 2, painterControl1.Height / 2);
            int radius = Math.Min(center.X, center.Y) - 40;
            int dotradius = 3;

            float dalpha = -2 * (float)Math.PI / vars.Count;

            g.TranslateTransform(painterControl1.Width / 2, painterControl1.Height / 2);
            g.RotateTransform(trackBar1.Value);

            if (vars.Count == 0)
            {
                g.FillEllipse(Brushes.Black, 0 - dotradius - 0.5f, dotradius - 0.5f, dotradius * 2 + 1, dotradius * 2 + 1);
                g.FillEllipse(Brushes.Red, 0 - dotradius, dotradius, dotradius * 2, dotradius * 2);
            }

            if (vars.Count == 1)
                radius = 0;

            foreach (var item in CurrentBR)
            {
                int a = vars.IndexOf(item.X), b = vars.IndexOf(item.Y);
                if (a != b)
                {
                    Point p1 = new Point((int)(radius * Math.Sin(dalpha * a)), (int)(radius * Math.Cos(dalpha * a)));
                    Point p2 = new Point((int)(radius * Math.Sin(dalpha * b)), (int)(radius * Math.Cos(dalpha * b)));

                    DrawArrow(g, p1, p2);
                }
                else
                {
                    float crad = 15;
                    Point p1 = new Point(
                        (int)((radius + crad) * Math.Sin(dalpha * a)),
                        (int)((radius + crad) * Math.Cos(dalpha * a)));
                    g.DrawEllipse(Pens.Green, p1.X - crad, p1.Y - crad, crad * 2, crad * 2);
                    p1 = new Point(
                        (int)(radius * Math.Sin(dalpha * a)),
                        (int)(radius * Math.Cos(dalpha * a)));
                    DrawArrow(g, p1, -dalpha * a + Math.PI / 8);
                }
            }

            dalpha = (float)(-dalpha / Math.PI * 180);

            foreach (var item in vars)
            {
                g.FillEllipse(Brushes.Black, 0 - dotradius - 0.5f, radius - dotradius - 0.5f, dotradius * 2 + 1, dotradius * 2 + 1);
                g.FillEllipse(Brushes.Red, 0 - dotradius, radius - dotradius, dotradius * 2, dotradius * 2);
                g.DrawString(item.ToString(), SystemFonts.CaptionFont, Brushes.Black, 
                    - dotradius - StringWidth(item.ToString(), SystemFonts.CaptionFont, g) / 2, radius + 2 * dotradius);
                g.RotateTransform(dalpha);
            }

            g.ResetTransform();
            g.Flush();
        }


        public Form1()
        {
            InitializeComponent();
            button1_Click(null, null);

            painterControl1.Graphics.Flush();
            painterControl1.RazorPaint();
            Draw();
        }

        void Error(int l, int r)
        {
            errorsWasOccured = true;
            richTextBox1.SelectionStart = l;
            richTextBox1.SelectionLength = r - l;
            richTextBox1.SelectionBackColor = Color.LightPink;
        }

        string Adjust(string s)
        {
            return Regex.Replace(s, @"^R *= *\{(.*)}$", @"$1").Trim();
        }

        void ReadBR(string s)
        {
            CurrentBR.Clear();
            s += " ";
            Regex bet = new Regex("[^., \n]");
            var res = Regex.Matches(s, @"\( *\w+ *[;, ]{1} *\w+ *\)");
            int i = 0;
            var beg = Regex.Matches(s, "R? *=? *{");
            if (beg.Count > 0)
                i = beg[0].Index + beg[0].Length;

            int to = s.IndexOf('}', i);
            if (to != -1)
            {
                if (s.Substring(to + 1, s.Length - to - 1).Trim() != "")
                    Error(to + 1, s.Length);
                s = s.Substring(0, to);
            }

            foreach (Match item in res)
            {
                if (item.Index + item.Length > s.Length)
                {
                    //i = item.Index + item.Length;
                    break;
                }
                string pref = s.Substring(i, item.Index - i);
                if (bet.IsMatch(pref) && !(Regex.IsMatch(pref, "R? *=? *{") && i == 0))
                    Error(i, item.Index);

                var t = item.Value.Substring(1, item.Length - 2).Replace('.', ',').Split(";, ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (t.Length != 2)
                    Error(item.Index, item.Index + item.Length);
                else
                    CurrentBR.Add(new Pair(t[0], t[1]));
                i = item.Index + item.Length;
            }
            if (s.Length > i)
                if (s.Substring(i, s.Length - i).Trim() != "")
                    Error(i, s.Length);
        }


        bool IsReflective(bool needValue = true)
        {
            vert.Clear();
            foreach (var item in CurrentBR)
                vert[item.X] = vert[item.Y] = false;

            foreach (var item in CurrentBR)
                if (item.X == item.Y)
                    vert[item.X] = true;

            foreach (var item in vert)
                if (item.Value != needValue)
                    return false;

            return true;
        }

        bool IsAntiReflective()
        {
            return IsReflective(false);
        }

        bool IsSymmetrical()
        {
            for (int i = 0; i < CurrentBR.Count; i++)
                for (int j = 0; j < CurrentBR.Count; j++)
                {
                    if (CurrentBR[i].X == CurrentBR[j].Y && CurrentBR[i].Y == CurrentBR[j].X)
                        break;

                    if (j == CurrentBR.Count - 1)
                        return false;
                }

            return true;
        }

        bool IsAntiSymmetrical(bool needValue = true)
        {
            bool hasIa = false;
            for (int i = 0; i < CurrentBR.Count; i++)
            {
                for (int j = 0; j < CurrentBR.Count; j++)
                {
                    if (CurrentBR[i].X == CurrentBR[j].Y && CurrentBR[i].Y == CurrentBR[j].X && i != j)
                        return false;

                }
                if (CurrentBR[i].X == CurrentBR[i].Y)
                    hasIa = true;
            }

            return hasIa == needValue;
        }

        bool IsASymmetrical()
        {
            return IsAntiSymmetrical(false);
        }

        bool IsTransitive()
        {
            for (int i = 0; i < CurrentBR.Count; i++)
            {
                for (int j = 0; j < CurrentBR.Count; j++)
                {
                    if (CurrentBR[i].Y == CurrentBR[j].X)
                    {
                        Pair t = new Pair(CurrentBR[i].X, CurrentBR[j].Y);
                        bool found = false;
                        for (int k = 0; k < CurrentBR.Count; k++)
                        {
                            if (CurrentBR[k] == t)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                            return false;
                    }
                }
            }

            return true;
        }

        string GetRmulR()
        {
            HashSet<Pair> s = new HashSet<Pair>();
            for (int i = 0; i < CurrentBR.Count; i++)
            {
                for (int j = 0; j < CurrentBR.Count; j++)
                {
                    if (CurrentBR[i].Y == CurrentBR[j].X)
                        s.Add(new Pair(CurrentBR[i].X, CurrentBR[j].Y));
                }
            }

            string res = "";

            foreach (var item in s)
                res += "(" + item.X + ", " + item.Y + "), ";

            return res;
        }

        bool errorsWasOccured = false;

        private void button1_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();
            int cursorIndex = richTextBox1.SelectionStart;

            //adjusting text
            if (errorsWasOccured)
            {
                richTextBox1.SelectionStart = 0;
                richTextBox1.SelectionLength = richTextBox1.Text.Length;
                richTextBox1.SelectionBackColor = richTextBox1.BackColor;
            }

            errorsWasOccured = false;

            //reading
            ReadBR(richTextBox1.Text);

            richTextBox2.Clear();

            if (CurrentBR.Count == 0)
            {
                richTextBox2.Text = "Не найено ни одной пригодной пары БО\nR = ∅";
                foreach (ListViewItem item in listView1.Items)
                    item.ImageIndex = 0;

                richTextBox1.SelectionStart = cursorIndex;
                richTextBox1.SelectionLength = 0;

                Draw();
                ResumeLayout();
                return;
            }

            richTextBox2.Text += "R = { ";
            foreach (var item in CurrentBR)
                richTextBox2.Text += "(" + item.X + ", " + item.Y + "), ";

            if (CurrentBR.Count > 0)
                richTextBox2.Text = richTextBox2.Text.Substring(0, richTextBox2.Text.Length - 2);
            richTextBox2.Text += " }\n\n";


            // D(R)
            vert.Clear();
            foreach (var item in CurrentBR)
                vert[item.X] = false;

            richTextBox2.Text += "D(R) = { ";
            foreach (var item in vert.OrderBy(t => t.Key))
                richTextBox2.Text += item.Key + ", ";

            if (vert.Count != 0)
                richTextBox2.Text = richTextBox2.Text.Substring(0, richTextBox2.Text.Length - 2);
            richTextBox2.Text += " }\n";

            // D(R)
            vert.Clear();
            foreach (var item in CurrentBR)
                vert[item.Y] = false;

            richTextBox2.Text += "E(R) = { ";
            foreach (var item in vert.OrderBy(t => t.Key))
                richTextBox2.Text += item.Key + ", ";

            if (vert.Count != 0)
                richTextBox2.Text = richTextBox2.Text.Substring(0, richTextBox2.Text.Length - 2);
            richTextBox2.Text += " }\n";

            // O(R)
            vert.Clear();
            foreach (var item in CurrentBR)
                vert[item.X] = vert[item.Y] = false;

            richTextBox2.Text += "O(R) = { ";
            foreach (var item in vert.OrderBy(t => t.Key))
                richTextBox2.Text += item.Key + ", ";

            if (vert.Count != 0)
                richTextBox2.Text = richTextBox2.Text.Substring(0, richTextBox2.Text.Length - 2);
            richTextBox2.Text += " }\n\n";

            //begin analysis
            listView1.Items[0].ImageIndex = Convert.ToInt32(IsReflective());


            richTextBox2.Text += "Тождественное БО: \t{ ";
            foreach (var item in vert.OrderBy(t => t.Key))
                richTextBox2.Text += "(" + item.Key + ", " + item.Key + "), ";

            richTextBox2.Text += "}\nОбратное БО: \t\t{ ";
            foreach (var item in CurrentBR)
                richTextBox2.Text += "(" + item.Y + ", " + item.X + "), ";

            richTextBox2.Text += "}\nКомпозиция R◦R = \t{ ";
            richTextBox2.Text += GetRmulR();
            richTextBox2.Text += "}";

            listView1.Items[1].ImageIndex = Convert.ToInt32(IsAntiReflective());
            listView1.Items[2].ImageIndex = Convert.ToInt32(IsSymmetrical());
            listView1.Items[3].ImageIndex = Convert.ToInt32(IsAntiSymmetrical());
            listView1.Items[4].ImageIndex = Convert.ToInt32(IsASymmetrical());
            listView1.Items[5].ImageIndex = Convert.ToInt32(IsTransitive());
            richTextBox2.Text = Regex.Replace(richTextBox2.Text, "{ *}", "∅").Replace("), }", ") }");
            richTextBox1.SelectionStart = cursorIndex;
            richTextBox1.SelectionLength = 0;

            Draw();

            this.ResumeLayout();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                button1_Click(sender, e);
        }

        private void painterControl1_Resize(object sender, EventArgs e)
        {
            Draw();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Draw();
        }
    }
}