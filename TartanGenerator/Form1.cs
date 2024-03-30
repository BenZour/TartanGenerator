using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TartanGenerator
{
    public partial class Form1 : Form
    {
        Dictionary<string, SolidBrush> solidColours;
        Dictionary<string, HatchBrush> patternColours;

        public Form1()
        {
            InitializeComponent();
            solidColours = new Dictionary<string, SolidBrush>();
            patternColours = new Dictionary<string, HatchBrush>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Invalidate();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                parseColourPalette();
                (float widthFactor, float heightFactor) = computeSizeFactor();

                string threadcount = "";
                string pattern = @"(?<colour_code>\p{Lu}+)(?<threads>\d+)";
                Regex regex = new Regex(pattern);


                // Reflecting setts
                if (radioButton1.Checked)
                {
                    Match match = regex.Match(textBox1.Text);

                    Stack<string> threadStack = new Stack<string>();
                    while (match.Success)
                    {
                        string threadString = match.Groups["colour_code"].Value + match.Groups["threads"].Value;
                        threadcount += threadString;
                        threadStack.Push(threadString);
                        match = match.NextMatch();
                    }
                    while (threadStack.Count > 0)
                    {
                        threadcount += threadStack.Pop();
                    }
                }
                // Repeating setts
                else
                {
                    threadcount = String.Concat(textBox1.Text, textBox1.Text);
                }


                Match matchedThread = regex.Match(threadcount);

                float carriageLocation = 0;
                while (matchedThread.Success)
                {

                    float threads = Int32.Parse(matchedThread.Groups["threads"].Value);
                    threads *= widthFactor;
                    string colour_code = matchedThread.Groups["colour_code"].Value;
                    e.Graphics.FillRectangle(
                        solidColours[colour_code],
                        carriageLocation,
                        0,
                        threads,
                        panel1.Height
                        );
                    carriageLocation += threads;
                    matchedThread = matchedThread.NextMatch();
                }

                matchedThread = regex.Matches(threadcount)[0];

                carriageLocation = 0;
                while (matchedThread.Success)
                {
                    float threads = Int32.Parse(matchedThread.Groups["threads"].Value);
                    threads *= heightFactor;
                    string colour_code = matchedThread.Groups["colour_code"].Value;
                    e.Graphics.FillRectangle(
                        patternColours[colour_code],
                        0,
                        carriageLocation,
                        panel1.Width,
                        threads
                        );
                    carriageLocation += threads;
                    matchedThread = matchedThread.NextMatch();
                }
            }
        }

        private (float,float) computeSizeFactor()
        {
            string pattern = @"\d+";
            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(textBox1.Text);

            int sum = 0;
            foreach (Match m in matches)
            {
                sum += Int32.Parse(m.Value);
            }

            return (panel1.Width / (sum * 2f), panel1.Height / (sum * 2f));
        }

        private void parseColourPalette()
        {
            solidColours.Clear();
            patternColours.Clear();
            string pattern = @"(?<colour>\w+)=(?<rgb_code>\S{6})\w+";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(textBox2.Text);

            while (match.Success)
            {
                SolidBrush solidBrush = new SolidBrush(
                    ColorTranslator.FromHtml("#" + match.Groups["rgb_code"].Value)
                    );
                HatchBrush hatchBrush = new HatchBrush(
                    HatchStyle.DarkDownwardDiagonal,
                    Color.Transparent,
                    ColorTranslator.FromHtml("#" + match.Groups["rgb_code"].Value)
                    );

                solidColours.Add(match.Groups["colour"].Value, solidBrush);
                patternColours.Add(match.Groups["colour"].Value, hatchBrush);

                match = match.NextMatch();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (Bitmap bmp = new Bitmap(panel1.Width, panel1.Height))
            {
                panel1.DrawToBitmap(bmp, new Rectangle(0, 0, panel1.Width, panel1.Height));
                bmp.Save("tartan_export.png", ImageFormat.Png);
            }
        }
    }
}
