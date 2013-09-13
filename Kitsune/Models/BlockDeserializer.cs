using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using Kitsune.Models;

namespace Kitsune
{
    public class BlockDeserializer
    {
        ProcDefBlock currentProcDef = null;
        // This is a map from proc name to a 'type fingerprint' of comma-separated type names
        // e.g 'move % steps forward and say %' => 'DataType.Number,DataType.String'
        // the type fingerprint is computed by DataTypeNames.TypeFingerPrint
        Dictionary<string, string> procDeclarations = new Dictionary<string, string>();
        BlockSpace blockSpace;
        Action<ProcDefBlock> handleDefineNewProc;
        internal BlockSpace LoadBlockSpace(string json, 
            BlockSpace blockSpace, 
            Dictionary<string, BlockInfo> systemBlocks,
            Action<ProcDefBlock> handleDefineNewProc)
        {
            BlockSpace ret = blockSpace;
            this.blockSpace = blockSpace;
            this.handleDefineNewProc = handleDefineNewProc;
            JArray o = JArray.Parse(json);
            for (int i = 0; i < o.Count; ++i)
            {
                JToken scriptJson = o[i];
                TopLevelScript script = ToTopLevelScript(scriptJson, ret);
                ret.AddScript(script);
            }

            return ret;
        }

        private TopLevelScript ToTopLevelScript(JToken scriptJson, BlockSpace owner)
        {
            Point location;
            IBlock block;
            int x = int.Parse((string)scriptJson["location"][0]);
            int y = int.Parse((string)scriptJson["location"][1]);
            location = new Point(x, y);
            block = ToBlock(scriptJson["code"]);
            TopLevelScript ret = new TopLevelScript(location, block, owner);
            return ret;
        }

        private IBlock ToBlock(JToken json)
        {
            if (json.Type == JTokenType.Array)
            {
                if (json[0].ToString() == "do")
                    return ToBlockStack(json);
                else if (json[0].ToString() == "var")
                    return ToVarAccess(json);
                else if (json[0].ToString() == "define")
                {
                    ProcDefBlock b = ToProcDef(json);
                    handleDefineNewProc(b);
                    return b;
                }
                else // assume invokation block
                    return ToInvokationBlock(json);
            }
            else
            {
                return new TextBlock(json.ToString());
            }
        }

        private static ProcDefBlock ToProcDef(JToken json)
        {
            ProcDefBlock b = new ProcDefBlock();
            string procName = json[1][0].ToString();
            List<string> argNames = new List<string>();
            List<DataType> argTypes = new List<DataType>();
            for (int i = 1; i < json[1].Count(); i += 2)
            {
                argNames.Add(json[1][i].ToString());
                argTypes.Add(DataTypeNames.TypeOf(json[1][i + 1].ToString()));
            }
            string[] bits = procName.SplitFuncArgs();
            int argCount = 0;
            for (int i = 0; i < bits.Length; ++i)
            {
                string bit = bits[i];
                if (bit == "%")
                {
                    b.AddBit(new VarDefBlock(argNames[argCount], argTypes[argCount]));
                    argCount++;
                }
                else
                {
                    b.AddBit(new ProcDefTextBit(bit));
                }
            }
            return b;
        }

        private IBlock ToVarAccess(JToken json)
        {
            string varName = json[1].ToString();
            if (currentProcDef == null)
                throw new InvalidOperationException(string.Format("Variable {0} used outside of proc definition", varName));
            if (!currentProcDef.GetArgNames().Contains(varName))
                throw new InvalidOperationException(string.Format("Variable {0} undefined", varName));
            VarDefBlock vdb = currentProcDef.GetArg(varName);
            VarAccessBlock b = new VarAccessBlock(vdb);
            return b;
        }

        private IBlock ToBlockStack(JToken json)
        {
            BlockStack b = new BlockStack();

            for (int i = 1; i < json.Count(); ++i)
            {
                IBlock subBlock = ToBlock(json[i]);
                if (i == 1 && subBlock is ProcDefBlock)
                    currentProcDef = subBlock as ProcDefBlock;
                b.Add(subBlock);
            }
            currentProcDef = null;
            return b;
        }
        public InvokationBlock ToInvokationBlock(JToken json)
        {
            string name = json[0].ToString();
            string typeFingerPrint = json[1].ToString();
            string retTypeName = json[2].ToString();
            TypeCheck(name, typeFingerPrint);
            DataType[] argTypes = DataTypeNames.DecodeFingerprint(typeFingerPrint);
            DataType retType = DataTypeNames.TypeOf(retTypeName);
            List<IBlock> args = new List<IBlock>();
            for (int i = 3; i < json.Count(); ++i)
            {
                args.Add(ToBlock(json[i]));
            }
            BlockAttributes attr;
            if (!blockSpace.RegisteredMethodAttribute(name, out attr))
            {
                attr = BlockAttributes.Stack;
            }
            InvokationBlock ib = new InvokationBlock(name, attr, argTypes, retType);
            ib.Args.AddRange(args.ToArray(), argTypes.ToArray());
            return ib;
        }

        private void TypeCheck(string name, string typeFingerPrint)
        {
            if (procDeclarations.ContainsKey(name))
            {
                string fp2 = procDeclarations[name];
                if (typeFingerPrint != fp2)
                    throw new TypeException(string.Format("Procedure {0} used as taking '{1}', but previously used as taking {2}",
                        name, typeFingerPrint, fp2));
            }
            else
            {
                procDeclarations[name] = typeFingerPrint;
            }
        }
    }
}
