using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelixToolkit.Wpf;
using SharpDX;
using Color = System.Windows.Media.Color;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private bool isCubeAdded = false;
        private bool isSphereAdded = false;
        private BoxVisual3D cube;
        private SphereVisual3D sphere;

        private double topCut = 0;
        private double bottomCut = 0;

        private bool isCutVisible = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Создаем и добавляем на сцену куб
            var directionalLight = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(-0.5, -0.5, -0.5)
            };

            // Создание источника окружающего света
            var ambientLight = new AmbientLight
            {
                Color = Color.FromArgb(255, 255, 255, (byte)(255 * 0.2))
            };

            // Добавление источников света в сцену
            viewport.Children.Add(new DefaultLights());
            viewport.Children.Add(new ModelVisual3D { Content = directionalLight });
            viewport.Children.Add(new ModelVisual3D { Content = ambientLight });
            viewport.Children.Add(new GridLinesVisual3D());
  
            // Создаем и добавляем на сцену шар

        }

        private void ButtonCube_Click(object sender, RoutedEventArgs e)
        {
            if (!isCubeAdded)
            {
                // Добавляем куб на сцену
                cube = new BoxVisual3D
                {
                    Width = 10,
                    Height = 10,
                    Length = 10,
                    Material = Materials.Red,
                };
                viewport.Children.Add(cube);
                cube.Transform = new TranslateTransform3D(0, 0, 0);

                // Устанавливаем флаг добавления куба
                isCubeAdded = true;
            }
            else
            {
                // Убираем куб со сцены
                 viewport.Children.Remove(cube);

                // Устанавливаем флаг удаления куба
                isCubeAdded = false;
            }
        }

        private void ButtonSphere_Click(object sender, RoutedEventArgs e)
        {
            if (!isSphereAdded)
            {
                sphere = new SphereVisual3D
                {
                    Radius = 5,
                    Material = Materials.Blue,
                };

                viewport.Children.Add(sphere);
                sphere.Transform = new TranslateTransform3D(0, 0, 0);

                // Устанавливаем флаг добавления шара
                isSphereAdded = true;
            }
            else
            {
                // Убираем шар со сцены
                viewport.Children.Remove(sphere);

                // Устанавливаем флаг удаления шара
                isSphereAdded = false;
            }
        }

        private void ButtonCut_Click(object sender, RoutedEventArgs e)
        {
            // Получаем значения для отсечения фигуры
            int.TryParse(txtTopCut.Text, out int topCut);
            int.TryParse(txtBottomCut.Text, out int bottomCut);

            // Проверяем, что значение topCut не больше bottomCut
            if (topCut > 10 || bottomCut > 10 || topCut + bottomCut > 10)
            {
                topCut = 0;
                bottomCut = 0;
            }

            if (isCubeAdded)
            {
                var mesh = cube.Model.Geometry as MeshGeometry3D;
                for (int i = 0; i < mesh.Positions.Count; i++)
                {
                    var position = mesh.Positions[i];
                    if (position.Y > 5 - topCut)
                    {
                        position.Y = 5 - topCut;
                    }
                    if (position.Y < -5 + bottomCut)
                    {
                        position.Y = -5 + bottomCut;
                    }
                    mesh.Positions[i] = position;
                }
            }
            if (isSphereAdded)
            {
                var mesh = sphere.Model.Geometry as MeshGeometry3D;
                // Flatten the sphere by setting the Y component of some of its vertices to zero
                for (int i = 0; i < mesh.Positions.Count; i++)
                {
                    var position = mesh.Positions[i];
                    if (position.Y > 5 - topCut)
                    {
                        position.Y = 5 - topCut;
                    }
                    if (position.Y < -5 + bottomCut)
                    {
                        position.Y = -5 + bottomCut;
                    }
                    mesh.Positions[i] = position;
                }


            }
        }

        private void SimpleCut(MeshGeometry3D mesh)
        {
            // Flatten the sphere by setting the Y component of some of its vertices to zero
            for (int i = 0; i < mesh.Positions.Count; i++)
            {
                var position = mesh.Positions[i];
                if (position.Y > 5 - topCut)
                {
                    position.Y = 5 - topCut;
                }
                if (position.Y < -5 + bottomCut)
                {
                    position.Y = -5 + bottomCut;
                }
                mesh.Positions[i] = position;
            }
        }
        private void Cut()
        {
            // Define the slice plane as a point and a normal vector
            var planePoint = new Point3D(1, 0, 0);
            var planeNormal = new Vector3D(0, 0, 0);

            // Get the MeshGeometry3D of the original cube
            var mesh = cube.Model.Geometry as MeshGeometry3D;

            var sliceMesh = new MeshGeometry3D();


            // Loop through the triangles of the original mesh
            for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
            {
                // Get the indices of the three vertices of the triangle
                var index1 = mesh.TriangleIndices[i];
                var index2 = mesh.TriangleIndices[i + 1];
                var index3 = mesh.TriangleIndices[i + 2];

                // Get the vertices of the triangle
                var v1 = mesh.Positions[index1];
                var v2 = mesh.Positions[index2];
                var v3 = mesh.Positions[index3];

                // Check if the triangle intersects the slice plane
                var isIntersecting = Intersects(planePoint, planeNormal, v1, v2, v3);

                if (isIntersecting)
                {
                    // Calculate the intersection points of the triangle edges with the slice plane
                    var intersection1 = Intersect(planePoint, planeNormal, v1, v2);
                    var intersection2 = Intersect(planePoint, planeNormal, v2, v3);
                    var intersection3 = Intersect(planePoint, planeNormal, v3, v1);

                    // Add the intersection points to the new mesh
                    var index4 = sliceMesh.Positions.Count;
                    sliceMesh.Positions.Add(intersection1);
                    sliceMesh.Positions.Add(intersection2);
                    sliceMesh.Positions.Add(intersection3);

                    // Create the indices for the new triangles
                    sliceMesh.TriangleIndices.Add(index1);
                    sliceMesh.TriangleIndices.Add(index2);
                    sliceMesh.TriangleIndices.Add(index4);

                    sliceMesh.TriangleIndices.Add(index2);
                    sliceMesh.TriangleIndices.Add(index3);
                    sliceMesh.TriangleIndices.Add(index4);

                    sliceMesh.TriangleIndices.Add(index3);
                    sliceMesh.TriangleIndices.Add(index1);
                    sliceMesh.TriangleIndices.Add(index4);
                }
            }

            // Create a Material for the slice
            var material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));

            // Create a new GeometryModel3D for the slice
            var sliceModel = new GeometryModel3D(sliceMesh, material);

            // Create a new ModelVisual3D for the slice
            var sliceVisual = new ModelVisual3D();
            sliceVisual.Content = sliceModel;

            // Add the slice to the HelixViewport3D
            viewport.Children.Add(sliceVisual);
        }

        // Calculates the intersection point of a line segment and a plane
        Point3D Intersect(Point3D point, Vector3D normal, Point3D p1, Point3D p2)
        {
            var direction = p2 - p1;

            // Find the distance between the line segment and the plane
            var t = -(normal.X * (point.X - p1.X) + normal.Y * (point.Y - p1.Y) + normal.Z * (point.Z - p1.Z)) /
                    (normal.X * direction.X + normal.Y * direction.Y + normal.Z * direction.Z);

            // Calculate the intersection point
            var intersection = new Point3D(point.X + t * direction.X, point.Y + t * direction.Y,
                point.Z + t * direction.Z);

            return intersection;
        }

        // Checks if a triangle intersects a plane
        bool Intersects(Point3D point, Vector3D normal, Point3D v1, Point3D v2, Point3D v3)
        {
            // Calculate the distances between the vertices and the plane
            var d1 = (v1.X - point.X) * normal.X + (v1.Y - point.Y) * normal.Y + (v1.Z - point.Z) * normal.Z;
            var d2 = (v2.X - point.X) * normal.X + (v2.Y - point.Y) * normal.Y + (v2.Z - point.Z) * normal.Z;
            var d3 = (v3.X - point.X) * normal.X + (v3.Y - point.Y) * normal.Y + (v3.Z - point.Z) * normal.Z;

            // Check if the triangle intersects the plane
            if ((d1 > 0 && d2 > 0 && d3 > 0) || (d1 < 0 && d2 < 0 && d3 < 0))
            {
                return false;
            }

            return true;
        }
    }
}
