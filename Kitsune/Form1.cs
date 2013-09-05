using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kitsune.Stage;
using Kitsune.VM;
using System.IO;
namespace Kitsune
{
    public partial class Form1 : Form
    {
        BlockController controller;
        Stage.Stage stage;
        VM.VM vm;
        VM.Compiler compiler;
        Sprite kitsune;
        Control runForm;
        Graphics runGraphics;
        Document document;
        Random random = new Random();
        public Form1()
        {
            InitializeComponent();         
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Graphics g = panel1.CreateGraphics();

            controller = new BlockController(g, this.Font, panel1.Size, () => MakeTextBox());
            controller.Modified += new ControllerModified(controller_Modified);
            document = new Document("Kitsune");
            document.TitleChange += new TitleChangeEvent(document_TitleChange);
            document.SetTitle();
            RegisterMethods();
            controller.InitPalette(); // after methods are registered

            vm = new VM.VM();
            compiler = new Compiler(vm, controller.GetBlockSpace());

            runForm = panel2;
            //runForm.Location = this.Location;
            //runForm.Size = this.Size;
            runGraphics = runForm.CreateGraphics();
            stage = new Stage.Stage(runGraphics, this.Font, runForm.ClientSize);
            kitsune = new Sprite(BitmapExtensions.LoadBmp("kitsune2.bmp"),
                runForm.ClientRectangle.Center().Offseted(-15, -15),
                true,
                -90);
            

            runForm.Paint += new PaintEventHandler(runForm_Paint);
            runForm.DoubleClick += new EventHandler(runForm_DoubleClick);
            stage.AddSprite(kitsune);
            stage.RedrawAll();
            PrepareVM(vm, stage);
            PrepareCompiler(compiler);


            // I really should use a testing framework
            // testSplitFuncArgs();
            // testCBlockView(new Point(10, 10));
            // testReporterBlockView(new Point(250, 10));
            // testBlockStackView(new Point(450, 10));

        }

        void document_TitleChange(object sender, string newTitle)
        {
            this.Text = newTitle;
        }

        void controller_Modified(object sender)
        {
            document.Modify();
        }

        void runForm_DoubleClick(object sender, EventArgs e)
        {
            runForm.Hide();
        }

        void runForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(stage.Buffer, 0, 0);
        }

       private void PrepareVM(VM.VM vm, Stage.Stage stage)
        {
            vm.RegisterPrimitve("fd", delegate(object[] args)
            {
                stage.Fwd(kitsune, (double)args[0]);
                return null;
            });

            vm.RegisterPrimitve("rt", delegate(object[] args)
            {
                stage.WithSprite(kitsune, (s) => s.rt((float)(double)args[0]));
                return null;
            });

            vm.RegisterPrimitve("lt", delegate(object[] args)
            {
                stage.WithSprite(kitsune, (s) => s.lt((float)(double)args[0]));
                return null;
            });

            vm.RegisterPrimitve("gotoxy", delegate(object[] args)
            {
                float x = (float) (double)args[0];
                float y = (float)(double)args[1];
                stage.WithSprite(kitsune, (s) => s.Move(new PointF(x,y)));
                return null;
            });

            vm.RegisterPrimitve("random", delegate(object[] args)
            {
                int low = (int)(double)args[0];
                int hi = (int)(double)args[1];
                double r = random.Next(low, hi);
                return r;
            });

            vm.RegisterPrimitve("+", delegate(object[] args)
            {
                double a = (double)args[0];
                double b = (double)args[1];
                return a+b;
            });

            vm.RegisterPrimitve("-", delegate(object[] args)
            {
                double a = (double)args[0];
                double b = (double)args[1];
                return a - b;
            });

            vm.RegisterPrimitve("*", delegate(object[] args)
            {
                double a = (double)args[0];
                double b = (double)args[1];
                return a * b;
            });

            vm.RegisterPrimitve("/", delegate(object[] args)
            {
                double a = (double)args[0];
                double b = (double)args[1];
                return a / b;
            });

            vm.RegisterPrimitve("say", delegate(object[] args)
            {
                stage.Say(kitsune, args[0].ToString());
                return null;
            });

            vm.RegisterPrimitve("sin", delegate(object[] args)
            {
                double angle = (double)args[0];
                angle = angle * Math.PI / 180.0;
                double result = Math.Sin(angle);
                return result;
            });

            vm.RegisterPrimitve("cos", delegate(object[] args)
            {
                double angle = (double)args[0];
                angle = angle * Math.PI / 180.0;
                double result = Math.Cos(angle);
                return result;
            });

            vm.RegisterPrimitve("tan", delegate(object[] args)
            {
                double angle = (double)args[0];
                angle = angle * Math.PI / 180.0;
                double result = Math.Tan(angle);
                return result;
            });

            vm.RegisterPrimitve("asin", delegate(object[] args)
            {
                double angle = (double)args[0];
                angle = angle * Math.PI / 180.0;
                double result = Math.Asin(angle);
                return result;
            });

            vm.RegisterPrimitve("acos", delegate(object[] args)
            {
                double angle = (double)args[0];
                angle = angle * Math.PI / 180.0;
                double result = Math.Acos(angle);
                return result;
            });

            vm.RegisterPrimitve("atan", delegate(object[] args)
            {
                double angle = (double)args[0];
                angle = angle * Math.PI / 180.0;
                double result = Math.Atan(angle);
                return result;
            });


            vm.RegisterPrimitve("sqrt", delegate(object[] args)
            {
                double angle = (double)args[0];
                angle = angle * Math.PI / 180.0;
                double result = Math.Sqrt(angle);
                return result;
            });

            vm.RegisterPrimitve("doNothing", delegate(object[] args)
            {
                return null; 
            });
        }

       private void PrepareCompiler(Compiler compiler)
       {
           compiler.PrimitiveAliases["move % steps"] = "fd";
           compiler.PrimitiveAliases["move to x: % y: %"] = "gotoxy";
           compiler.PrimitiveAliases["turn % degrees right"] = "rt";
           compiler.PrimitiveAliases["turn % degrees left"] = "lt";
           
           compiler.PrimitiveAliases["% + %"] = "+";
           compiler.PrimitiveAliases["% - %"] = "-";
           compiler.PrimitiveAliases["% * %"] = "*";
           compiler.PrimitiveAliases["% / %"] = "/";
           compiler.PrimitiveAliases["random from % to %"] = "random";
           compiler.PrimitiveAliases["sin %"] = "sin";
           compiler.PrimitiveAliases["cos %"] = "cos";
           compiler.PrimitiveAliases["tan %"] = "tan";
           compiler.PrimitiveAliases["asin %"] = "asin";
           compiler.PrimitiveAliases["acos %"] = "acos";
           compiler.PrimitiveAliases["atan %"] = "atan";
           compiler.PrimitiveAliases["sqrt %"] = "sqrt";
           
           compiler.PrimitiveAliases["say %"] = "say";
           compiler.PrimitiveAliases["when _flag_ clicked"] = "doNothing";
       }

        private TextBox MakeTextBox()
        {
            TextBox tb = new TextBox();
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.Parent = panel1;
            return tb;
        }

        private void RegisterMethods()
        {
            controller.RegisterSystemMethod("move % steps", BlockAttributes.Stack, DataType.Script, new DataType[]{ DataType.Number});
            controller.RegisterSystemMethod("turn % degrees right", BlockAttributes.Stack, DataType.Script, new DataType[] { DataType.Number });
            controller.RegisterSystemMethod("turn % degrees left", BlockAttributes.Stack, DataType.Script, new DataType[] { DataType.Number });
            controller.RegisterSystemMethod("move to x: % y: %", BlockAttributes.Stack, DataType.Script, new DataType[] { DataType.Number, DataType.Number });
            
            controller.RegisterSystemMethod("say %", BlockAttributes.Stack, DataType.Script, new DataType[] { DataType.Object});
            controller.RegisterSystemMethod("say % for % seconds", BlockAttributes.Stack, DataType.Script, new DataType[] { DataType.Object, DataType.Number});

            controller.RegisterSystemMethod("sin %", BlockAttributes.Report, DataType.Number, new DataType[] { DataType.Number });
            controller.RegisterSystemMethod("cos%", BlockAttributes.Report, DataType.Number, new DataType[] { DataType.Number });
            controller.RegisterSystemMethod("tan %", BlockAttributes.Report, DataType.Number, new DataType[] { DataType.Number });
            controller.RegisterSystemMethod("sqrt %", BlockAttributes.Report, DataType.Number, new DataType[] { DataType.Number });
            controller.RegisterSystemMethod("asin %", BlockAttributes.Report, DataType.Number, new DataType[] { DataType.Number });
            controller.RegisterSystemMethod("acos%", BlockAttributes.Report, DataType.Number, new DataType[] { DataType.Number });
            controller.RegisterSystemMethod("atan %", BlockAttributes.Report, DataType.Number, new DataType[] { DataType.Number });

            controller.RegisterSystemMethod("% + %", BlockAttributes.Report, DataType.Number, new DataType[] { DataType.Number, DataType.Number });
            controller.RegisterSystemMethod("% - %", BlockAttributes.Report, DataType.Number, new DataType[] { DataType.Number, DataType.Number });
            controller.RegisterSystemMethod("% * %", BlockAttributes.Report, DataType.Number, new DataType[] { DataType.Number, DataType.Number });
            controller.RegisterSystemMethod("% / %", BlockAttributes.Report, DataType.Number, new DataType[] { DataType.Number, DataType.Number });
            controller.RegisterSystemMethod("random from % to %", BlockAttributes.Report, DataType.Number, new DataType[] { DataType.Number, DataType.Number });
            
            controller.RegisterSystemMethod("if % then % else %", BlockAttributes.Stack, DataType.Script, new DataType[] { DataType.Number, DataType.Script, DataType.Script });
            controller.RegisterSystemMethod("repeat % times %", BlockAttributes.Stack, DataType.Script, new DataType[] { DataType.Number, DataType.Script });
            controller.RegisterSystemMethod("forever %", BlockAttributes.Cap, DataType.Script, new DataType[] { DataType.Script });
            controller.RegisterSystemMethod("wait % milliseconds", BlockAttributes.Stack, DataType.Script, new DataType[] { DataType.Number});

            controller.RegisterSystemMethod("when _flag_ clicked", BlockAttributes.Hat, DataType.Script, new DataType[] { });
            controller.RegisterSystemMethod("stop script", BlockAttributes.Cap, DataType.Script, new DataType[] { });
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            controller.Update(e.ClipRectangle);
            controller.Redraw(e.Graphics, e.ClipRectangle);
        }


        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            controller.MouseDown(e.Location);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            //controller.MarkSelectedView(e.Location);
            controller.MouseMove(e.Location);
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            controller.MouseUp(e.Location);
        }
        private InvokationBlock makeInvokationBlock(string invokation, DataType[] types, IBlock[] values)
        {
            List<DataType> typesList = new List<DataType>();
            typesList.AddRange(types);
            InvokationBlock ret = new InvokationBlock(invokation, BlockAttributes.Hat, typesList);
            ret.Args.AddRange(values, types);
            return ret;
        }

        BlockStack makeSampleBlockStack()
        {
            InvokationBlock b1 = makeInvokationBlock("move % steps",
             new DataType[] { DataType.Number },
             new IBlock[] { new TextBlock("") });

            InvokationBlock b2 = makeInvokationBlock("turn % degrees right",
                new DataType[] { DataType.Number },
                new IBlock[] { new TextBlock("") });

            BlockStack stack = new BlockStack();
            stack.AddRange(new IBlock[] { b1, b2 });
            return stack;
        }

        private void testReporterBlockView(Point at)
        {
            InvokationBlock b = makeInvokationBlock("sin %",
               new DataType[] { DataType.Number },
               new IBlock[] { makeInvokationBlock("% + %", new DataType[]{DataType.Number, DataType.Number},
                   new IBlock[]{new TextBlock(""), new TextBlock("")}) });

            controller.AddTopLevel(b, at);
        }

        private void testBlockStackView(Point at)
        {
            BlockStack stack = makeSampleBlockStack();
            controller.AddTopLevel(stack, at);
        }

        private void testCBlockView(Point at)
        {
            InvokationBlock b = makeInvokationBlock("if % then % else %",
                new DataType[] { DataType.Number, DataType.Script, DataType.Script },
                new IBlock[] { new TextBlock(""), makeSampleBlockStack(), makeSampleBlockStack() });

            controller.AddTopLevel(b, at);
        }

        private void testSplitFuncArgs()
        {
            string[] parts = "when _flag_ clicked".SplitFuncArgs();
        }

        private void Run()
        {
            foreach (IBlock b in controller.GetTopLevelBlocks())
            {
                if (b is BlockStack)
                {
                    IBlock s = ((BlockStack)b)[0];
                    if (s is InvokationBlock)
                    {
                        InvokationBlock ib = (InvokationBlock)s;
                        if (ib.Text == "when _flag_ clicked")
                        {
                            Run(b);
                            return;
                        }
                    }
                }
            }
        }

        private void Run(IBlock s)
        {
            Method m = compiler.Compile(s, true);
            vm.LaunchProcess(m);
            vm.Done = false;
            timer1.Start();
        }

        private void Terminate()
        {
            vm.Done = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (vm.Done)
            {
                timer1.Stop();
            }
            else
            {
                vm.RunStep();
            }
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void toolGreenFlag_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void terminateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Terminate();
        }

        private void toolStop_Click(object sender, EventArgs e)
        {
            Terminate();
        }

        private void panel1_Resize(object sender, EventArgs e)
        {
            if (controller == null)
                return;
            if (panel1.Size.Width == 0 || panel1.Size.Height == 0)
                return; // window's being minimized
            controller.Resize(panel1.Size, panel1.CreateGraphics());
            
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            panel2.Width = (int)((double)this.Width * 0.35);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CloseCurrentOrCancel())
            {
                controller.Clear();
                document.New();
            }
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CloseCurrentOrCancel())
            {
                OpenFileDialog dlg = new OpenFileDialog();
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Open(dlg.FileName); 
                    document.Load(dlg.FileName);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (document.Newfile)
            {
                SaveAs();
            }
            else
            {
                Save(document.Filename);
                document.Save(document.Filename);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseCurrentOrCancel();
        }

        private bool CloseCurrentOrCancel()
        {
            // Return true  -> Can safely close
            //        false -> Don't close
            string filename;
            bool exit, save;
            document.ActWhenClosing(out save, out exit, out filename);
            if (!exit)
                return false;
            if (save)
            {
                Save(filename);
            }
            return true;
        }

        private void Save(string filename)
        {
            Stream s = new FileStream(filename, FileMode.Create, FileAccess.Write);
            controller.Save(s);
            s.Close();
        }

        private void Open(string filename)
        {
            Stream s = new FileStream(filename, FileMode.Open, FileAccess.Read);
            controller.Load(s);
            s.Close();
        }
        private void SaveAs()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Save(dlg.FileName);
                document.Save(dlg.FileName);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void defineNewProcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditProcDefForm form = new EditProcDefForm();
            form.SetController(controller.NewProcDef(() => form.MakeTextBox(), form.GetEraseButton()));
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
        }

      
    }
}
