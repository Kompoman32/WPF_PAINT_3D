using Paint.classes;
using Paint.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Paint
{
    /// <summary>
    /// Логика взаимодействия для Matrix.xaml
    /// </summary>
    public partial class Morph_Window : Window
    {
        Action<IEnumerable<IMyObject>> changeSelectetObjectsAction;
        Action<IEnumerable<CustomLine>> addLinesActon;
        Action<IEnumerable<CustomLine>> removeLinesActon;

        static Random rnd = new Random();
        Dispatcher _disp;


        public Morph_Window(Dispatcher disp, IEnumerable<IMyObject> objects, Action<IEnumerable<IMyObject>> changeSelectetObjectsAction,
                            Action<IEnumerable<CustomLine>> addLinesActon, Action<IEnumerable<CustomLine>> removeLinesActon)
        {
            _disp = disp;
            InitializeComponent();

            this.changeSelectetObjectsAction = changeSelectetObjectsAction;
            this.addLinesActon = addLinesActon;
            this.removeLinesActon = removeLinesActon;

            sourceObjects.AddRange(objects);

            currentObjects = this.sourceObjects;
            currentTextBLock = SourceLinesBlock;
            UpdateVisual();

            DataContext = this;
        }

        string defaultOffset = "    ";


        bool isMoving = false;
        bool isAvailableButtons = false;
        bool isAvailableScrolls= false;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isMoving) e.Cancel = true;
        }

        List<IMyObject> currentObjects;
        TextBlock currentTextBLock;


        List<IMyObject> sourceObjects = new List<IMyObject>();
        List<IMyObject> targetObjects = new List<IMyObject>();

        Button lastButton;

        private void ChooseSourceOrTargetButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (lastButton != button)
            {
                lastButton = button;

                if (lastButton == ChooseSourceButton)
                {
                    ChooseSourceButton.Background = new SolidColorBrush(Colors.Purple);
                    ChooseSourceButton.Foreground = new SolidColorBrush(Colors.White);

                    ChooseTargetButton.Background = new SolidColorBrush(Colors.White);
                    ChooseTargetButton.Foreground = new SolidColorBrush(Colors.Black);

                    SetCurrentObjects(sourceObjects, SourceLinesBlock);
                }
                else
                {
                    ChooseSourceButton.Background = new SolidColorBrush(Colors.White);
                    ChooseSourceButton.Foreground = new SolidColorBrush(Colors.Black);

                    ChooseTargetButton.Background = new SolidColorBrush(Colors.Purple);
                    ChooseTargetButton.Foreground = new SolidColorBrush(Colors.White);

                    SetCurrentObjects(targetObjects, TargetLinesBlock);
                }
            }
        }

        private void SetCurrentObjects(List<IMyObject> objects, TextBlock textblock)
        {
            currentObjects.Select(x => { x.UnSelect(); return true; }).ToList();
            currentObjects = objects;
            currentObjects.Select(x => { x.Select(); return true; }).ToList();

            currentTextBLock = textblock;
            changeSelectetObjectsAction.Invoke(currentObjects);
        }

        private string GetStringOnLine(string offset, CustomLine line)
        {
            return $"{offset}({line.X1:00.00};{line.Y1:00.00};{line.Z1:00.00}) " +
                   $"({line.X2:00.00};{line.Y2:00.00};{line.Z2:00.00})\n" +
                   $"{offset}  {line.Equation}\n";
        }
        private string GetStringOnObjects(string offset, IEnumerable<IMyObject> objects)
        {
            string str = $"{offset}---\n";
            foreach (var o in objects)
            {
                if (o is CustomLine)
                {
                    str += GetStringOnLine(offset, o as CustomLine);
                }

                if (o is CustomGroup)
                {
                    str += GetStringOnObjects(offset + defaultOffset, (o as CustomGroup).GetObjects());
                }
            }

            str += $"{offset}---\n";

            return str;
        }

        public void AddObject(IMyObject obj)
        {
            currentObjects.Add(obj);
            UpdateVisual();
        }
        public void AddRangeObjects(IEnumerable<IMyObject> objects)
        {
            currentObjects.AddRange(objects);
            UpdateVisual();
        }
        public void RemoveObject(IMyObject obj)
        {
            currentObjects.Remove(obj);
            UpdateVisual();
        }
        public void ClearObjects()
        {
            currentObjects.Clear();
            UpdateVisual();
        }
        public void ClearAll()
        {
            sourceObjects.Clear();
            targetObjects.Clear();
            UpdateVisual();
        }

        public void UpdateVisual()
        {
            if (isMoving) return;

            currentTextBLock.Text = GetStringOnObjects("", currentObjects);

            if (sourceObjects.Count > 0 && targetObjects.Count > 0)
            {
                if (!isAvailableButtons)
                {
                    StartButton.Background = new SolidColorBrush(Colors.White);
                    ManualStartButton.Background = new SolidColorBrush(Colors.White);
                }

                isAvailableButtons = true;
                isAvailableScrolls = true;
            }
            else
            {
                if (isAvailableButtons)
                {
                    StartButton.Background = new SolidColorBrush(Colors.LightGray);
                    ManualStartButton.Background = new SolidColorBrush(Colors.LightGray);
                }

                isAvailableButtons = false;
                isAvailableScrolls = false;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            isMoving = true;
            isAvailableButtons = false;
            isAvailableScrolls = false;

            scrollBar.Value = 0;

            var t = new Thread(() => DoMoving());
            t.Start();
        }

        private void ManualStartButton_Click(object sender, RoutedEventArgs e)
        {
            isMoving = !isMoving;
            isAvailableButtons = false;
        }

        private void DoMoving()
        {
            var lines = GetLines();

            Action<double, Tuple<CustomLine, Matrix, Matrix>> func = (double t, Tuple<CustomLine, Matrix, Matrix> l) =>
            {
                var newX1 = l.Item2.M11 * (1 - t) + l.Item3.M11 * t;
                var newY1 = l.Item2.M21 * (1 - t) + l.Item3.M21 * t;
                var newZ1 = l.Item2.OffsetX * (1 - t) + l.Item3.OffsetX * t;

                var newX2 = l.Item2.M12 * (1 - t) + l.Item3.M12 * t;
                var newY2 = l.Item2.M22 * (1 - t) + l.Item3.M22 * t;
                var newZ2 = l.Item2.OffsetY * (1 - t) + l.Item3.OffsetY * t;


                l.Item1.MoveTo(newX1, newX2, newY1, newY2, newZ1, newZ2);
            };

            _disp.Invoke(() =>
            {
                addLinesActon.Invoke(lines.Select(x => x.Item1));
            });

            for (double t = 0; t < 1; t+=0.1)
            {
                _disp.Invoke(() =>
                {
                    foreach(var l in lines)
                    {
                        func(t, l);
                    }
                });
                Thread.Sleep(100);
            }

            _disp.Invoke(() =>
            {
                foreach (var l in lines)
                {
                    func(1, l);
                }
                Thread.Sleep(100);
                removeLinesActon.Invoke(lines.Select(x => x.Item1));
                isMoving = false;
            });
        }

        private List<Tuple<CustomLine, Matrix, Matrix>> GetLines()
        {
            var sourceLines = new List<CustomLine>();

            foreach (var o in sourceObjects)
            {
                if (o is CustomLine)
                {
                    sourceLines.Add(o as CustomLine);
                }
                if (o is CustomGroup)
                {
                    sourceLines.AddRange((o as CustomGroup).GetShapes().Cast<CustomLine>());
                }
            }

            var targetLines = new List<CustomLine>();

            foreach (var o in targetObjects)
            {
                if (o is CustomLine)
                {
                    targetLines.Add(o as CustomLine);
                }
                if (o is CustomGroup)
                {
                    targetLines.AddRange((o as CustomGroup).GetShapes().Cast<CustomLine>());
                }
            }


            SolidColorBrush brush = null;
            _disp.Invoke(() =>
            {
                brush = new SolidColorBrush(Colors.Orange);
            });



            List<Tuple<CustomLine, Matrix, Matrix>> lines = new List<Tuple<CustomLine, Matrix, Matrix>>();

            if (sourceLines.Count > targetLines.Count)
            {
                for (var i = 0; i < targetLines.Count; i++)
                {
                    var index = rnd.Next(sourceLines.Count);
                    var line = sourceLines[index];
                    var targetLine = targetLines[i];
                    _disp.Invoke(() =>
                    {
                        var li = new CustomLine(_disp, brush, line.Point1, line.Point2);
                        lines.Add(new Tuple<CustomLine, Matrix, Matrix>(
                            li,
                            new Matrix(
                                line.Point1.X, line.Point2.X,
                                line.Point1.Y, line.Point2.Y,
                                line.Point1.Z, line.Point2.Z
                            ),
                            new Matrix(
                                targetLine.Point1.X, targetLine.Point2.X,
                                targetLine.Point1.Y, targetLine.Point2.Y,
                                targetLine.Point1.Z, targetLine.Point2.Z
                            )
                        ));
                    });
                }
            }
            else
            {
                if (sourceLines.Count < targetLines.Count)
                {
                    for (var i = 0; i < sourceLines.Count; i++)
                    {
                        var line = sourceLines[i];
                        var targetLine = targetLines[i];

                        _disp.Invoke(() =>
                        {
                            lines.Add(new Tuple<CustomLine, Matrix, Matrix>(
                                new CustomLine(brush, line.Point1, line.Point2),
                                new Matrix(
                                    line.Point1.X, line.Point2.X,
                                    line.Point1.Y, line.Point2.Y,
                                    line.Point1.Z, line.Point2.Z
                                ),
                                new Matrix(
                                    targetLine.Point1.X, targetLine.Point2.X,
                                    targetLine.Point1.Y, targetLine.Point2.Y,
                                    targetLine.Point1.Z, targetLine.Point2.Z
                                )
                            ));
                        });

                    }

                    for (var i = sourceLines.Count; i < targetLines.Count; i++)
                    {
                        var index = rnd.Next(sourceLines.Count);
                        var line = sourceLines[index];
                        var targetLine = targetLines[i];

                        _disp.Invoke(() =>
                        {
                            lines.Add(new Tuple<CustomLine, Matrix, Matrix>(
                                new CustomLine(brush, line.Point1, line.Point2),
                                new Matrix(
                                    line.Point1.X, line.Point2.X,
                                    line.Point1.Y, line.Point2.Y,
                                    line.Point1.Z, line.Point2.Z
                                ),
                                new Matrix(
                                    targetLine.Point1.X, targetLine.Point2.X,
                                    targetLine.Point1.Y, targetLine.Point2.Y,
                                    targetLine.Point1.Z, targetLine.Point2.Z
                                )
                            ));
                        });
                    }
                }
                else
                {
                    for (var i = 0; i < targetLines.Count; i++)
                    {
                        var index = rnd.Next(sourceLines.Count);
                        var line = sourceLines[i];
                        var targetLine = targetLines[i];
                        _disp.Invoke(() =>
                        {
                            lines.Add(new Tuple<CustomLine, Matrix, Matrix>(
                                new CustomLine(brush, line.Point1, line.Point2),
                                new Matrix(
                                    line.Point1.X, line.Point2.X,
                                    line.Point1.Y, line.Point2.Y,
                                    line.Point1.Z, line.Point2.Z
                                ),
                                new Matrix(
                                    targetLine.Point1.X, targetLine.Point2.X,
                                    targetLine.Point1.Y, targetLine.Point2.Y,
                                    targetLine.Point1.Z, targetLine.Point2.Z
                                )
                            ));
                        });
                    }
                }
            }

            return lines;
        }
    }
}
