using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;

namespace Kitsune
{
    public class BlockViewFactory
    {
        StackBlockImgParts sb_parts;
        StackBlockImgParts fib_parts;
        StackBlockImgParts capb_parts;
        StackBlockImgParts hatb_parts;
        CBlockImgParts cb_parts;
        Nine ib_parts;
        Dictionary<string, Bitmap> specialTextBits = new Dictionary<string, Bitmap>();
        Graphics textMetrics;
        Font textFont, boldFont;
        BlockSpace blockSpace;
        Action modified;

        Dictionary<IBlock, IBlockView> blockViews;

        public BlockViewFactory(Graphics textMetrics,
            Font textFont, 
            BlockSpace blockSpace,
            Dictionary<IBlock, IBlockView> blockViews,
            Action modified)
        {
            this.modified = modified;
            this.textMetrics = textMetrics;
            this.textFont = textFont;
            boldFont = new Font(textFont, FontStyle.Bold);
            this.blockSpace = blockSpace;
            this.blockViews = blockViews;
            InitImageParts();
        }
        internal void ResetBlockSpace(BlockSpace blockSpace)
        {
            this.blockSpace = blockSpace;
        }
        void InitImageParts()
        {
            sb_parts = StackBlockImgParts.FromBitmap(BitmapExtensions.LoadBmp("stack_blue_small.bmp"));
            fib_parts = StackBlockImgParts.FromBitmap(BitmapExtensions.LoadBmp("function_green_small.bmp"));
            hatb_parts = StackBlockImgParts.FromBitmap(BitmapExtensions.LoadBmp("hat_small.bmp"));
            capb_parts = StackBlockImgParts.FromBitmap(BitmapExtensions.LoadBmp("cap_small.bmp"));
            
            cb_parts = new CBlockImgParts();
            cb_parts.FromBitmapCutting(BitmapExtensions.LoadBmp("C_stack_full.bmp"), Color.Red);
            ib_parts = new Nine();
            ib_parts.FromBitmapCutting(BitmapExtensions.LoadBmp("input_controls_small.bmp"), Color.Red);

            specialTextBits["_flag"] = (BitmapExtensions.LoadBmp("flag_textbit.bmp")).Transparent();
        }

    

        public IBlockView ViewFromBlockStack(BlockStack blocks)
        {
            IEnumerable<IBlockView> stack = blocks.Select(b => ViewFromBlock((InvokationBlock)b));
            BlockStackView ret = new BlockStackView(blocks, stack);
            blocks.OnInsert += new BlockStackInsertEvent(blockstack_OnInsert);
            return ret;
        }

        public IBlockView ViewFromBlock(IBlock block)
        {
            if (blockViews.ContainsKey(block))
                return blockViews[block];
            if (block is BlockStack)
            {
                IBlockView ret = ViewFromBlockStack((BlockStack)block);;
                blockViews[block] = ret;
                return ret;
            }

            if (!(block is InvokationBlock))
            {
                IBlockView ret = new TextView((TextBlock) block, ib_parts, textMetrics, textFont);
                blockViews[block] = ret;
                return ret;
            }

            InvokationBlock b = (InvokationBlock)block;
            BlockAttributes attr = blockSpace.AttributeOf(b);
            IBlockView r = ViewFromInvokationBlock(b, attr);
            blockViews[block] = r;
            return r;
        }

        public IBlockView ViewFromInvokationBlock(InvokationBlock b, BlockAttributes attribute)
        {
            string[] textParts = b.Text.SplitFuncArgs();
            Bitmap[] textBitmaps = RenderTextBits(textParts);

            if (b.ArgTypes.All(t => t != DataType.Script))
            {
                // it ain't a C block
                List<IBlockView> subContent = new List<IBlockView>();
                int i = 0;
                int currentArg = 0;
                BitArray trueArgs = new BitArray(textParts.Length);
                foreach (string s in textParts)
                {
                    if (s == "%")
                    {
                        subContent.Add(ViewFromBlock(b.Args[currentArg++]));
                        trueArgs[i] = true;
                    }
                    else
                    {
                        subContent.Add(new LabelView(textBitmaps[i]));
                        trueArgs[i] = false;
                    }
                    ++i;
                }


                NineContent imageParts = sb_parts; // dummy initial val
                switch (attribute)
                {
                    case BlockAttributes.Stack:
                        imageParts = sb_parts;
                        break;
                    case BlockAttributes.Report:
                        imageParts = fib_parts;
                        break;
                    case BlockAttributes.Cap:
                        imageParts = capb_parts;
                        break;
                    case BlockAttributes.Hat:
                        imageParts = hatb_parts;
                        break;
                }

                ContentView content = new ContentView(subContent.ToArray(), b.ArgTypes.ToArray(), trueArgs, imageParts);
                InvokationBlockView ib = new InvokationBlockView(b, attribute, content);
                b.OnArgChanged += new InvokationBlockArgChangeEvent(InvokationBlock_ArgChanged);
                return ib;
                
            }
            else
            {
                // it's a C block, yappari
                List<ContentView> contents = new List<ContentView>();
                List<IBlockView> subContent = new List<IBlockView>();
                List<DataType> subArgTypes = new List<DataType>();
                List<BlockStackView> scripts = new List<BlockStackView>();
                List<ArgumentPartition> argPartitions = new List<ArgumentPartition>();
                List<bool> trueArgs = new List<bool>();
                int currentArg = 0;
                int currentPartitionCount = 0;
                int i = 0;
                bool head = true;
                foreach (string s in textParts)
                {
                    if (s != "%")
                    {
                        LabelView lv = new LabelView(textBitmaps[i]);
                        subContent.Add(lv);
                        trueArgs.Add(false);
                    }
                    else
                    {
                        // It's an arg. Script or normal?
                        DataType type = b.ArgTypes[currentArg];
                        IBlock arg = b.Args[currentArg];
                        if (type != DataType.Script)
                        {
                            // Oh it's just a normal argument
                            IBlockView bv = ViewFromBlock(arg);
                            subContent.Add(bv);
                            subArgTypes.Add(type);
                            trueArgs.Add(true);
                            currentPartitionCount++;
                        }
                        else
                        {
                            // We need to split a new head or waist in the C block
                            NineContent nc;
                            if (head)
                            {
                                nc = cb_parts.Head;
                                head = false;
                            }
                            else
                            {
                                nc = cb_parts.Waist;
                            }
                            ContentView cv = new ContentView(subContent.ToArray(), subArgTypes.ToArray(), new BitArray(trueArgs.ToArray()), nc);
                            contents.Add(cv);
                            ArgumentPartition ap = new ArgumentPartition(currentPartitionCount, ArgViewType.Content);
                            argPartitions.Add(ap);
                            currentPartitionCount = 0;

                            BlockStackView side = (BlockStackView)ViewFromBlock((BlockStack)arg);
                            scripts.Add(side);
                            ap = new ArgumentPartition(1, ArgViewType.Script);
                            argPartitions.Add(ap);
                            subContent = new List<IBlockView>();
                            subArgTypes = new List<DataType>();
                            trueArgs = new List<bool>();

                        }
                        currentArg++;
                    }
                    i++;
                }
                b.OnArgChanged +=new InvokationBlockArgChangeEvent(InvokationBlock_ArgChanged);
                CBlockView cb = new CBlockView(b, attribute, contents, scripts, cb_parts, argPartitions);

                return cb;
            }
        }

        void InvokationBlock_ArgChanged(object sender, int arg, IBlock _old, IBlock _new)
        {
            IBlockView v = ViewFromBlock(_new);
            IInvokationBlockView parent = (IInvokationBlockView) ViewFromBlock((IBlock)sender);
            parent.SetArgView(arg, v);
            modified();
        }
        
        void blockstack_OnInsert(object sender, int i, IBlock b)
        {
            IBlockView v = ViewFromBlock(b);
            BlockStackView parent = (BlockStackView) ViewFromBlock((IBlock) sender);
            parent.InsertView(i, v);
            modified();
        }

        private Bitmap[] RenderTextBits(string[] textBits)
        {
            Bitmap[] TextBitBitmaps = new Bitmap[textBits.Length];
            int height = textBits.Max(t => TextBitHeight(t));
            for (int i = 0; i < textBits.Length; ++i)
                TextBitBitmaps[i] = MakeTextBitBitmap(textBits[i], height);

            return TextBitBitmaps;
        }

        private int TextBitHeight(string t)
        {
            if(!t.StartsWith("_"))
                return (int)textMetrics.MeasureString(t, textFont).Height;
            else
                return specialTextBits[t].Height;
        }

        private Bitmap MakeTextBitBitmap(string p, int height)
        {
            if (!p.StartsWith("_"))
            {
                int w = (int)textMetrics.MeasureString(p, boldFont).Width;
                Bitmap b = new Bitmap(w, height);
                Graphics g = Graphics.FromImage(b);
                g.Clear(Color.Transparent);
                g.DrawString(p, boldFont, Brushes.White, 0, 0);
                g.Dispose();
                return b;
            }
            else
            {
                return specialTextBits[p];
            }
        }

        internal void SetTextMetricsGraphics(Graphics g)
        {
            textMetrics = g;
        }
    }
}
