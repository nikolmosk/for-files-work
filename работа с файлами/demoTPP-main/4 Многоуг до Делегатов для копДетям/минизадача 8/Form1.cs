using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        List<MyShape> sh = new List<MyShape>(); //создаем ссылку на список из наших фигур
        byte whatShape; //тип вершины
                        // обходимся без флага на рисование, теперь существование хоть одной вершины должно приводить к рисованию
        bool IsFigureDragged; // флаг на перетаскивание выпуклого многоугольника
        int intensivity;

        public Form1()
        {
            InitializeComponent();
            whatShape = 0;
            IsFigureDragged = false;
            timer1.Stop();
            intensivity = 5;
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            bool yes = false; //мы попали хотя бы по одной точке? флаг нужен для того, чтобы не рисовать под нарисованной вершиной еще одну
                              
      //  начинаем просматривать каждую вершину
            foreach (MyShape s in sh)
            {
                if (s.Check(e.X, e.Y)) // мышка попала в область вершины?
                {
                    // drag&Drop вершины
                    if (e.Button == MouseButtons.Left)
                    {
                        s.beingDragged = true;
                        s.del_x = e.X - s.SetX;
                        s.del_y = e.Y - s.SetY;
                        yes = true; // зафиксировано попадание
                    }
                    // удаление вершины
                    if (e.Button == MouseButtons.Right)
                    {
                        sh.Remove(s);
                        this.Invalidate();
                        break; // уходим из обработчика, рисовать нового не надо
                    }
                }
            } //конец просмотра существующих вершин

              // уходим из обработчика - рисовать не надо, координаты изменения зафиксированы
            if (!yes)  // ни по одной вершине не попали
            {
                if (e.Button == MouseButtons.Left) 
                {
                    if (sh.Count > 2) //многоугольник уже есть
                    {
                        if (MoveObolochka(new Point(e.X, e.Y)))   //мышка попала внутрь многоугольника?
                        {
                            IsFigureDragged = true;
                            foreach (MyShape s in sh)
                            {
                                s.del_x = e.X - s.SetX;
                                s.del_y = e.Y - s.SetY;
                            }
                        }
                        else   // мышка не попала внутрь многоугольника
                            switch (whatShape) // Добавление новой вершины теперь будет сразу в список
                            {
                                case 0: sh.Add(new Circle(e.X, e.Y)); break;
                                case 1: sh.Add(new Square(e.X, e.Y)); break;
                                case 2: sh.Add(new Triangle(e.X, e.Y)); break;
                            }
                    }
                    else // Добавление новой вершины в список  
                        switch (whatShape)
                        {
                            case 0: sh.Add(new Circle(e.X, e.Y)); break;
                            case 1: sh.Add(new Square(e.X, e.Y)); break;
                            case 2: sh.Add(new Triangle(e.X, e.Y)); break;
                        }
                    
                } // нажата левая клавиша на пустом месте формы
                 
                }  // конец случая не попали мышкой в вершину
            this.Invalidate();  //Refresh();
        }
                
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            foreach (MyShape s in sh)
            {
                if (s.beingDragged)
                {
                    s.beingDragged = false;
                    //Refresh();
                }
            }
            // Если уже есть возможность строить выпуклый многоугольник, то проверяем состояние вершин и внутренние удаляем
            if (sh.Count > 2)
                for (int i = 0; i < sh.Count; i++) if(!sh[i].isInside ) sh.Remove(sh[i]);

            IsFigureDragged = false; // выключили флаг на перетаскивание многоугольника
            this.Invalidate();
        }
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsFigureDragged) // при перетаскивании пересчитываем все вершины
              foreach (MyShape s in sh)
                {
                    s.SetX = e.X - s.del_x;
                    s.SetY = e.Y - s.del_y;
                }
              else
                foreach (MyShape s in sh)
            {
                if (s.beingDragged) // теперь флаг для каждой вершины свой
                {
                    s.SetX = e.X - s.del_x;  // и смещение от цента до мышки свое
                    s.SetY = e.Y - s.del_y;
                }
            }
            this.Invalidate();
        }

       
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (sh.Any())
            {
                foreach (MyShape s in sh)
                {
                    s.Draw(g);
                }
                     
                if (sh.Count > 2)
                {
                    CreationObolochka(g);                    
                }
            }
        }
        #region Выпуклая оболочка 
        private void CreationObolochka(Graphics g) //Строим оболочку
        {
            foreach (MyShape s in sh)
            {
                // задаем, что вершины не принадлежат к выпуклой оболочке
                for (int i = 0; i < sh.Count; i++) sh[i].isInside = false;

                // циклы для перебора вершин
                for (int i = 0; i < sh.Count; i++)
                {
                    for (int j = i + 1; j < sh.Count; j++)
                    {
                        bool allShapesUP = true;  //все НАД вершиной
                        bool allShapesDOWN = true; // все ПОД вершиной
                                                   
                        for (int k = 0; k < sh.Count; k++)
                        {
                            if (k != j && k != i && i != j)
                            {
                                if ((sh[i].SetY - sh[j].SetY) * sh[k].SetX + (sh[j].SetX - sh[i].SetX) * sh[k].SetY + (sh[i].SetX * sh[j].SetY - sh[j].SetX * sh[i].SetY) >= 0)
                                    allShapesDOWN = false;
                                if ((sh[i].SetY - sh[j].SetY) * sh[k].SetX + (sh[j].SetX - sh[i].SetX) * sh[k].SetY + (sh[i].SetX * sh[j].SetY - sh[j].SetX * sh[i].SetY) < 0)
                                    allShapesUP = false;
                            }
                        }
                        if (allShapesDOWN  || allShapesUP)
                        {
                            sh[i].isInside = true;
                            sh[j].isInside = true;
                            //соединяем точки линиями, так как все точки по одну сторону  
                            g.DrawLine(new Pen(Color.Black), sh[i].SetX, sh[i].SetY, sh[j].SetX, sh[j].SetY);
                        }
                    }
                }
            }
        }
        private bool MoveObolochka(Point MouseXY) // Проверяем, где точка относительно оболочки для определения перетаскивания
        {
        
            List<MyShape> shOb = new List<MyShape>();             //создаем ссылку на новый список для проверки положения мышки         
            
                shOb.AddRange(sh); // Добавляем в новый исходный список
            shOb.Add(new Circle(MouseXY.X, MouseXY.Y));// добавляем туда же точку мышки

            // задаем, что вершины не принадлежат к выпуклой оболочке
            for (int i = 0; i < shOb.Count; i++) shOb[i].isInside = false;

            // циклы для перебора вершин
            for (int i = 0; i < shOb.Count; i++)
                {
                    for (int j = i + 1; j < shOb.Count; j++)
                    {
                        bool allShapesUP = true;  //все НАД вершиной
                        bool allShapesDOWN = true; // все ПОД вершиной

                    for (int k = 0; k < shOb.Count; k++)
                    {
                        if (k != j && k != i && i != j)
                        {
                            if ((shOb[i].SetY - shOb[j].SetY) * shOb[k].SetX + (shOb[j].SetX - shOb[i].SetX) * shOb[k].SetY + (shOb[i].SetX * shOb[j].SetY - shOb[j].SetX * shOb[i].SetY) >= 0)
                                allShapesDOWN = false;
                            if ((shOb[i].SetY - shOb[j].SetY) * shOb[k].SetX + (shOb[j].SetX - shOb[i].SetX) * shOb[k].SetY + (shOb[i].SetX * shOb[j].SetY - shOb[j].SetX * shOb[i].SetY) < 0)
                                allShapesUP = false;
                        }
                    }

                    if (allShapesDOWN || allShapesUP) // вершины попали в оболочку
                    {
                        shOb[i].isInside = true;
                        shOb[j].isInside = true;
                    }
                }// конец по j                
            } // конец по i

            if (shOb.Last().isInside) return false; // НЕ надо двигать (надо формировать новую вершину)
            else return true; // Надо двигать
        }
        #endregion Выпуклая оболочка 




        private void Form1_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
          
        }

        private void кругToolStripMenuItem_Click(object sender, EventArgs e)
        {
            whatShape = 0;
            this.Invalidate();
        }
        private void квадратToolStripMenuItem_Click(object sender, EventArgs e)
        {
            whatShape = 1;
            this.Invalidate();
        }
        private void треугольникToolStripMenuItem_Click(object sender, EventArgs e)
        {
            whatShape = 2;
            this.Invalidate();
        }

        private void цветВершинToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            MyShape.ColorShape = colorDialog1.Color;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Random rand = new Random();
            foreach (MyShape s in sh)
            { 
            s.SetX = s.SetX + rand.Next(-intensivity, intensivity);  
            s.SetY = s.SetY + rand.Next(-intensivity, intensivity);
            }
            Invalidate();
        }

        private void анимироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void начатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void остановитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }
        public void IntensivityDelegate(object sender, IntensivityEventArgs e)
        {
            intensivity = e.inten;
            Refresh();
        }

        private void изменитьЧастотуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 intens = new Form2();
            intens.Show();
            intens.ev += IntensivityDelegate;
        }
    }
}
