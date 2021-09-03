using EvoCodeGen.Core.Models;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace EvoCodeGen
{
    public class EvoCodeGenWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private CollectionView generators;
        public CollectionView Generators
        {
            get => this.generators; set
            {
                this.generators = value;
                OnPropertyChanged("Generators");
            }
        }

        private EvoCodeGenerator generator;
        public EvoCodeGenerator Generator
        {
            get => this.generator; set
            {
                this.generator = value;
                OnPropertyChanged("Generator");
            }
        }

        private string modelName; 
        public string ModelName
        {
            get => this.modelName; set
            {
                this.modelName = value;
                OnPropertyChanged("ModelName");
            }
        }

        private string jsonModel;
        public string JsonModel
        {
            get => this.jsonModel; set
            {
                this.jsonModel = value;
                OnPropertyChanged("JsonModel");
            }
        }

        private CollectionView templates;
        public CollectionView Templates
        {
            get => this.templates; set
            {
                this.templates = value;
                OnPropertyChanged("Templates");
            }
        }

        public Action<bool> GenerateCode;
        private DelegateCommand _generateCodeCommand;
        public bool HasErrors { get; set; } = false;
        public ICommand GenerateCodeCommand
        {
            get
            {
                if (_generateCodeCommand == null)
                {
                    _generateCodeCommand = new DelegateCommand(_ =>
                    {
                        Validate(propertyName: null);

                        if (!HasErrors)
                        {
                            GenerateCode(true);
                        }
                    });
                }

                return _generateCodeCommand;
            }
        }

        public string SelectFolder { get; set; }
        public object RootNamespace { get; set; }
        public string ProjectFolder { get; set; }

        private void Validate(string propertyName)
        {
            //if (string.IsNullOrEmpty(this.ServiceClass) ||
            //    string.IsNullOrEmpty(this.IServiceClass) ||
            //    string.IsNullOrEmpty(this.ModelName) ||
            //    string.IsNullOrEmpty(this.DtoClass)
            //    )
            if (string.IsNullOrEmpty(this.JsonModel))
            {
                this.HasErrors = true;
            }
            else
            {
                this.HasErrors = false;
            }

        }
    }
}
