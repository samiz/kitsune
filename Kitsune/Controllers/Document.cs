using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Kitsune
{
    public delegate void TitleChangeEvent(object sender, string newTitle);
    public class Document
    {
        public event TitleChangeEvent TitleChange;
        string _Filename;
        bool _Dirty;
        bool _NewFile;
        string titlebase;
        public Document(string titlebase)
        {
            _Filename = "";
            _Dirty = false;
            _NewFile = true;
            this.titlebase = titlebase;
            this.TitleChange += delegate(object sender, string newTitle) { };
        }

        public void Modify() 
        {
            _Dirty = true;
            SetTitle();
        }

        public void SetTitle()
        {
            string title = titlebase;
            if (_Filename != "")
            {
                title += " - " + Path.GetFileName(_Filename);
            }
            else
            {
                title += " - " + "(untitled)";
            }
            if (_Dirty)
                title += "*";
            TitleChange(this, title);
        }
        public void Save(string filename)
        {
            _Dirty = _NewFile = false;
            _Filename = filename;
            SetTitle();
        }
        public void New()
        {
            _Dirty = false;
            _NewFile = true;
            _Filename = "";
            SetTitle();
        }
        public void Load(string filename)
        {
            _Dirty = _NewFile = false;
            _Filename = filename;
            SetTitle();
        }
        public bool NeedtoSave(out bool askForFile)
        {
            if (!_Dirty)
            {
                askForFile = false;
                return false;
            }
            
            askForFile = _NewFile;
            return true;
        }
        public void ActWhenClosing(out bool save, out bool exit, out string fileName)
        {
            bool askForFile;
            if (!NeedtoSave(out askForFile))
            {
                fileName = "";
                save = false;
                exit = true;
                return;
            }

            DialogResult dr = MessageBox.Show(string.Format("File {0} has not yet been saved! Save before quitting?", _Filename),
                "Modified file", MessageBoxButtons.YesNoCancel);
            if (dr == DialogResult.Yes)
            {
                if (!_NewFile)
                {
                    fileName = this._Filename;
                    save = true;
                    exit = true;
                    return;
                }
                else
                {
                    SaveFileDialog dlg = new SaveFileDialog();
                    DialogResult dr2 = dlg.ShowDialog();
                    if (dr2 == DialogResult.OK)
                    {
                        fileName = dlg.FileName;
                        save = true;
                        exit = true;
                        return;
                    }
                    else
                    {
                        fileName = "";
                        save = false;
                        exit = false;
                        return;
                    }
                }
                
            }
            else if (dr == DialogResult.No)
            {
                fileName = "";
                save = false;
                exit = true;
                return;
            }
            else
            {
                fileName = "";
                save = false;
                exit = false;
                return;
            }
        }
        public bool Newfile { get { return _NewFile; } }
        public bool Dirty { get { return _Dirty; } }
        public string Filename { get { return _Filename; } }
    }
}
