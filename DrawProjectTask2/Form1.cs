using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DrawProjectTask2
{
    public partial class Form1 : Form
    {

        Bitmap bitmap;
        private bool canRecognizeMouseClick = false;

        public Form1()
        {
            InitializeComponent();
            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bitmap;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.canRecognizeMouseClick = false;
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.ShowDialog();
            string FileName = openFile.FileName;
            bitmap = new Bitmap(FileName);
            pictureBox1.Image = bitmap;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.canRecognizeMouseClick = true;
        }

        //Алгоритм: в параметрах передается точка после нажатия мышкой, сама же функция возвращает точку того же цвета как можно выше и правее
        private CustomPoint countStart(Bitmap image, Point pressedPoint)
        {
            int x1 = pressedPoint.X;
            int y1 = pressedPoint.Y;
            int lastX = -1;
            int lastY = -1;
            Color backgroundColor = image.GetPixel(pressedPoint.X, pressedPoint.Y);
            Color currentPColor = backgroundColor;
            while (currentPColor.ToArgb() == backgroundColor.ToArgb() && y1 > 0)
            {
                while (currentPColor.ToArgb() == backgroundColor.ToArgb() && x1 < image.Width - 2)
                {
                    x1++;
                    currentPColor = image.GetPixel(x1, y1);
                }
                // Приоритет отдается точке, находящейся как можно правее
                if (lastX < x1)
                {
                    lastY = y1;
                    lastX = x1;
                }
                y1--;
                x1 = pressedPoint.X;
                currentPColor = image.GetPixel(x1, y1);
            }
            Console.WriteLine("start point return = " + lastX + " " + lastY);
            return new CustomPoint(lastX, lastY, 4);
        }



        // Обработка нажатия на картинку
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.canRecognizeMouseClick) {
                Point location = e.Location;
                Console.WriteLine("Pressed = " + location.X + " " + location.Y);
                List<CustomPoint> borders = this.makeBorderPointList(bitmap, location);
                pictureBox1.Refresh();
            }
        }

        //Алгоритм такой: двигшаемся против часовой стралки, находим рядом хотя бы 1 из 8 пикселей такого же цвета, после чего переходим на 90 градусов на другой пиксель,
        //чтобы обработать случай когда граница в ширину > 1 пикселя. Таким образом в конечном итоге мы выделим всю границу. 1 пиксель - это тоже граница 
        private List<CustomPoint> makeBorderPointList(Bitmap image, Point location)
        {
            CustomPoint startPoint = this.countStart(image, location);
            CustomPoint currentPoint = startPoint;
            List<CustomPoint> borders = new List<CustomPoint>();
            borders.Add(startPoint);
            CustomPoint nextPoint = new CustomPoint(0, 0, 0);
            int findIndex;
            Color headColor = image.GetPixel(currentPoint.ownPoint.X, currentPoint.ownPoint.Y);

            do
            {
                // 90 градусов 
                // 3  2  1
                // 4  X  0
                // 5  6  7
                findIndex = currentPoint.fromLastPixelIndex - 2;
                if (findIndex == -1) findIndex = 7;
                if (findIndex == -2) findIndex = 6;
                // Будем по всем 8-и связным пикселям проходить и искать первый такого же цвета, камнем преткновения станет changesIndex, вернувшись к которой закончим цикл в крайнем случае
                int changesIndex = findIndex;

                do
                {
                    nextPoint = currentPoint;
                    nextPoint.fromLastPixelIndex = changesIndex;
                    //Переходим от X к каждому индексу
                    switch (findIndex)
                    {
                        case 0:
                            nextPoint.ownPoint.X++;
                            break;
                        case 1:
                            nextPoint.ownPoint.X++;
                            nextPoint.ownPoint.Y--;
                            break;
                        case 2:
                            nextPoint.ownPoint.Y--;
                            break;
                        case 3:
                            nextPoint.ownPoint.X--;
                            nextPoint.ownPoint.Y--;
                            break;
                        case 4:
                            nextPoint.ownPoint.X--;
                            break;
                        case 5:
                            nextPoint.ownPoint.X--;
                            nextPoint.ownPoint.Y++;
                            break;
                        case 6:
                            nextPoint.ownPoint.Y++;
                            break;
                        case 7:
                            nextPoint.ownPoint.X++;
                            nextPoint.ownPoint.Y++;
                            break;
                    }

                    if (nextPoint.Equals(startPoint)) break;
                    if (image.GetPixel(nextPoint.ownPoint.X, nextPoint.ownPoint.Y) == headColor)
                    {
                        borders.Add(nextPoint);
                        //теперь начнем цикл заново, но текущая точка будет другая
                        currentPoint = nextPoint;
                        break;
                    }
                    findIndex = (findIndex + 1) % 8;
                } while (findIndex != changesIndex);

            } while (nextPoint.ownPoint != startPoint.ownPoint);

            foreach (var x in borders)
                image.SetPixel(x.ownPoint.X, x.ownPoint.Y, Color.Red);

            return borders;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }

    public struct CustomPoint
    {
        public Point ownPoint;
        public int fromLastPixelIndex;

        public CustomPoint(int x, int y, int lastPI)
        {
            ownPoint = new Point(x, y);
            fromLastPixelIndex = lastPI;
        }
    }
}
