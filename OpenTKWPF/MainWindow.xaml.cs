using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenTKWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GLControl glControl;
        PolygonMode mod = PolygonMode.Fill;
        ShadingModel mod2 = ShadingModel.Smooth;
        int texture = -1;
        double scale = 1;
        double angleZ = 0;
        double angleX = 0;
        double angleY = 0;
        bool rej = true;
        bool peg = true;
        bool checkT = true;
        int maxH = 200;
        int cylinderRadius = 50;
        int sphereRadius = 100;
        float lp0 = 100;
        float lp1 = 0;
        //https://www.opengl.org/wiki/Sampler_(GLSL)
        List<Tuple<Vector3d, Vector3d>> cylinder = new List<Tuple<Vector3d, Vector3d>>();
        List<Tuple<Vector3d, Vector3d>> sphere = new List<Tuple<Vector3d, Vector3d>>();
        int slices = 500;
        int stacks = 500;
        List<Vector3d> CylinderCor = new List<Vector3d>();
        List<Vector3d> CylinderNor = new List<Vector3d>();
        List<Vector3d> CylinderTex = new List<Vector3d>();
        List<Vector3d> SphereCor = new List<Vector3d>();
        List<Vector3d> SphereNor = new List<Vector3d>();
        List<Vector3d> SphereTex = new List<Vector3d>();

        int handle = -1;

        DateTime prevTime = new DateTime();
        double t = 0;
        double coef = 1;

        long prevTick = 0;

        private VertexBuffer<NormalTexturedVertex> vertexBuffer;
        private ShaderProgram shaderProgram;
        private VertexArray<NormalTexturedVertex> vertexArray;
        private Matrix4Uniform projectionMatrix;
        private Matrix4Uniform worldMatrix;
        private Matrix4Uniform normalMatrix;
        private AnimationUniform animationUniform;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

        }

        private void CreateVertices(int slices, int stacks)
        {
            cylinder = new List<Tuple<Vector3d, Vector3d>>();
            sphere = new List<Tuple<Vector3d, Vector3d>>();

            //var cylinderRadius = 50;
            //var sphereRadius = 100;
            //var maxH = 200;

            for (int i = 0; i < slices; i++)
            {
                var h = i * maxH / (slices - 1);
                var thetta = Math.PI * i / (slices - 1) - Math.PI / 2;

                for (int j = 0; j < stacks; j++)
                {
                    var phi = j * Math.PI * 2 / (stacks - 1);

                    var cylinderNormal = new Vector3d(cylinderRadius * Math.Sin(phi), 0, cylinderRadius * Math.Cos(phi));
                    cylinderNormal.Normalize();
                    cylinder.Add(
                    new Tuple<Vector3d, Vector3d>(
                        new Vector3d(cylinderRadius * Math.Sin(phi), h - maxH / 2, cylinderRadius * Math.Cos(phi)),
                        cylinderNormal));

                    var sphereNormal = new Vector3d(sphereRadius * Math.Cos(phi) * Math.Cos(thetta), sphereRadius * Math.Sin(phi) * Math.Cos(thetta), sphereRadius * Math.Sin(thetta));
                    sphereNormal.Normalize();
                    sphere.Add(
                        new Tuple<Vector3d, Vector3d>(
                            new Vector3d(sphereRadius * Math.Cos(phi) * Math.Cos(thetta), sphereRadius * Math.Sin(phi) * Math.Cos(thetta), sphereRadius * Math.Sin(thetta)),
                            sphereNormal
                            ));
                }
            }
        }

        private void CreateVertices1(int slices, int stacks)
        {
            cylinder = new List<Tuple<Vector3d, Vector3d>>();
            sphere = new List<Tuple<Vector3d, Vector3d>>();

            //var cylinderRadius = 50;
            //var sphereRadius = 100;
            //var maxH = 200;

            for (int i = 0; i < slices; i++)
            {
                var h = i * maxH / (slices - 1);
                var thetta = Math.PI * i / (slices - 1) - Math.PI / 2;

                for (int j = 0; j < stacks; j++)
                {
                    var phi = j * Math.PI * 2 / (stacks - 1);

                    var cylinderNormal = new Vector3d(cylinderRadius * Math.Sin(phi), 0, cylinderRadius * Math.Cos(phi));
                    cylinderNormal.Normalize();
                    CylinderCor.Add(
                        new Vector3d(cylinderRadius * Math.Sin(phi), h - maxH / 2, cylinderRadius * Math.Cos(phi)));
                    // cylinderNormal));
                    CylinderNor.Add(cylinderNormal);

                    var sphereNormal = new Vector3d(sphereRadius * Math.Cos(phi) * Math.Cos(thetta), sphereRadius * Math.Sin(phi) * Math.Cos(thetta), sphereRadius * Math.Sin(thetta));
                    sphereNormal.Normalize();
                    SphereCor.Add(
                            // new Tuple<Vector3d, Vector3d>(
                            new Vector3d(sphereRadius * Math.Cos(phi) * Math.Cos(thetta), sphereRadius * Math.Sin(phi) * Math.Cos(thetta), sphereRadius * Math.Sin(thetta)));
                    //  sphereNormal
                    //  ));
                    SphereNor.Add(sphereNormal);
                }
            }

         
            this.vertexBuffer = new VertexBuffer<NormalTexturedVertex>(NormalTexturedVertex.Size);
            for (int i = 0; i < slices - 1; i++)
            {
                for (int j = 0; j < stacks - 1; j++)
                {
                    var index = i * slices + j;
                    this.vertexBuffer.AddVertex(new NormalTexturedVertex(ToVector3(CylinderCor[index]), ToVector3(SphereCor[index]), ToVector3(CylinderNor[index]), ToVector3(SphereNor[index]), new Vector2(i / (slices - 1), j / (stacks - 1))));
                    index = (i + 1) * slices + j;
                    this.vertexBuffer.AddVertex(new NormalTexturedVertex(ToVector3(CylinderCor[index]), ToVector3(SphereCor[index]), ToVector3(CylinderNor[index]), ToVector3(SphereNor[index]), new Vector2((i + 1) / (slices - 1), j / (stacks - 1))));
                    index = (i + 1) * slices + j + 1;
                    this.vertexBuffer.AddVertex(new NormalTexturedVertex(ToVector3(CylinderCor[index]), ToVector3(SphereCor[index]), ToVector3(CylinderNor[index]), ToVector3(SphereNor[index]), new Vector2((i + 1) / (slices - 1), (j + 1) / (stacks - 1))));
                    index = i * slices + j + 1;
                    this.vertexBuffer.AddVertex(new NormalTexturedVertex(ToVector3(CylinderCor[index]), ToVector3(SphereCor[index]), ToVector3(CylinderNor[index]), ToVector3(SphereNor[index]), new Vector2(i / (slices - 1), (j + 1) / (stacks - 1))));
                }
            }
        }

        private void CreateShader()
        {
            var vertexShader = new Shader(ShaderType.VertexShader,
@"#version 130


uniform mat4 world;
uniform mat4 normalMatrix;
uniform mat4 projectionMatrix;
uniform float t;

// attributes of our vertex
in vec3 vPosition;
in vec3 vPosition2;
in vec3 vNormal;
in vec3 vNormal2;
in vec2 vTex;

out vec3 N; // Normal
out vec3 v; // World Position
out vec2 tex;

void main()
{
    vec3 position = t * t * vPosition2 + t * (1 - t) * (vPosition + vPosition2) + (1 - t) * (1 - t) * vPosition;
    vec3 normal = t * t * vNormal2 + t * (1 - t) * (vNormal + vNormal2) + (1 - t) * (1 - t) * vNormal;

    // gl_Position is a special variable of OpenGL that must be set
    gl_Position = projectionMatrix * world * vec4(position, 1.0);
	N = vec3(normalMatrix * vec4(normal, 0.0));
    v = vec3(world * vec4(position, 1.0));
    tex = vTex;
}"
                );


            var perPixelFragmentCode = @"#version 130
in vec3 N;
in vec3 v;   
in vec2 tex; 

void main (void)  
{  
   vec3 L = normalize(gl_LightSource[0].position.xyz - v);   
   vec3 E = normalize(-v); // we are in Eye Coordinates, so EyePos is (0,0,0)  
   vec3 R = normalize(-reflect(L,N));  
 
   //calculate Ambient Term:  
   vec4 Iamb = gl_FrontLightProduct[0].ambient;    

   //calculate Diffuse Term:  
   vec4 Idiff = gl_FrontLightProduct[0].diffuse * max(dot(N,L), 0.0);    
   
   // calculate Specular Term:
   vec4 Ispec = gl_FrontLightProduct[0].specular 
                * pow(max(dot(R,E),0.0),0.3*gl_FrontMaterial.shininess);

   // write Total Color:  
   gl_FragColor = Iamb + Idiff + Ispec; //gl_FrontLightModelProduct.sceneColor + Iamb + Idiff + Ispec;   
}";


            var fragmentShader = new Shader(ShaderType.FragmentShader, perPixelFragmentCode);

            // link shaders into shader program
            this.shaderProgram = new ShaderProgram(vertexShader, fragmentShader);

            // create vertex array to specify vertex layout
            this.vertexArray = new VertexArray<NormalTexturedVertex>(
                this.vertexBuffer, this.shaderProgram,
                new VertexAttribute("vPosition", 3, VertexAttribPointerType.Float, NormalTexturedVertex.Size, 0),
                new VertexAttribute("vPosition2", 3, VertexAttribPointerType.Float, NormalTexturedVertex.Size, 12),
                new VertexAttribute("vNormal", 3, VertexAttribPointerType.Float, NormalTexturedVertex.Size, 24),
                new VertexAttribute("vNormal2", 3, VertexAttribPointerType.Float, NormalTexturedVertex.Size, 36),
                new VertexAttribute("vTex", 2, VertexAttribPointerType.Float, NormalTexturedVertex.Size, 48)
                );

            // create projection matrix uniform
            this.projectionMatrix = new Matrix4Uniform("projectionMatrix");
            this.animationUniform = new AnimationUniform("t");
            this.worldMatrix = new Matrix4Uniform("world");
            this.normalMatrix = new Matrix4Uniform("normalMatrix");
            //this.projectionMatrix.Matrix =
            //    Matrix4.LookAt(new Vector3(100, 100, 100), new Vector3(0, 0, 0), new Vector3(0, 1, 0)) *
            //    Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 16f / 9, 10f, 1000f);
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            var time = DateTime.Now;
            var diff = (time - prevTime).TotalSeconds / 6;
            prevTime = time;
            if (checkT == true)
                t += coef * diff;
            if (t < 0)
            {
                t = 0;
                coef = 1;
            }
            if (t > 1)
            {
                t = 1;
                coef = -1;
            }

           
            long ticks = Stopwatch.GetTimestamp();
            var ticksBetween = ticks - prevTick;
            var secondsBetween = TimeSpan.FromTicks(ticksBetween).TotalSeconds;
            fpsText.Text = "FPS: " + (1 / secondsBetween).ToString();
            prevTick = ticks;

            glControl.Invalidate();
        }

        private Tuple<Vector3d, Vector3d> GetPoint(int index, double t)
        {
            var A = cylinder[index];
            var C = sphere[index];
            var B = new Tuple<Vector3d, Vector3d>(
                (A.Item1 + C.Item1) / 2,
                (A.Item2 + C.Item2) / 2);

            var result = new Tuple<Vector3d, Vector3d>(
                (1 - t) * (1 - t) * A.Item1 + 2 * (1 - t) * t * B.Item1 + t * t * C.Item1,
                (1 - t) * (1 - t) * A.Item2 + 2 * (1 - t) * t * B.Item2 + t * t * C.Item2
                );

            return result;
        }

        private Tuple<Vector3d, Vector3d> GetPoint1(int index, double t)
        {
            var A1 = CylinderCor[index];
            var C1 = SphereCor[index];
            var A2 = CylinderNor[index];
            var C2 = SphereNor[index];
            var B = new Tuple<Vector3d, Vector3d>(
                (A1 + C1) / 2,
                (A2 + C2) / 2);

            var result = new Tuple<Vector3d, Vector3d>(
                (1 - t) * (1 - t) * A1 + 2 * (1 - t) * t * B.Item1 + t * t * C1,
                (1 - t) * (1 - t) * A2 + 2 * (1 - t) * t * B.Item2 + t * t * C2
                );

            return result;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            glControl = new GLControl();

            glControl.Load += GlControl_Load;
            glControl.Paint += GlControl_Paint;
            glControl.Width = 600;
            glControl.Height = 600;
            windowsFormsHost.Child = glControl;
            texture = TextureLoader.LoadTexture("mgtu.png");

            CreateVertices1(slices, stacks);
            CreateShader();

            prevTime = DateTime.Now;
            prevTick = Stopwatch.GetTimestamp();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void GlControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            var w = glControl.Width;
            var h = glControl.Height;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.PolygonMode(MaterialFace.FrontAndBack, mod);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);

            float[] mat_diffuse = { 0.0f, 0.0f, 1.0f, 1.0f };
            float[] mat_specular = { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] mat_shininess = { 50.0f };
            float[] light_position = { lp0, lp1, 0.0f, 1.0f };
            float[] light_ambient = { 0.5f, 0.5f, 0.5f, 1.0f };

            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.ShadeModel(mod2);
            //GL.DrawArrays()
            GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, mat_diffuse);
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, mat_specular);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, mat_shininess);
            GL.Light(LightName.Light0, LightParameter.Position, light_position);
            GL.Light(LightName.Light0, LightParameter.Ambient, light_ambient);
            GL.Light(LightName.Light0, LightParameter.Diffuse, mat_diffuse);

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.ColorMaterial);


            //GL.Scale(scale, scale, scale);
            //GL.Rotate(angleX, new Vector3d(1, 0, 0));
            //GL.Rotate(angleY, new Vector3d(0, 1, 0));
            //GL.Rotate(angleZ, new Vector3d(0, 0, 1));
            var world = Matrix4.CreateScale((float)scale, (float)scale, (float)scale) * Matrix4.CreateRotationX((float)angleX) * Matrix4.CreateRotationY((float)angleY) * Matrix4.CreateRotationZ((float)angleZ);
            var normal = world;
            normal.Invert();
            normal.Transpose();

            var viewMatrix = Matrix4.LookAt(new Vector3(300, 300, 300), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            var projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)glControl.Width / glControl.Height, 10, 1000);

            this.projectionMatrix.Matrix = viewMatrix * projectionMatrix;
            this.worldMatrix.Matrix = world;
            this.normalMatrix.Matrix = normal;
            this.animationUniform.Value = t;

            
            this.shaderProgram.Use();
            this.projectionMatrix.Set(this.shaderProgram);
            this.worldMatrix.Set(this.shaderProgram);
            this.normalMatrix.Set(this.shaderProgram);
            this.animationUniform.Set(this.shaderProgram);

            
            this.vertexBuffer.Bind();
            this.vertexArray.Bind();
            
            this.vertexBuffer.BufferData();
            this.vertexBuffer.Draw();

          
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.UseProgram(0);

            glControl.SwapBuffers();
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(System.Drawing.Color.Black);

            int w = glControl.Width;
            int h = glControl.Height;


            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

           
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, w, 0, h, -1, 1);

            GL.Viewport(0, 0, w, h);

          
            this.handle = GL.GenBuffer();

            var vertices = new List<LabVertex>();
            for (int i = 0; i < this.cylinder.Count; i++)
            {
                vertices.Add(new LabVertex(ToVector3(cylinder[i].Item1), ToVector3(sphere[i].Item1), ToVector3(cylinder[i].Item2), ToVector3(sphere[i].Item2), new Vector2(0, 0)));
            }

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(LabVertex.Size * this.cylinder.Count),
                vertices.ToArray(), BufferUsageHint.StreamDraw);

        }

        private Vector3 ToVector3(Vector3d vec)
        {
            return new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
        }

        private void SaveStateToFile(string fileName)
        {
            File.WriteAllText(fileName, t.ToString(CultureInfo.InvariantCulture) + "," + coef.ToString());
        }

        private void LoadStateFromFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                var contents = File.ReadAllText(fileName);
                var split = contents.Split(',');

                t = double.Parse(split[0], CultureInfo.InvariantCulture);
                coef = int.Parse(split[1]);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Q:
                    scale += 0.05;
                    CreateVertices1(slices, stacks);
                    break;
                case Key.A:
                    scale -= 0.05;
                    CreateVertices1(slices, stacks);
                    break;
                case Key.W:
                    rej = !rej;
                    CreateVertices1(slices, stacks);
                    break;
                case Key.S:
                    peg = !peg;
                    CreateVertices1(slices, stacks);
                    break;
                case Key.D:
                    angleX += 0.5;
                    //CreateVertices(slices, stacks);
                    break;
                case Key.F:
                    angleY += 0.5;
                    //CreateVertices(slices, stacks);
                    break;
                case Key.X:
                    angleX -= 0.5;
                    //CreateVertices(slices, stacks);
                    break;
                case Key.C:
                    angleY -= 0.5;
                    //CreateVertices(slices, stacks);
                    break;
                case Key.G:
                    angleZ += 1;
                    //CreateVertices(slices, stacks);
                    break;
                case Key.V:
                    angleZ -= 1;
                    //CreateVertices(slices, stacks);
                    break;
                case Key.E:
                    checkT = !checkT;
                    break;
                case Key.R:
                    SaveStateToFile("1.txt");
                    break;
                case Key.T:
                    LoadStateFromFile("1.txt");
                    break;
                case Key.Y:
                    if (mod == PolygonMode.Fill)
                        mod = PolygonMode.Line;
                    else mod = PolygonMode.Fill;
                    break;
                case Key.U:
                    slices += 10;
                    stacks += 10;
                    CreateVertices1(slices, stacks);
                    break;
                case Key.I:
                    slices -= 10;
                    stacks -= 10;
                    CreateVertices1(slices, stacks);
                    break;
                case Key.H:
                    if (mod2 == ShadingModel.Smooth)
                        mod2 = ShadingModel.Flat;
                    else mod2 = ShadingModel.Smooth;
                    break;
                case Key.J:
                    lp0 -= 50;
                    break;
                case Key.N:
                    lp0 += 50;
                    break;
                case Key.K:
                    lp1 -= 50;
                    break;
                case Key.M:
                    lp1 += 50;
                    break;


                default:
                    break;
            }
            glControl.Invalidate();


        }
    }
}


